using UnityEngine;

namespace GeometryGenerators {

    public class PlaneGenerator : GeometryGeneratorWithGizmos {

        public enum UVMode {
            VertexCoordinates,
            Normalized
        }

        public enum TriMode {
            Fixed,
            FixedAlternate,
            Random,
            ShortestDiagonal,
            LongestDiagonal
        }

        [Header("Plane Settings")]
        [SerializeField] [Range(1, 254)] protected int xTiles = 32;
        [SerializeField] [Range(1, 254)] protected int zTiles = 32;
        [SerializeField] protected Vector2 tileSize = Vector2.one;
        [SerializeField] protected TriMode triMode = TriMode.Fixed;
        [SerializeField] string triSeed = string.Empty;

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
            Vector2[] texcoords = new Vector2[numberOfVerts];

            float iOffset = (float)xTiles / 2f;
            float jOffset = (float)zTiles / 2f;
            for(int j=0; j<zVerts; j++){
                for(int i=0; i<xVerts; i++){
                    int index = (j * xVerts) + i;
                    var pos = GetVertexCoordinates(i, j, iOffset, jOffset);
                    pos += GetAdditionalVertexOffset(pos);
                    vertices[index] = pos;
                    float fracI = (float)i/xTiles;
                    float fracJ = (float)j/zTiles;
                    switch(uvMode){
                        case UVMode.Normalized:
                            texcoords[index] = new Vector2(fracI, fracJ) * uvScale;
                            break;
                        case UVMode.VertexCoordinates:
                            texcoords[i] = new Vector2(pos.x, pos.z) * uvScale;
                            break;
                        default:
                            Debug.LogError($"Unknown {nameof(UVMode)} \"{uvMode}\"!");
                            return null;
                    }
                }
            }            

            int numberOfTris = xTiles * zTiles * 2;
            int[] triangles = new int[numberOfTris * 3];

            int rngSeed;
            if(triSeed == null || triSeed.Length == 0){
                rngSeed = Random.value.GetHashCode();
            }else{
                rngSeed = triSeed.Trim().GetHashCode();
            }
            System.Random triRNG = new System.Random(rngSeed);
            for(int j=0; j<zTiles; j++){
                for(int i=0; i<xTiles; i++){
                    int quad = (j * xTiles) + i;
                    int triStart = quad * 6;
                    int vertStart = (j * xVerts) + i;
                    var bottomLeft = vertStart;
                    var bottomRight = vertStart + 1;
                    var topLeft = vertStart + xVerts;
                    var topRight = vertStart + xVerts + 1;
                    var sqrDiag1 = (vertices[bottomLeft] - vertices[topRight]).sqrMagnitude;
                    var sqrDiag2 = (vertices[topLeft] - vertices[bottomRight]).sqrMagnitude;
                    bool alternate;
                    switch(triMode){
                        case TriMode.Fixed:
                            alternate = false;
                            break;
                        case TriMode.FixedAlternate:
                            alternate = true;
                            break;
                        case TriMode.Random:
                            alternate = triRNG.NextDouble() < 0.5;
                            break;
                        case TriMode.ShortestDiagonal:
                            alternate = sqrDiag1 > sqrDiag2;
                            break;
                        case TriMode.LongestDiagonal:
                            alternate = sqrDiag1 < sqrDiag2;
                            break;
                        default:
                            Debug.LogError($"Unknown {nameof(TriMode)} \"{triMode}\"!");
                            return null;
                    }
                    if(!alternate){
                        triangles[triStart + 0] = bottomLeft;
                        triangles[triStart + 1] = topLeft;
                        triangles[triStart + 2] = topRight;
                        triangles[triStart + 3] = bottomLeft;
                        triangles[triStart + 4] = topRight;
                        triangles[triStart + 5] = bottomRight;
                    }else{
                        triangles[triStart + 0] = bottomLeft;
                        triangles[triStart + 1] = topLeft;
                        triangles[triStart + 2] = bottomRight;
                        triangles[triStart + 3] = topRight;
                        triangles[triStart + 4] = bottomRight;
                        triangles[triStart + 5] = topLeft;
                    }
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