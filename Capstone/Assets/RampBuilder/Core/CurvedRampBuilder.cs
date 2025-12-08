using UnityEngine;

namespace RampCreation
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class CurvedRampBuilder : MonoBehaviour
    {
        [Header("Ramp Specs")]
        public float rampRadius = 1;
        [Range(0.1f, 90.0f)]
        public float rampEndAngle = 30;
        [Tooltip("The more tiles the better the curve. Choose one tile to have a straight ramp.")]
        [SerializeField] int rampTiles = 40;
        [Tooltip("Make the final part of the ramp linear, if it's long enough this removes the angular velocity of the object riding the ramp.")]

        [Header("Curve Specs")]
        [SerializeField] float innerRadius = 1;

        [Range(0.1f, 360.0f)]
        [SerializeField] float curveEndAngle = 30;
        [SerializeField] int curveTiles = 40;

        [Header("Landing Specs")]

        Vector3[] objectVertices;
        int[] objectTriangles;
        Vector2[] objectUV;


        // Public methods
        public void BuildRamp()
        {
            /// Builds the mesh of the ramp and update the MeshFilter and MeshCollider components.
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

        private void OnValidate()
        {
            if (rampRadius < 0)
            {
                rampRadius = 0;
            }
            if (rampTiles < 1)
            {
                rampTiles = 1;
            }
            if (curveTiles < 1)
            {
                curveTiles = 1;
            }
            if (innerRadius < 0)
            {
                innerRadius = 0;
            }
        }

        void BuildMesh()
        {
            // Build top face
            (Vector3[] vertices, int[] triangles, Vector2[] uv) = BuildCurvedRamp();
            int nTopVertices = vertices.Length;

            // Build back face
            (Vector3[] backVertices, int[] backTriangles, Vector2[] backUV) = BuildBackFace(curveTiles, vertices, vertices.Length - 1 - curveTiles);
            uv = uv.Concat(backUV);
            vertices = vertices.Concat(backVertices);
            triangles = triangles.Concat(backTriangles);
            int nBackVertices = vertices.Length;

            // Build the side faces
            if (curveEndAngle != 360)
            {
                triangles = triangles.Concat(BuildCurveSide(0, nTopVertices, true));
                triangles = triangles.Concat(BuildCurveSide(curveTiles, nTopVertices + curveTiles, false));
            }

            // // Build the bottom face
            int leftBottomIndex = vertices.Length - curveTiles - 1;
            int[] bottomTriangles = BuildBottomFace(leftBottomIndex);
            triangles = triangles.Concat(bottomTriangles);

            objectVertices = vertices;
            objectTriangles = triangles;
            objectUV = uv;
        }

        (Vector3[], int[], Vector2[]) BuildBackFace(int nTiles, Vector3[] vertices, int topLeftIndex)
        {
            /// Build the back face
            /// The vertix on the right index is the rightmost vertix on the top face that is further way in the z axis.
            Vector3[] backVertices = new Vector3[nTiles + 1];
            Vector2[] backUV = new Vector2[nTiles + 1];
            int[] backTriangles = new int[nTiles * 6];
            int vert = topLeftIndex;
            int currentIndex = vertices.Length;
            backVertices[0] = new Vector3(vertices[vert].x, 0, vertices[vert].z);
            backUV[0] = new Vector2(0, 0);
            vert++;
            currentIndex++;
            for (int i = 1; i < nTiles + 1; i++)
            {
                backVertices[i] = new Vector3(vertices[vert].x, 0, vertices[vert].z);
                backTriangles[(i - 1) * 6 + 0] = vert - 1;
                backTriangles[(i - 1) * 6 + 1] = currentIndex - 1;
                backTriangles[(i - 1) * 6 + 2] = vert;
                backTriangles[(i - 1) * 6 + 3] = vert;
                backTriangles[(i - 1) * 6 + 4] = currentIndex - 1;
                backTriangles[(i - 1) * 6 + 5] = currentIndex;
                currentIndex++;
                vert++;
            }
            return (backVertices, backTriangles, backUV);
        }

        int[] BuildCurveSide(int startIndex, int fanVertixIndex, bool isLeftSide)
        {
            /// Connects the vertice of one side of the tile, with the vertice of the fan and the next tile.
            int[] triangles = new int[rampTiles * 3];

            for (int i = 0; i < rampTiles; i++)
            {
                triangles[i * 3 + 0] = fanVertixIndex;
                if (isLeftSide)
                {
                    triangles[i * 3 + 1] = (curveTiles + 1) * (i + 1) + startIndex;
                    triangles[i * 3 + 2] = (curveTiles + 1) * i + startIndex;
                }
                else
                {
                    triangles[i * 3 + 1] = (curveTiles + 1) * i + startIndex;
                    triangles[i * 3 + 2] = (curveTiles + 1) * (i + 1) + startIndex;
                }
            }
            return triangles;
        }

        int[] BuildBottomFace(int leftBottomIndex)
        {
            /// Build the bottom face
            int[] bottomTriangles = new int[curveTiles * 6];
            for (int i = 0; i < curveTiles; i++)
            {
                bottomTriangles[i * 6 + 0] = i;
                bottomTriangles[i * 6 + 1] = i + 1;
                bottomTriangles[i * 6 + 2] = leftBottomIndex + i;
                bottomTriangles[i * 6 + 3] = leftBottomIndex + i;
                bottomTriangles[i * 6 + 4] = i + 1;
                bottomTriangles[i * 6 + 5] = leftBottomIndex + i + 1;
            }
            return bottomTriangles;
        }

        (Vector3[], int[], Vector2[]) BuildCurvedRamp()
        {
            Vector3[] vertices = new Vector3[(rampTiles + 1) * (curveTiles + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[rampTiles * curveTiles * 6];
            float rampStepAngle = Mathf.Deg2Rad * rampEndAngle / rampTiles;
            float curveStepAngle = Mathf.Deg2Rad * curveEndAngle / curveTiles;

            for (int b = 0, i = 0; i < rampTiles + 1; i++)
            {
                float rampAngle = rampStepAngle * i;
                float y = rampRadius - rampRadius * Mathf.Cos(rampAngle);
                // u is a point in the ramp projected to the xz plane
                float u = rampRadius * Mathf.Sin(rampAngle);
                for (int j = 0; j < curveTiles + 1; j++)
                {
                    float curveAngle = curveStepAngle * j;
                    float x = Mathf.Sin(curveAngle) * (innerRadius + u);
                    float z = Mathf.Cos(curveAngle) * (innerRadius + u);
                    vertices[b] = new Vector3(x, y, z); //i*(curveTiles + 1) + j
                    uv[b] = new Vector2((float)j / curveTiles, (float)i / rampTiles);
                    b++;
                }
            }

            int vert = 0;
            int tris = 0;
            for (int i = 0; i < rampTiles; i++)
            {
                for (int j = 0; j < curveTiles; j++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + curveTiles + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + curveTiles + 1;
                    triangles[tris + 5] = vert + curveTiles + 2;

                    vert++;
                    tris += 6;
                }
                vert++;
            }
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