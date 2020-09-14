using UnityEngine;

namespace GeometryGenerators {

    public class PlaneGenerator : GeometryGeneratorWithGizmos {

        public enum UVMode {
            VERTEXCOORDS,
            NORMALIZED
        }

        [Header("Plane Settings")]
        [SerializeField] [Range(1, 254)] protected int xTiles = 32;
        [SerializeField] [Range(1, 254)] protected int zTiles = 32;
        [SerializeField] protected Vector2 tileSize = Vector2.one;

        [Header("UV Settings")]
        [SerializeField] protected UVMode uvMode = UVMode.NORMALIZED;
        [SerializeField] protected Vector2 uvScale = Vector2.one;

        protected Vector3 LocalSize => new Vector3(xTiles * tileSize.x, 0f, zTiles * tileSize.y);
        protected Vector3 LocalExtents => 0.5f * LocalSize;

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
                    float x = tileSize.x * (i - iOffset);
                    float z = tileSize.y * (j - jOffset);
                    var pos = new Vector3(x, 0f, z);
                    vertices[index] = pos + GetAdditionalVertexOffset(pos);
                    float fracI = (float)i/xTiles;
                    float fracJ = (float)j/zTiles;
                    normedUVs[index] = new Vector2(fracI, fracJ) / uvScale;
                    localPosUVs[index] = new Vector2(x, z) / uvScale;
                }
            }

            Vector2[] texcoords;
            switch(uvMode){
                case UVMode.NORMALIZED:
                    texcoords = normedUVs;
                    break;
                case UVMode.VERTEXCOORDS:
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

        protected virtual Vector3 GetAdditionalVertexOffset (Vector3 position) {
            return Vector3.zero;
        }

        protected override void DrawGizmos () {
            var ext = LocalExtents;
            var last = WorldCorner(0);
            for(int i=0; i<4; i++){
                var current = WorldCorner(i+1);
                Gizmos.DrawLine(last, current);
                last = current;
            }
            
            Vector3 WorldCorner (int i) {
                var sx = ((i%4)>1 ? -1 : 1);
                var sz = (((i+1)%4)>1 ? -1 : 1);
                return transform.TransformPoint(Vector3.Scale(ext, new Vector3(sx, 0f, sz)));
            }
        }

    }

}