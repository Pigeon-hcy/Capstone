using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Builds an extruded 3D Mesh from a Tilemap.
/// Algorithm:
///   1. Collect all filled cell positions.
///   2. Walk the outer boundary using a clockwise edge-marching approach
///      to produce one or more closed contour loops.
///   3. Triangulate each loop (ear-clipping) to create front/back caps.
///   4. Stitch adjacent boundary edges into quads for the side wall.
/// </summary>
public static class TilemapMeshBuilder
{
    // ── Cardinal directions used during contour tracing ──────────────────
    private static readonly Vector2Int[] Dirs = {
        Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down
    };

    // ─────────────────────────────────────────────────────────────────────
    //  Public entry point
    // ─────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Generate a 3-D extruded mesh from <paramref name="tilemap"/>.
    /// </summary>
    /// <param name="tilemap">Source tilemap.</param>
    /// <param name="depth">Extrusion depth along the Z axis (positive = forward).</param>
    /// <returns>Combined mesh, or null if the tilemap is empty.</returns>
    public static Mesh Build(Tilemap tilemap, float depth)
    {
        // 1. Gather filled cells -------------------------------------------
        var filled = CollectFilledCells(tilemap);
        if (filled.Count == 0) return null;

        // 2. Extract boundary edge loops ------------------------------------
        var loops = ExtractContourLoops(filled);
        if (loops.Count == 0) return null;

        // 3. Build mesh -----------------------------------------------------
        var verts   = new List<Vector3>();
        var tris    = new List<int>();
        var uvs     = new List<Vector2>();
        var normals = new List<Vector3>();

        float halfD = depth * 0.5f;

        foreach (var loop in loops)
        {
            if (loop.Count < 3) continue;

            int baseIdx = verts.Count;

            // --- Front cap (z = -halfD, normal points towards -Z) ----------
            foreach (var p in loop)
            {
                verts.Add(new Vector3(p.x, p.y, -halfD));
                uvs.Add(new Vector2(p.x, p.y));
                normals.Add(Vector3.back);
            }

            // --- Back cap (z = +halfD, normal points towards +Z) -----------
            foreach (var p in loop)
            {
                verts.Add(new Vector3(p.x, p.y, halfD));
                uvs.Add(new Vector2(p.x, p.y));
                normals.Add(Vector3.forward);
            }

            int n        = loop.Count;
            int frontOff = baseIdx;
            int backOff  = baseIdx + n;

            // --- Triangulate front cap (CCW when viewed from -Z) -----------
            var frontTris = EarClip(loop);
            foreach (var t in frontTris)
                tris.Add(frontOff + t);

            // --- Triangulate back cap (CW = flip winding) ------------------
            foreach (var t in frontTris)
                tris.Add(backOff + t);           // collected below then reversed
            // Rewrite: add back-cap with reversed winding
            tris.RemoveRange(tris.Count - frontTris.Count, frontTris.Count);
            for (int i = 0; i < frontTris.Count; i += 3)
            {
                tris.Add(backOff + frontTris[i]);
                tris.Add(backOff + frontTris[i + 2]);
                tris.Add(backOff + frontTris[i + 1]);
            }

            // --- Side walls ------------------------------------------------
            int sideBase = verts.Count;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;

                Vector2 a = loop[i];
                Vector2 b = loop[j];

                // Per-quad normal (flat shading)
                Vector3 edge   = new Vector3(b.x - a.x, b.y - a.y, 0f);
                Vector3 normal = Vector3.Cross(edge, Vector3.forward).normalized;

                // 4 verts per quad
                int qi = verts.Count;
                verts.Add(new Vector3(a.x, a.y, -halfD));  // 0 front-left
                verts.Add(new Vector3(b.x, b.y, -halfD));  // 1 front-right
                verts.Add(new Vector3(b.x, b.y,  halfD));  // 2 back-right
                verts.Add(new Vector3(a.x, a.y,  halfD));  // 3 back-left

                float uA = i / (float)n;
                float uB = (i + 1) / (float)n;
                uvs.Add(new Vector2(uA, 0));
                uvs.Add(new Vector2(uB, 0));
                uvs.Add(new Vector2(uB, 1));
                uvs.Add(new Vector2(uA, 1));

                for (int k = 0; k < 4; k++) normals.Add(normal);

                // Two triangles (CCW from outside)
                tris.Add(qi); tris.Add(qi + 1); tris.Add(qi + 2);
                tris.Add(qi); tris.Add(qi + 2); tris.Add(qi + 3);
            }
        }

        // 4. Assemble Mesh --------------------------------------------------
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
    //  Step 1 – collect filled cells
    // ─────────────────────────────────────────────────────────────────────
    private static HashSet<Vector2Int> CollectFilledCells(Tilemap tilemap)
    {
        var set = new HashSet<Vector2Int>();
        tilemap.CompressBounds();
        var bounds = tilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                set.Add(new Vector2Int(pos.x, pos.y));
        }
        return set;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Step 2 – extract contour loops
    //  Each "boundary edge" separates a filled cell from an empty one.
    //  We build a directed half-edge graph and walk closed loops.
    // ─────────────────────────────────────────────────────────────────────
    private static List<List<Vector2>> ExtractContourLoops(HashSet<Vector2Int> filled)
    {
        // A boundary edge is stored as (from, to) in grid-vertex space.
        // Grid vertices are the corners of cells: cell (cx,cy) has corners
        //   (cx,cy), (cx+1,cy), (cx+1,cy+1), (cx,cy+1).
        //
        // For each filled cell, check 4 neighbours.
        // If a neighbour is empty, that shared edge is a boundary edge.
        // Boundary edge direction is chosen so the filled side is always
        // to the LEFT of the directed edge (right-hand rule → CCW outline).

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
            // Check each of the 4 sides
            // Right neighbour empty  → edge from (cx+1,cy) to (cx+1,cy+1)
            if (!filled.Contains(new Vector2Int(cx + 1, cy)))
                AddEdge(new Vector2Int(cx + 1, cy), new Vector2Int(cx + 1, cy + 1));
            // Left neighbour empty   → edge from (cx,cy+1) to (cx,cy)
            if (!filled.Contains(new Vector2Int(cx - 1, cy)))
                AddEdge(new Vector2Int(cx, cy + 1), new Vector2Int(cx, cy));
            // Top neighbour empty    → edge from (cx+1,cy+1) to (cx,cy+1)
            if (!filled.Contains(new Vector2Int(cx, cy + 1)))
                AddEdge(new Vector2Int(cx + 1, cy + 1), new Vector2Int(cx, cy + 1));
            // Bottom neighbour empty → edge from (cx,cy) to (cx+1,cy)
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

                    next = nexts[0]; // single-exit half-edge graph
                }

                if (loop.Count >= 3)
                    loops.Add(loop);
            }
        }

        return loops;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Step 3 – Ear-Clipping triangulation (simple polygon, no holes)
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
            if (!earFound) break; // degenerate polygon guard
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

        // Must be a convex vertex
        if (Cross2D(a, b, c) < 0f) return false;

        // No other vertex inside triangle abc
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
