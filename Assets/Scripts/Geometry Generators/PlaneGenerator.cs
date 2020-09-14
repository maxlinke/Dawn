using UnityEngine;

namespace GeometryGenerators {

    public class PlaneGenerator : GeometryGeneratorWithGizmos {

        public enum UVMode {
            VertexCoordinates,
            Normalized
        }

        [Header("Plane Settings")]
        [SerializeField] [Range(1, 254)] protected int xTiles = 32;
        [SerializeField] [Range(1, 254)] protected int zTiles = 32;
        [SerializeField] protected Vector2 tileSize = Vector2.one;

        [Header("UV Settings")]
        [SerializeField] protected UVMode uvMode = UVMode.Normalized;
        [SerializeField] protected Vector2 uvScale = Vector2.one;

        protected Vector3 GetVertexCoordinates (int x, int z) {
            var xOffset = (float)xTiles / 2f;
            var zOffset = (float)zTiles / 2f;
            return GetVertexCoordinates(x, z, xOffset, zOffset);
        }

        protected Vector3 GetVertexCoordinates (int x, int z, float xOffset, float zOffset) {
            return new Vector3(tileSize.x * (x - xOffset), 0f, tileSize.y * (z - zOffset));
        }

        protected override Mesh CreateMesh () {
            int xVerts = xTiles + 1;
            int zVerts = zTiles + 1;
            int numberOfVerts = xVerts * zVerts;
            Vector3[] vertices = new Vector3[numberOfVerts];
            Vector2[] normedUVs = new Vector2[numberOfVerts];
            Vector2[] localPosUVs = new Vector2[numberOfVerts];

            float iOffset = (float)xTiles / 2f;
            float jOffset = (float)zTiles / 2f;
            for(int j=0; j<zVerts; j++){
                for(int i=0; i<xVerts; i++){
                    int index = (j * xVerts) + i;
                    var pos = GetVertexCoordinates(i, j, iOffset, jOffset);
                    pos += GetAdditionalVertexOffset(i , j, pos);
                    vertices[index] = pos;
                    float fracI = (float)i/xTiles;
                    float fracJ = (float)j/zTiles;
                    normedUVs[index] = new Vector2(fracI, fracJ) / uvScale;
                    localPosUVs[index] = new Vector2(pos.x, pos.z) / uvScale;
                }
            }

            Vector2[] texcoords;
            switch(uvMode){
                case UVMode.Normalized:
                    texcoords = normedUVs;
                    break;
                case UVMode.VertexCoordinates:
                    texcoords = localPosUVs;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(UVMode)} \"{uvMode}\"!");
                    texcoords = normedUVs;
                    break;
            }

            int numberOfTris = xTiles * zTiles * 2;
            int[] triangles = new int[numberOfTris * 3];

            for(int j=0; j<zTiles; j++){
                for(int i=0; i<xTiles; i++){
                    int quad = (j * xTiles) + i;
                    int triStart = quad * 6;
                    int vertStart = (j * xVerts) + i;
                    triangles[triStart + 0] = vertStart;
                    triangles[triStart + 1] = vertStart + xVerts;
                    triangles[triStart + 2] = vertStart + xVerts + 1;
                    triangles[triStart + 3] = vertStart;
                    triangles[triStart + 4] = vertStart + 1 + xVerts;
                    triangles[triStart + 5] = vertStart + 1;
                }
            }

            Mesh output = new Mesh();
            output.name = "Custom Plane";
            output.vertices = vertices;
            output.uv = texcoords;
            output.triangles = triangles;
            output.RecalculateBounds();
            output.RecalculateNormals();
            output.RecalculateTangents();
            return output;
        }

        protected virtual Vector3 GetAdditionalVertexOffset (int x, int z, Vector3 position) {
            return Vector3.zero;
        }

        protected override void DrawGizmos () {
            var points = new Vector3[] { 
                GetVertexCoordinates(0, 0),
                GetVertexCoordinates(0, zTiles),
                GetVertexCoordinates(xTiles, zTiles),
                GetVertexCoordinates(xTiles, 0) 
            };
            for(int i=0; i<4; i++){
                DrawLine(points[i], points[(i+1)%4]);
            }

            void DrawLine (Vector3 a, Vector3 b) {
                Gizmos.DrawLine(
                    transform.TransformPoint(a),
                    transform.TransformPoint(b)
                );
            }
        }

    }

}