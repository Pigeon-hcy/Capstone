using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Builds an extruded 3D Mesh from a Tilemap.
/// Algorithm:
///   1. Collect all filled cell positions, separating solid blocks from slope tiles.
///   2. Walk the outer boundary of solid blocks using a clockwise edge-marching approach
///      to produce one or more closed contour loops.
///   3. Triangulate each loop (ear-clipping) to create front/back caps.
///   4. Stitch adjacent boundary edges into quads for the side wall.
///   5. For slope tiles, extrude each tile's sprite physics shape individually.
/// </summary>
public static class TilemapMeshBuilder
{
    // ── Cardinal directions used during contour tracing ──────────────────
    private static readonly Vector2Int[] Dirs = {
        Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down
    };

    // ── Slope detection threshold ────────────────────────────────────────
    // A tile whose physics-shape area is less than this fraction of a full
    // cell is considered a slope / non-rectangular tile.
    private const float RectAreaThreshold = 0.95f;

    // ─────────────────────────────────────────────────────────────────────
    //  Public entry point
    // ─────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Generate a 3-D extruded mesh from <paramref name="tilemap"/>.
    /// Solid rectangular tiles are merged via contour extraction.
    /// Slope / non-rectangular tiles are extruded individually from their
    /// sprite physics shapes.
    /// </summary>
    public static Mesh Build(Tilemap tilemap, float depth)
    {
        // 1. Gather cells, separating solid vs slope -------------------------
        var solidCells = new HashSet<Vector2Int>();
        var slopeCells = new Dictionary<Vector2Int, List<Vector2>>();
        CollectAndClassifyCells(tilemap, solidCells, slopeCells);

        if (solidCells.Count == 0 && slopeCells.Count == 0) return null;

        // Shared mesh buffers
        var verts   = new List<Vector3>();
        var tris    = new List<int>();
        var uvs     = new List<Vector2>();
        var normals = new List<Vector3>();

        float halfD = depth * 0.5f;

        // 2. Pass 1 – solid blocks (existing contour algorithm) --------------
        if (solidCells.Count > 0)
        {
            var loops = ExtractContourLoops(solidCells);
            foreach (var loop in loops)
                ExtrudeLoop(loop, halfD, verts, tris, uvs, normals);
        }

        // 3. Pass 2 – slope tiles (per-tile extrusion) -----------------------
        foreach (var kvp in slopeCells)
        {
            var polygon = kvp.Value;
            if (polygon.Count >= 3)
                ExtrudeLoop(polygon, halfD, verts, tris, uvs, normals);
        }

        if (verts.Count == 0) return null;

        // 4. Assemble Mesh ---------------------------------------------------
        var mesh = new Mesh { name = "TilemapExtruded" };
        mesh.indexFormat = verts.Count > 65535
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds();

        return mesh;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Step 1 – collect and classify cells
    // ─────────────────────────────────────────────────────────────────────
    private static void CollectAndClassifyCells(
        Tilemap tilemap,
        HashSet<Vector2Int> solidCells,
        Dictionary<Vector2Int, List<Vector2>> slopeCells)
    {
        tilemap.CompressBounds();
        var bounds = tilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            var cell = new Vector2Int(pos.x, pos.y);
            Sprite sprite = tilemap.GetSprite(pos);

            // Try to read sprite physics shape
            List<Vector2> shape = null;
            if (sprite != null && sprite.GetPhysicsShapeCount() > 0)
            {
                var raw = new List<Vector2>();
                sprite.GetPhysicsShape(0, raw);
                if (raw.Count >= 3 && !IsRectangularShape(raw))
                {
                    // Transform shape by tile matrix (handles rotation/flip)
                    Matrix4x4 tileMatrix = tilemap.GetTransformMatrix(pos);
                    shape = TransformAndOffsetShape(raw, tileMatrix, cell);
                }
            }

            if (shape != null)
                slopeCells[cell] = shape;
            else
                solidCells.Add(cell);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Shape classification helpers
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the physics shape is approximately a full-cell
    /// rectangle (4 corners, area ≥ threshold).
    /// </summary>
    private static bool IsRectangularShape(List<Vector2> shape)
    {
        if (shape.Count != 4) return false;

        float area = Mathf.Abs(SignedArea(shape));
        // A full tile sprite physics shape covers ~1.0 square units
        return area >= RectAreaThreshold;
    }

    /// <summary>
    /// Apply the tile's transform matrix to physics-shape points and
    /// offset them into world grid space (cell bottom-left + 0.5 center).
    /// </summary>
    private static List<Vector2> TransformAndOffsetShape(
        List<Vector2> raw, Matrix4x4 tileMatrix, Vector2Int cell)
    {
        // Sprite physics shape coords are relative to sprite center (0,0).
        // Cell center in world space is (cell.x + 0.5, cell.y + 0.5).
        float cx = cell.x + 0.5f;
        float cy = cell.y + 0.5f;

        var result = new List<Vector2>(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            Vector3 transformed = tileMatrix.MultiplyPoint3x4(raw[i]);
            result.Add(new Vector2(transformed.x + cx, transformed.y + cy));
        }
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Shared extrusion: front cap + back cap + side walls
    //  Works for both contour loops (solid blocks) and individual slope
    //  polygons.
    // ─────────────────────────────────────────────────────────────────────
    private static void ExtrudeLoop(
        List<Vector2> loop, float halfD,
        List<Vector3> verts, List<int> tris,
        List<Vector2> uvs, List<Vector3> normals)
    {
        if (loop.Count < 3) return;

        int baseIdx = verts.Count;

        // --- Front cap (z = -halfD, normal towards -Z) ---
        foreach (var p in loop)
        {
            verts.Add(new Vector3(p.x, p.y, -halfD));
            uvs.Add(new Vector2(p.x, p.y));
            normals.Add(Vector3.back);
        }

        // --- Back cap (z = +halfD, normal towards +Z) ---
        foreach (var p in loop)
        {
            verts.Add(new Vector3(p.x, p.y, halfD));
            uvs.Add(new Vector2(p.x, p.y));
            normals.Add(Vector3.forward);
        }

        int n        = loop.Count;
        int frontOff = baseIdx;
        int backOff  = baseIdx + n;

        // --- Triangulate front cap (CCW when viewed from -Z) ---
        var frontTris = EarClip(loop);
        foreach (var t in frontTris)
            tris.Add(frontOff + t);

        // --- Back cap (reversed winding) ---
        for (int i = 0; i < frontTris.Count; i += 3)
        {
            tris.Add(backOff + frontTris[i]);
            tris.Add(backOff + frontTris[i + 2]);
            tris.Add(backOff + frontTris[i + 1]);
        }

        // --- Side walls ---
        float fullDepth = halfD * 2f;
        float cumulativeU = 0f;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;

            Vector2 a = loop[i];
            Vector2 b = loop[j];

            float edgeLen = Vector2.Distance(a, b);

            // Per-quad normal (flat shading)
            Vector3 edge   = new Vector3(b.x - a.x, b.y - a.y, 0f);
            Vector3 normal = Vector3.Cross(edge, Vector3.forward).normalized;

            // 4 verts per quad
            int qi = verts.Count;
            verts.Add(new Vector3(a.x, a.y, -halfD));  // 0 front-left
            verts.Add(new Vector3(b.x, b.y, -halfD));  // 1 front-right
            verts.Add(new Vector3(b.x, b.y,  halfD));  // 2 back-right
            verts.Add(new Vector3(a.x, a.y,  halfD));  // 3 back-left

            float uA = cumulativeU;
            float uB = cumulativeU + edgeLen;
            uvs.Add(new Vector2(uA, 0));
            uvs.Add(new Vector2(uB, 0));
            uvs.Add(new Vector2(uB, fullDepth));
            uvs.Add(new Vector2(uA, fullDepth));

            for (int k = 0; k < 4; k++) normals.Add(normal);

            // Two triangles (CCW from outside)
            tris.Add(qi); tris.Add(qi + 1); tris.Add(qi + 2);
            tris.Add(qi); tris.Add(qi + 2); tris.Add(qi + 3);

            cumulativeU += edgeLen;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Step 2 – extract contour loops
    //  Each "boundary edge" separates a filled cell from an empty one.
    //  We build a directed half-edge graph and walk closed loops.
    // ─────────────────────────────────────────────────────────────────────
    private static List<List<Vector2>> ExtractContourLoops(HashSet<Vector2Int> filled)
    {
        var edgeMap = new Dictionary<Vector2Int, List<Vector2Int>>();

        void AddEdge(Vector2Int from, Vector2Int to)
        {
            if (!edgeMap.TryGetValue(from, out var list))
            {
                list = new List<Vector2Int>();
                edgeMap[from] = list;
            }
            list.Add(to);
        }

        foreach (var c in filled)
        {
            int cx = c.x, cy = c.y;
            if (!filled.Contains(new Vector2Int(cx + 1, cy)))
                AddEdge(new Vector2Int(cx + 1, cy), new Vector2Int(cx + 1, cy + 1));
            if (!filled.Contains(new Vector2Int(cx - 1, cy)))
                AddEdge(new Vector2Int(cx, cy + 1), new Vector2Int(cx, cy));
            if (!filled.Contains(new Vector2Int(cx, cy + 1)))
                AddEdge(new Vector2Int(cx + 1, cy + 1), new Vector2Int(cx, cy + 1));
            if (!filled.Contains(new Vector2Int(cx, cy - 1)))
                AddEdge(new Vector2Int(cx, cy), new Vector2Int(cx + 1, cy));
        }

        // Walk loops
        var visited = new HashSet<(Vector2Int, Vector2Int)>();
        var loops   = new List<List<Vector2>>();

        foreach (var kvp in edgeMap)
        {
            var startFrom = kvp.Key;
            foreach (var startTo in kvp.Value)
            {
                if (visited.Contains((startFrom, startTo))) continue;

                var loop = new List<Vector2>();
                var cur  = startFrom;
                var next = startTo;

                int safety = 100000;
                while (!visited.Contains((cur, next)) && safety-- > 0)
                {
                    visited.Add((cur, next));
                    loop.Add(new Vector2(cur.x, cur.y));
                    cur = next;

                    if (!edgeMap.TryGetValue(cur, out var nexts) || nexts.Count == 0)
                        break;

                    next = nexts[0];
                }

                if (loop.Count >= 3)
                    loops.Add(loop);
            }
        }

        return loops;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Ear-Clipping triangulation (simple polygon, no holes)
    // ─────────────────────────────────────────────────────────────────────
    private static List<int> EarClip(List<Vector2> polygon)
    {
        var result  = new List<int>();
        var indices = new List<int>();
        for (int i = 0; i < polygon.Count; i++) indices.Add(i);

        // Ensure CCW winding for correct front-face normals
        if (SignedArea(polygon) < 0) indices.Reverse();

        int maxIter = polygon.Count * polygon.Count + 10;
        int iter    = 0;

        while (indices.Count > 3 && iter++ < maxIter)
        {
            bool earFound = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prev = indices[(i - 1 + indices.Count) % indices.Count];
                int curr = indices[i];
                int next = indices[(i + 1) % indices.Count];

                if (IsEar(polygon, prev, curr, next, indices))
                {
                    result.Add(prev);
                    result.Add(curr);
                    result.Add(next);
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound) break;
        }

        if (indices.Count == 3)
        {
            result.Add(indices[0]);
            result.Add(indices[1]);
            result.Add(indices[2]);
        }

        return result;
    }

    private static float SignedArea(List<Vector2> poly)
    {
        float area = 0f;
        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            area += (a.x * b.y) - (b.x * a.y);
        }
        return area * 0.5f;
    }

    private static bool IsEar(List<Vector2> poly, int prev, int curr, int next,
                               List<int> remaining)
    {
        Vector2 a = poly[prev], b = poly[curr], c = poly[next];

        if (Cross2D(a, b, c) < 0f) return false;

        foreach (int idx in remaining)
        {
            if (idx == prev || idx == curr || idx == next) continue;
            if (PointInTriangle(poly[idx], a, b, c)) return false;
        }
        return true;
    }

    private static float Cross2D(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross2D(p, a, b);
        float d2 = Cross2D(p, b, c);
        float d3 = Cross2D(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }
}
