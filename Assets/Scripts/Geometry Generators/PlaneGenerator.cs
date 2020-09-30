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

        public enum TileMode {
            Quads,
            IsoTris
        }

        [Header("Plane Settings")]
        [SerializeField] protected TileMode tileMode = TileMode.Quads;
        [SerializeField] [Range(1, 254)] protected int xTiles = 32;
        [SerializeField] [Range(1, 254)] protected int zTiles = 32;
        [SerializeField] protected Vector2 tileSize = Vector2.one;
        [SerializeField] protected TriMode triMode = TriMode.Fixed;
        [SerializeField] protected string triSeed = string.Empty;

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
            Mesh output = null;
            Vector3[] vertices = null;
            Vector2[] texcoords = null;
            int[] triangles = null;
            if(tileMode == TileMode.Quads && TryMakeQuadMesh(out vertices, out texcoords, out triangles)){
                output = new Mesh();
            }
            if(tileMode == TileMode.IsoTris && TryMakeIsoTriMesh(out vertices, out texcoords, out triangles)){
                output = new Mesh();
            }
            if(output == null){
                return null;
            }
            switch(uvMode){
                case UVMode.Normalized:
                    break;
                case UVMode.VertexCoordinates:
                    for(int i=0; i<texcoords.Length; i++){
                        texcoords[i] = vertices[i].XZ() * uvScale;
                    }
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(UVMode)} \"{uvMode}\"!");
                    return null;
            }
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
            Vector3[] points;
            if(tileMode == TileMode.Quads){
                points = new Vector3[]{ 
                    GetVertexCoordinates(0, 0),
                    GetVertexCoordinates(0, zTiles),
                    GetVertexCoordinates(xTiles, zTiles),
                    GetVertexCoordinates(xTiles, 0) 
                };
            }else if(tileMode == TileMode.IsoTris){
                var d = GetIsoDimensions();
                var x = d.x / 2f;
                var z = d.y / 2f;
                points = new Vector3[]{
                    new Vector3(-x, 0f, -z),
                    new Vector3(-x, 0f,  z),
                    new Vector3( x, 0f,  z),
                    new Vector3( x, 0f, -z)
                };
            }else{
                points = new Vector3[]{};
            }
            for(int i=0; i<points.Length; i++){
                DrawLine(points[i], points[(i+1)%points.Length]);
            }

            void DrawLine (Vector3 a, Vector3 b) {
                Gizmos.DrawLine(
                    transform.TransformPoint(a),
                    transform.TransformPoint(b)
                );
            }
        }

        private bool TryMakeQuadMesh (out Vector3[] vertices, out Vector2[] texcoords, out int[] triangles) {
            int xVerts = xTiles + 1;
            int zVerts = zTiles + 1;
            int numberOfVerts = xVerts * zVerts;
            vertices = new Vector3[numberOfVerts];
            texcoords = new Vector2[numberOfVerts];
            int numberOfTris = xTiles * zTiles * 2;
            triangles = new int[numberOfTris * 3];
            if(!TryMakeVertsAndTexcoords(vertices, texcoords)){
                return false;
            }
            return TryMakeTriangles(vertices, triangles);

            bool TryMakeVertsAndTexcoords (Vector3[] verts, Vector2[] uvs) {
                float iOffset = (float)xTiles / 2f;
                float jOffset = (float)zTiles / 2f;
                for(int j=0; j<zVerts; j++){
                    for(int i=0; i<xVerts; i++){
                        int index = (j * xVerts) + i;
                        var pos = GetVertexCoordinates(i, j, iOffset, jOffset);
                        pos += GetAdditionalVertexOffset(pos);
                        verts[index] = pos;
                        float fracI = (float)i/xTiles;
                        float fracJ = (float)j/zTiles;
                        uvs[index] = new Vector2(fracI, fracJ) * uvScale;
                    }
                }
                return true;
            }

            System.Random GetTriRNG () {
                int rngSeed;
                if(triSeed == null || triSeed.Length == 0){
                    rngSeed = Random.value.GetHashCode();
                }else{
                    rngSeed = triSeed.Trim().GetHashCode();
                }
                return new System.Random(rngSeed);
            }

            System.Func<float, float, double, bool> GetTriangulationPatternFunction () {
                switch(triMode){
                    case TriMode.Fixed:
                        return (dia1, dia2, rng) => false;
                    case TriMode.FixedAlternate:
                        return (dia1, dia2, rng) => true;
                    case TriMode.Random:
                        return (dia1, dia2, rng) => (rng < 0.5);
                    case TriMode.ShortestDiagonal:
                        return (dia1, dia2, rng) => (dia1 > dia2);
                    case TriMode.LongestDiagonal:
                        return (dia1, dia2, rng) => (dia1 < dia2);
                    default:
                        Debug.LogError($"Unknown {nameof(TriMode)} \"{triMode}\"!");
                        return null;
                }
            }

            bool TryMakeTriangles (Vector3[] verts, int[] tris) {
                var triRNG = GetTriRNG();
                var triPattern = GetTriangulationPatternFunction();
                if(triPattern == null){
                    return false;
                }
                for(int j=0; j<zTiles; j++){
                    for(int i=0; i<xTiles; i++){
                        int quad = (j * xTiles) + i;
                        int triStart = quad * 6;
                        int vertStart = (j * xVerts) + i;

                        var bottomLeft = vertStart;
                        var bottomRight = vertStart + 1;
                        var topLeft = vertStart + xVerts;
                        var topRight = vertStart + xVerts + 1;

                        var sqrDiag1 = (verts[bottomLeft] - verts[topRight]).sqrMagnitude;
                        var sqrDiag2 = (verts[topLeft] - verts[bottomRight]).sqrMagnitude;
                        
                        var alternate = triPattern(sqrDiag1, sqrDiag2, triRNG.NextDouble());
                        if(!alternate){
                            tris[triStart + 0] = bottomLeft;
                            tris[triStart + 1] = topLeft;
                            tris[triStart + 2] = topRight;
                            tris[triStart + 3] = bottomLeft;
                            tris[triStart + 4] = topRight;
                            tris[triStart + 5] = bottomRight;
                        }else{
                            tris[triStart + 0] = bottomLeft;
                            tris[triStart + 1] = topLeft;
                            tris[triStart + 2] = bottomRight;
                            tris[triStart + 3] = topRight;
                            tris[triStart + 4] = bottomRight;
                            tris[triStart + 5] = topLeft;
                        }
                    }
                }
                return true;
            }
        }

        private Vector2 GetIsoDimensions () {
            var width = tileSize.x * (0.5f * (xTiles + 1));
            var height = tileSize.y * (zTiles * Mathf.Sqrt(3) * 0.5f);
            return new Vector2(width, height);
        }

        private bool TryMakeIsoTriMesh (out Vector3[] vertices, out Vector2[] texcoords, out int[] triangles) {
            var isoDims = GetIsoDimensions();
            GetXCoordArrays(isoDims.x, out var xCoordsBase, out var xCoordsOffset);
            var baseLineCount = 1 + (zTiles / 2);
            var offsetLineCount = 1 + ((zTiles - 1) / 2);
            vertices = new Vector3[(baseLineCount * xCoordsBase.Length) + (offsetLineCount * xCoordsOffset.Length)];
            texcoords = new Vector2[vertices.Length];
            triangles = new int[3 * (xTiles * zTiles)];
            MakeVertAndTexCoords(isoDims.x, isoDims.y, vertices, texcoords);
            MakeTris(vertices, triangles);
            return true;

            void GetXCoordArrays (float width, out float[] outputBaseCoords, out float[] outputOffsetCoords) {
                var baseVertCount = ((xTiles + 1) / 2) + 1;
                var offsetVertCount = baseVertCount - (xTiles % 2);
                outputBaseCoords = new float[baseVertCount];
                outputOffsetCoords = new float[offsetVertCount];
                for(int i=0; i<(baseVertCount + offsetVertCount); i++){
                    var fracI = (float)i / (baseVertCount + offsetVertCount - 1);
                    var x = (fracI - 0.5f) * width;
                    if(i%2 == 0){
                        outputBaseCoords[i/2] = x;
                    }else{
                        outputOffsetCoords[i/2] = x;
                    }
                }
            }

            void MakeVertAndTexCoords (float width, float height, Vector3[] verts, Vector2[] uvs) {
                int vertCounter = 0;
                for(int z=0; z<=zTiles; z++){
                    var fracZ = (float)z / zTiles;
                    var zCoord = (fracZ - 0.5f) * height;
                    var isBaseLine = ((z%2) == 0);
                    var xCoords = isBaseLine ? xCoordsBase : xCoordsOffset;
                    for(int x=0; x<xCoords.Length; x++){
                        var pos = new Vector3(xCoords[x], 0f, zCoord);
                        pos += GetAdditionalVertexOffset(pos);
                        verts[vertCounter] = pos;
                        uvs[vertCounter] = new Vector2((pos.x / width) + 0.5f, fracZ) * uvScale;
                        vertCounter++;
                    }
                }
            }

            void MakeTris (Vector3[] verts, int[] tris) {
                var equalLineLengths = (xCoordsBase.Length == xCoordsOffset.Length);
                var processedLineVerts = 0;
                for(int j=0; j<zTiles; j++){
                    var isBaseLine = ((j % 2) == 0);
                    var lineVertCount = (isBaseLine ? xCoordsBase.Length : xCoordsOffset.Length);
                    var bottomVert = processedLineVerts;
                    var topVert = bottomVert + lineVertCount;
                    var pointy = !isBaseLine;
                    for(int i=0; i<xTiles; i++){
                        var index = j * xTiles + i;
                        var triStart = 3 * index;
                        tris[triStart+0] = bottomVert;
                        tris[triStart+1] = topVert;
                        if(pointy){
                            topVert++;
                            tris[triStart+2] = topVert;
                        }else{
                            bottomVert++;
                            tris[triStart+2] = bottomVert;
                        }
                        pointy = !pointy;
                    }
                    processedLineVerts += lineVertCount;
                }
            }
        }

    }

}