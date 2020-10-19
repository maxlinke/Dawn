using System.Collections.Generic;
using UnityEngine;

namespace GeometryGenerators {

    public class LadderGenerator : GeometryGeneratorWithGizmos {

        const float MIN_LENGTH = 0.5f;
        const float MAX_LENGTH = 100f;

        const float MIN_WIDTH = 0.5f;
        const float MAX_WIDTH = 4f;

        const float MIN_RUNG_SPACING = 0.1f;
        const float MAX_RUNG_SPACING = 1f;

        const int MIN_RING_VERTS = 3;
        const int MAX_RING_VERTS = 30;

        const float MIN_RAIL_RADIUS = 0.01f;
        const float MAX_RAIL_RADIUS = 0.5f;

        const float MIN_RUNG_RADIUS = 0.01f;
        const float MAX_RUNG_RADIUS = 0.5f;

        enum OriginMode {
            Top,
            Center,
            Bottom
        }

        [Header("Settings")]
        [SerializeField]                                            OriginMode originMode = OriginMode.Top;
        [SerializeField, Range(MIN_LENGTH, MAX_LENGTH)]             float length = 10f;
        [SerializeField, Range(MIN_WIDTH, MAX_WIDTH)]               float width = 1f;
        [SerializeField, Range(MIN_RUNG_SPACING, MAX_RUNG_SPACING)] float rungSpacing = 0.5f;
        [SerializeField, Range(MIN_RUNG_SPACING, MAX_RUNG_SPACING)] float topRungOffset = 0.25f;
        [SerializeField, Range(MIN_RING_VERTS, MAX_RING_VERTS)]     int railVertexCount = 7;
        [SerializeField, Range(MIN_RING_VERTS, MAX_RING_VERTS)]     int rungVertexCount = 7;
        [SerializeField, Range(MIN_RAIL_RADIUS, MAX_RAIL_RADIUS)]   float railRadius = 0.1f;
        [SerializeField, Range(MIN_RUNG_RADIUS, MAX_RUNG_RADIUS)]   float rungRadius = 0.1f;
        
        protected override Mesh CreateMesh () {
            var points = GetPoints();
            List<Vector3> vertices = new List<Vector3>();
            List<Triangle> triangles = new List<Triangle>();
            for(int i=0; i<points.Length; i+=2){
                int svc = vertices.Count;
                var railDeltas = GetDeltas(forRails: true);
                var rungDeltas = GetDeltas(forRails: false);
                var p0 = points[i];
                var p1 = points[i+1];
                if(i<4){
                    AddCap(railDeltas, p0, flip: false);
                    AddCap(railDeltas, p1, flip: true);
                    AddRing(railDeltas);
                }else{
                    AddRing(rungDeltas);
                }

                void AddCap (Vector3[] deltas, Vector3 centerPoint, bool flip) {
                    var vc = vertices.Count;
                    vertices.Add(centerPoint);
                    for(int j=0; j<deltas.Length; j++){
                        vertices.Add(centerPoint + deltas[j]);
                        var ti0 = vc;                                   // center
                        var ti1 = vc + 1 + j;                           // new vert
                        var ti2 = vc + 1 + ((j+1)%deltas.Length);   // "next" vert
                        var t = new Triangle(ti0, ti1, ti2);
                        triangles.Add(flip ? t.reversed : t);
                    }
                }

                void AddRing (Vector3[] deltas) {
                    var vc = vertices.Count;
                    for(int j=0; j<deltas.Length; j++){
                        vertices.Add(p0 + deltas[j]);
                        vertices.Add(p1 + deltas[j]);
                        var curr = vc + (2 * j);
                        var next = vc + (2 * ((j+1)% deltas.Length));
                        triangles.Add(new Triangle(curr + 0, curr + 1, next + 0));
                        triangles.Add(new Triangle(next + 1, next + 0, curr + 1));
                    }
                }
            }
            var output = new Mesh();
            output.name = "Generated Ladder";
            output.vertices = vertices.ToArray();
            output.triangles = Triangle.MakeIndexArrayFromTriangles(triangles);
            output.RecalculateBounds();
            output.RecalculateNormals();
            output.RecalculateTangents();
            return output;

            Vector3[] GetDeltas (bool forRails) {
                int vertexCount;
                float radius;
                System.Func<float, float, Vector3> makeDelta;
                if(forRails){
                    vertexCount = railVertexCount;
                    radius = railRadius;
                    makeDelta = (a, b) => new Vector3(a, 0, b);
                }else{
                    vertexCount = rungVertexCount;
                    radius = rungRadius;
                    makeDelta = (a, b) => new Vector3(0, b, a);
                }
                var deltas = new Vector3[vertexCount];
                for(int j=0; j<vertexCount; j++){
                    var fj = ((float)j)/vertexCount;
                    var cj = 2f * Mathf.PI * fj;
                    deltas[j] = makeDelta(Mathf.Cos(cj) * radius, Mathf.Sin(cj) * radius);
                }
                return deltas;
            }
        }

        Vector3[] GetPoints () {
            var output = new List<Vector3>();
            var bottomPos = GetBottomPos();
            AddRail(-1);
            AddRail(1);
            var rungY = bottomPos.y + length - topRungOffset;
            while(rungY > bottomPos.y){
                AddRung(rungY);
                rungY -= rungSpacing;
            }
            return output.ToArray();

            Vector3 GetBottomPos () {
                switch(originMode){
                    case OriginMode.Top:    return new Vector3(0f, -length, 0f);
                    case OriginMode.Center: return new Vector3(0f, -0.5f * length, 0f);
                    case OriginMode.Bottom: return Vector3.zero;
                    default: 
                        Debug.LogError($"Unknown {nameof(OriginMode)} \"{originMode}\"!");
                        return Vector3.zero;
                }
            }

            void AddRail (float side) {
                output.Add(bottomPos + new Vector3(side * 0.5f * width, 0f, 0f));
                output.Add(bottomPos + new Vector3(side * 0.5f * width, length, 0f));
            }

            void AddRung (float y) {
                output.Add(new Vector3(-0.5f * width, y, 0f));
                output.Add(new Vector3(0.5f * width, y, 0f));
            }
        }

        protected override void DrawGizmos () {
            var points = GetPoints();
            for(int i=0; i<points.Length; i+=2){
                var a = transform.TransformPoint(points[i]);
                var b = transform.TransformPoint(points[i+1]);
                Gizmos.DrawLine(a, b);
            }
        }
        
    }

}