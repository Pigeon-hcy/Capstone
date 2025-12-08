using UnityEngine;

namespace RampCreation
{
    [System.Serializable]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class RampBuilder : MonoBehaviour
    {
        [SerializeField] RampType rampType;
        [SerializeField] SegmentData[] segments;

        [Header("General Specs")]
        [SerializeField] float rampCurvature = 1;
        [Range(1, 100)]
        [SerializeField] int rampTiles = 20;
        [SerializeField] float width = 1;
        [Range(0.1f, 90)]
        [SerializeField] float endAngle = 30;

        [Header("Tabletop Specs")]
        [SerializeField] float tableTopLength = 1;

        [Header("Hill Specs")]

        [SerializeField] float topCurvature = 1;
        [Header("Launch Specs")]
        [SerializeField] float flatTakeOffLength = 1;
        [SerializeField] float deckDepth = 1;

        // vector perpendicular to the ramp origin in the direction of the circle center,
        // the x component must be 0
        Vector3 normal;
        Vector3 origin;

        public Vector3 parabolaOrigin, parabolaForward;

        // Status variables
        int vertexIndex;
        float maxLen, currentLen, maxHeight, maxForward;
        float rampUvStart;
        Vector3[] objectVertices;
        int[] objectTriangles;
        Vector2[] objectUV;

        // Public methods
        public void BuildRampSegments()
        {
            /// Build the mesh and update the MeshFilter and MeshCollider components.
            Initialize();
            BuildMesh();
            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mesh.Clear();
            mesh.vertices = objectVertices;
            mesh.triangles = objectTriangles;
            mesh.uv = objectUV;
            mesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public Vector3[] GetFlightPath(float launchSpeed, float drag, float timeStep, float flightDuration)
        {
            /// Get the flight path of a projectile launched from the ramp.

            int steps = (int)Mathf.Floor(flightDuration / timeStep);
            Vector3[] flightPath = new Vector3[steps];

            Vector3 velocity = parabolaForward * launchSpeed;
            Vector3 position = parabolaOrigin;
            // Change position from local coordiantes to world coordinates
            flightPath[0] = transform.TransformPoint(position);
            for (int i = 1; i < steps; i++)
            {
                // Runge-Kutta 4th order    
                Vector3 k1v = (Physics.gravity - velocity * drag) * timeStep;
                Vector3 k1p = velocity * timeStep;
                Vector3 k2v = (Physics.gravity - (velocity + 0.5f * k1v) * drag) * timeStep;
                Vector3 k2p = (velocity + 0.5f * k1v) * timeStep;
                Vector3 k3v = (Physics.gravity - (velocity + 0.5f * k2v) * drag) * timeStep;
                Vector3 k3p = (velocity + 0.5f * k2v) * timeStep;
                Vector3 k4v = (Physics.gravity - (velocity + k3v) * drag) * timeStep;
                Vector3 k4p = (velocity + k3v) * timeStep;
                velocity += (k1v + 2f * k2v + 2f * k3v + k4v) / 6f;
                position += (k1p + 2f * k2p + 2f * k3p + k4p) / 6f;

                flightPath[i] = transform.TransformPoint(position);
            }
            return flightPath;
        }

        void OnValidate()
        {
            if (rampCurvature <= 0)
            {
                rampCurvature = 0.1f;
            }
            if (width <= 0)
            {
                width = 0.1f;
            }
            if (topCurvature <= 0)
            {
                topCurvature = 0.1f;
            }
            if (flatTakeOffLength > 0 && endAngle == 90 && deckDepth == 0)
            {
                deckDepth = 0.1f;
            }
            if (tableTopLength < 0){
                tableTopLength = 0;
            }
            if (deckDepth < 0)
            {
                deckDepth = 0;
            }
            if (flatTakeOffLength < 0)
            {
                flatTakeOffLength = 0;
            }
        }

        void Initialize()
        {
            normal = Vector3.up;
            origin = Vector3.zero;
            objectVertices = new Vector3[0];
            objectTriangles = new int[0];
            objectUV = new Vector2[0];
            vertexIndex = 0;
            currentLen = 0;

            parabolaForward = Quaternion.AngleAxis(-endAngle, Vector3.right) * Vector3.forward;
            // Vector3 parabolaUp = Quaternion.AngleAxis(-90, Vector3.right) * parabolaForward;

            switch (rampType)
            {
                case RampType.OneSided:
                    segments = new SegmentData[1]{
                    new SegmentData(rampCurvature, endAngle, rampTiles)
                };
                    maxLen = rampCurvature * Mathf.Deg2Rad * endAngle;
                    maxHeight = rampCurvature - rampCurvature * Mathf.Cos(Mathf.Deg2Rad * endAngle);
                    maxForward = rampCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle);
                    if (flatTakeOffLength > 0 && (deckDepth != 0 || endAngle != 90))
                    {
                        segments = segments.Concat(new SegmentData[1]{
                        new SegmentData(flatTakeOffLength, endAngle)
                    });
                        maxLen += flatTakeOffLength;
                        maxHeight += flatTakeOffLength * Mathf.Sin(Mathf.Deg2Rad * endAngle);
                        maxForward += flatTakeOffLength * Mathf.Cos(Mathf.Deg2Rad * endAngle);
                    }
                    parabolaOrigin = new Vector3(0, maxHeight, maxForward);// + parabolaUp*0.1f;
                    if (deckDepth != 0)
                    { // Depth is a new UV
                        segments = segments.Concat(new SegmentData[1]{
                            new SegmentData(deckDepth, 0)
                        });
                        maxForward += deckDepth;
                    }
                    rampUvStart = maxForward * Mathf.Sin(endAngle) / (2 * maxForward * Mathf.Sin(endAngle) + width);
                    break;
                case RampType.Tabletop:
                    segments = new SegmentData[3]{
                    new SegmentData(rampCurvature, endAngle, rampTiles),
                    new SegmentData(tableTopLength, 0),
                    new SegmentData(rampCurvature, endAngle, rampTiles)
                };
                    maxLen = 2 * rampCurvature * Mathf.Deg2Rad * endAngle + tableTopLength;
                    maxHeight = rampCurvature - rampCurvature * Mathf.Cos(Mathf.Deg2Rad * endAngle);
                    maxForward = 2 * rampCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle) + tableTopLength;
                    parabolaOrigin = new Vector3(0, maxHeight, rampCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle));
                    rampUvStart = maxHeight / (2 * maxHeight + width);
                    break;
                case RampType.Hill:
                    segments = new SegmentData[3] {
                        new SegmentData(rampCurvature, endAngle, rampTiles),
                        new SegmentData(topCurvature, -2*endAngle, rampTiles),
                        new SegmentData(rampCurvature, endAngle, rampTiles),
                    };
                    maxLen = 2 * rampCurvature * Mathf.Deg2Rad * endAngle + topCurvature * Mathf.Deg2Rad * 2 * endAngle;
                    maxHeight = rampCurvature - rampCurvature * Mathf.Cos(Mathf.Deg2Rad * endAngle) + topCurvature - topCurvature * Mathf.Cos(Mathf.Deg2Rad * endAngle);
                    maxForward = 2 * rampCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle) + topCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle);
                    rampUvStart = maxHeight / (2 * maxHeight + width);
                    parabolaOrigin = new Vector3(0, rampCurvature - rampCurvature * Mathf.Cos(Mathf.Deg2Rad * endAngle), rampCurvature * Mathf.Sin(Mathf.Deg2Rad * endAngle));
                    break;

            }
        }

        void BuildMesh()
        {
            // Build top faces
            Vector3[] vertices;
            int[] triangles;
            Vector2[] uv;
            int i = 0;
            foreach (SegmentData segment in segments)
            {
                switch (segment.segmentType)
                {
                    case SegmentType.Ramp:
                        (vertices, triangles, uv) = BuildRamp(segment);
                        break;
                    case SegmentType.Flat:

                        (vertices, triangles, uv) = BuildFlat(segment);
                        break;
                    default:
                        (vertices, triangles, uv) = (new Vector3[0], new int[0], new Vector2[0]);
                        break;
                }
                i++;
                objectVertices = objectVertices.Concat(vertices);
                objectTriangles = objectTriangles.Concat(triangles);
                objectUV = objectUV.Concat(uv);
            }

            // Build sides
            switch (rampType)
            {
                case RampType.Tabletop:
                    (vertices, triangles, uv) = BuildSymmetricFaces(rampTiles);
                    break;
                case RampType.Hill:
                    (vertices, triangles, uv) = BuildSymmetricFaces((int)Mathf.Floor(rampTiles * 1.5f));
                    break;
                case RampType.OneSided:
                    (vertices, triangles, uv) = BuildRampSides();
                    break;
                default:
                    (vertices, triangles, uv) = (new Vector3[0], new int[0], new Vector2[0]);
                    break;
            }
            objectTriangles = objectTriangles.Concat(triangles);
            objectVertices = objectVertices.Concat(vertices);
            objectUV = objectUV.Concat(uv);

            if (rampType == RampType.OneSided)
            {
                if (deckDepth > 0)
                {
                    (vertices, triangles, uv) = BuildDeck();
                }
                else
                {
                    (vertices, triangles, uv) = BuildBackSimpleRamp();
                }
                objectTriangles = objectTriangles.Concat(triangles);
                objectVertices = objectVertices.Concat(vertices);
                objectUV = objectUV.Concat(uv);
            }

        }

        (Vector3[], int[], Vector2[]) BuildDeck()
        {
            bool hasTakeOffRamp = flatTakeOffLength > 0 && (deckDepth != 0 || endAngle != 90);
            int topLeftIndex = rampTiles * 2 + (hasTakeOffRamp ? 2 : 0);
            Vector3[] vertices = new Vector3[8];
            Vector2[] uv = new Vector2[8];
            int[] triangles = new int[0];
            float maxUV = deckDepth * 2 + width;

            // top face 
            vertices[0] = objectVertices[topLeftIndex]; // back left 
            vertices[1] = objectVertices[topLeftIndex + 1]; // back right
            vertices[2] = objectVertices[topLeftIndex] + new Vector3(0, 0, deckDepth); // front left
            vertices[3] = objectVertices[topLeftIndex + 1] + new Vector3(0, 0, deckDepth); // front right

            uv[0] = new Vector2(0, 1);
            uv[1] = new Vector2(1, 1);
            uv[2] = new Vector2(deckDepth / maxUV, 1);
            uv[3] = new Vector2((deckDepth + width) / maxUV, 1);

            // bottom face 
            vertices[4] = vertices[0] + new Vector3(0, -maxHeight, 0); // back left
            vertices[5] = vertices[1] + new Vector3(0, -maxHeight, 0); ; // back right
            vertices[6] = vertices[2] + new Vector3(0, -maxHeight, 0); ; // front left
            vertices[7] = vertices[3] + new Vector3(0, -maxHeight, 0); ; // front right

            uv[4] = uv[0] + new Vector2(0, -1);
            uv[5] = uv[1] + new Vector2(0, -1);
            uv[6] = uv[2] + new Vector2(0, -1);
            uv[7] = uv[3] + new Vector2(0, -1);

            //BuildRectangle()
            triangles = triangles.Concat(BuildRectangle(vertexIndex + 6, vertexIndex + 2, vertexIndex + 0, vertexIndex + 4));
            triangles = triangles.Concat(BuildRectangle(vertexIndex + 7, vertexIndex + 3, vertexIndex + 2, vertexIndex + 6));
            triangles = triangles.Concat(BuildRectangle(vertexIndex + 3, vertexIndex + 7, vertexIndex + 5, vertexIndex + 1));

            return (vertices, triangles, uv);
        }

        (Vector3[], int[], Vector2[]) BuildBackSimpleRamp()
        {
            bool hasTakeOffRamp = flatTakeOffLength > 0 && (deckDepth != 0 || endAngle != 90);
            Vector3[] vertices = new Vector3[2]{
            new Vector3(-width/2, 0, maxForward), new Vector3(width/2, 0, maxForward)
        };
            Vector2[] uv = new Vector2[2]{
            new Vector2(rampUvStart, 1+maxHeight/maxLen), new Vector2(1 - rampUvStart, 1+maxHeight/maxLen)
        };
            int topLeftIndex = rampTiles * 2 + (hasTakeOffRamp ? 2 : 0);
            int[] triangles = BuildRectangle(vertexIndex + 1, topLeftIndex + 1, topLeftIndex, vertexIndex);
            vertexIndex += 2;
            return (vertices, triangles, uv);
        }

        (Vector3[], int[], Vector2[]) BuildRampSides()
        {
            bool hasTakeOffRamp = flatTakeOffLength > 0 && (deckDepth != 0 || endAngle != 90);
            bool hasDeck = deckDepth > 0;
            Vector3[] vertices = new Vector3[((rampTiles + 1) + 1 + (hasTakeOffRamp ? 1 : 0)) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(rampTiles + (hasTakeOffRamp ? 1 : 0)) * 3 * 2];

            int fanVertixIndex = vertexIndex;
            float offset = hasDeck ? -deckDepth : 0;
            int tris = 0;
            int vert = 0;
            for (int j = 0; j < 2; j++)
            {
                bool isLeftSide = j == 0;
                // Create fanVertix
                vertices[vert] = new Vector3(-width / 2 + j * width, 0, maxForward + offset);
                uv[vert] = new Vector2(j, maxForward * Mathf.Cos(endAngle) / maxLen);
                vert++;
                for (int i = 0; i < rampTiles + (hasTakeOffRamp ? 1 : 0); i++)
                {
                    triangles[tris + 0] = fanVertixIndex;

                    float smallLen = rampCurvature * i / rampTiles * endAngle * Mathf.Deg2Rad / maxLen;
                    float bigLen = rampCurvature * (i + 1) / rampTiles * endAngle * Mathf.Deg2Rad / maxLen;
                    if (hasTakeOffRamp && i == rampTiles)
                    {
                        bigLen = 1.0f;
                    }

                    if (i == 0)
                    {
                        vertices[vert] = objectVertices[2 * i + j];
                        uv[vert] = new Vector2(j == 0 ? rampUvStart : 1 - rampUvStart, smallLen);
                        vert++;
                    }
                    vertices[vert] = objectVertices[2 * (i + 1) + j];
                    uv[vert] = new Vector2(j == 0 ? rampUvStart : 1 - rampUvStart, bigLen);
                    vert++;

                    triangles[tris + 1] = vert + vertexIndex - 2 + (isLeftSide ? 1 : 0);
                    triangles[tris + 2] = vert + vertexIndex - 1 - (isLeftSide ? 1 : 0);

                    tris += 3;
                }
                fanVertixIndex = vert + vertexIndex;
            }
            vertexIndex += vertices.Length;
            return (vertices, triangles, uv);
        }

        (Vector3[], int[], Vector2[]) BuildSymmetricFaces(int tiles)
        {
            Vector3[] vertices = new Vector3[(tiles + 1) * 2 * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[tiles * 6 * 2];

            int vert = 0;
            int tris = 0;
            for (int j = 0; j < 2; j++)
            {

                for (int i = 0; i < tiles; i++)
                {
                    Vector3 bl, tl, br, tr;
                    if (j == 0)
                    {
                        bl = objectVertices[vertexIndex - 2 - i * 2 + j];
                        tl = objectVertices[vertexIndex - 2 - (i + 1) * 2 + j];
                        br = objectVertices[i * 2 + j];
                        tr = objectVertices[(i + 1) * 2 + j];
                    }
                    else
                    {
                        bl = objectVertices[i * 2 + j];
                        tl = objectVertices[(i + 1) * 2 + j];
                        br = objectVertices[vertexIndex - 2 - i * 2 + j];
                        tr = objectVertices[vertexIndex - 2 - (i + 1) * 2 + j];
                    }
                    if (i == 0)
                    {
                        vertices[vert + 0] = bl;
                        vertices[vert + 1] = br;
                        uv[vert + 0] = new Vector2(j == 0 ? rampUvStart - (maxHeight - bl.y) / maxHeight * rampUvStart : 1 - rampUvStart + (maxHeight - bl.y) / maxHeight * rampUvStart,
                         bl.z / maxForward);
                        uv[vert + 1] = new Vector2(j == 0 ? rampUvStart - (maxHeight - br.y) / maxHeight * rampUvStart : 1 - rampUvStart + (maxHeight - br.y) / maxHeight * rampUvStart,
                         br.z / maxForward);
                        vert += 2;
                    }
                    vertices[vert + 0] = tl;
                    vertices[vert + 1] = tr;
                    uv[vert + 0] = new Vector2(j == 0 ? rampUvStart - (maxHeight - tl.y) / maxHeight * rampUvStart : 1 - rampUvStart + (maxHeight - tl.y) / maxHeight * rampUvStart,
                     tl.z / maxForward);
                    uv[vert + 1] = new Vector2(j == 0 ? rampUvStart - (maxHeight - tl.y) / maxHeight * rampUvStart :
                    1 - rampUvStart + (maxHeight - tr.y) / maxHeight * rampUvStart, tr.z / maxForward);

                    triangles[tris + 0] = vert - 2 + vertexIndex; // bl
                    triangles[tris + 1] = vert + vertexIndex; // tl
                    triangles[tris + 2] = vert - 1 + vertexIndex; // br
                    triangles[tris + 3] = vert - 1 + vertexIndex; // br
                    triangles[tris + 4] = vert + vertexIndex; // tl
                    triangles[tris + 5] = vert + 1 + vertexIndex; // tr
                    tris += 6;
                    vert += 2;
                }
            }
            return (vertices, triangles, uv);
        }

        (Vector3[], int[], Vector2[]) BuildRamp(SegmentData segment)
        {
            int rampTiles = segment.rampTiles;
            float rampRadius = segment.rampRadius;
            float rampEndAngle = segment.rampEndAngle;

            Vector3 center = origin + normal * rampRadius;
            Vector3 vec = -normal * rampRadius;

            Vector3[] vertices = new Vector3[rampTiles * 2 + (vertexIndex == 0 ? 2 : 0)];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[rampTiles * 6 + 6];
            float stepAngle = Mathf.Deg2Rad * rampEndAngle / rampTiles;

            int j = 0;
            for (int i = 0; i < rampTiles + 1; i++)
            {
                if (i > 0)
                {
                    vec = Quaternion.AngleAxis(stepAngle * Mathf.Rad2Deg, -Vector3.right) * vec;
                }
                else if (vertexIndex != 0)
                {
                    continue;
                }
                Vector3 point = center + vec;
                float z = point.z;
                float y = point.y;
                vertices[j * 2 + 0] = new Vector3(-width / 2, y, z);
                vertices[j * 2 + 1] = new Vector3(width / 2, y, z);
                float len = currentLen + i / (float)rampTiles * rampRadius * Mathf.Deg2Rad * rampEndAngle;
                uv[j * 2 + 0] = new Vector2(rampUvStart, len / maxLen);
                uv[j * 2 + 1] = new Vector2(1 - rampUvStart, len / maxLen);
                j++;
            }
            currentLen += rampRadius * Mathf.Deg2Rad * rampEndAngle;
            Vector3 lastPoint = vertices[vertices.Length - 1];
            if (rampType == RampType.Tabletop)
            {
                origin.z = lastPoint.z + tableTopLength;
                origin.y = lastPoint.y;
                Vector3 newCenter = new Vector3(center.x, center.y, 2 * lastPoint.z + tableTopLength);
                normal = (newCenter - origin).normalized;
            }
            else
            {
                origin = new Vector3(0, lastPoint.y, lastPoint.z);
                normal = vec.normalized;
            }

            int vert = vertexIndex;
            for (int i = 0; i < rampTiles; i++)
            {
                if (i == 0 && vert != 0)
                {
                    triangles[i * 6 + 0] = vert - 2;
                    triangles[i * 6 + 1] = vert;
                    triangles[i * 6 + 2] = vert - 1;
                    triangles[i * 6 + 3] = vert - 1;
                    triangles[i * 6 + 4] = vert;
                    triangles[i * 6 + 5] = vert + 1;
                }
                else
                {
                    triangles[i * 6 + 0] = vert + 0;
                    triangles[i * 6 + 1] = vert + 2;
                    triangles[i * 6 + 2] = vert + 1;
                    triangles[i * 6 + 3] = vert + 1;
                    triangles[i * 6 + 4] = vert + 2;
                    triangles[i * 6 + 5] = vert + 3;
                    vert += 2;
                }
            }
            vertexIndex += vertices.Length;
            return (vertices, triangles, uv);
        }

        (Vector3[], int[], Vector2[]) BuildFlat(SegmentData segment)
        {
            int blIndex = vertexIndex - 2;
            int brIndex = vertexIndex - 1;
            Vector3 bl = objectVertices[blIndex];
            Vector3 br = objectVertices[brIndex];
            Quaternion rotation = Quaternion.AngleAxis(-segment.angleWithHorizontal, Vector3.right);
            Vector3 direction = rotation * Vector3.forward;
            Vector3 tl = bl + direction * segment.length;
            Vector3 tr = br + direction * segment.length;

            Vector3[] vertices = new Vector3[2] { tl, tr };
            currentLen += segment.length;
            Vector2[] uv = new Vector2[2] { new Vector2(rampUvStart, currentLen / maxLen), new Vector2(1 - rampUvStart, currentLen / maxLen) };
            int[] triangles = BuildRectangle(brIndex, blIndex, vertexIndex, vertexIndex + 1);
            vertexIndex += 2;

            return (vertices, triangles, uv);
        }

        int[] BuildRectangle(int bl, int tl, int tr, int br)
        {
            return new int[]{
            bl, tl, br,
            br, tl, tr
        };
        }
    }
}
