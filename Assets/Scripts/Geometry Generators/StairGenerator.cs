using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Triangle = MeshUtils.Triangle;

namespace GeometryGenerators {

    public class StairGenerator : GeometryGenerator {

        const int MIN_STEP_COUNT = 0;
        const int MAX_STEP_COUNT = 20;

        const float MIN_STEP_SIZE = 0.01f;
        const float MAX_STEP_SIZE = 2f;

        const float MIN_END_LENGTH = 1f;
        const float MAX_END_LENGTH = 10f;

        const float MIN_WIDTH = 0f;
        const float MAX_WIDTH = 10f;

        [Header("Gizmos")]
        [SerializeField] bool drawGizmos = true;
        [SerializeField] Color gizmoColor = Color.cyan;

        [Header("Settings")]
        [SerializeField, Range(MIN_STEP_COUNT, MAX_STEP_COUNT)] int stepCount = 10;
        [SerializeField, Range(MIN_STEP_SIZE, MAX_STEP_SIZE)] float stepHeight = 0.2f;
        [SerializeField, Range(MIN_STEP_SIZE, MAX_STEP_SIZE)] float stepLength = 0.5f;
        [SerializeField, Range(MIN_END_LENGTH, MAX_END_LENGTH)] float endLength = 2f;
        [SerializeField, Range(MIN_WIDTH, MAX_WIDTH)] float width = 2f;

        protected override Mesh CreateMesh () {
            var points = Get2DPoints();
            var protoVerts = Make3DPoints(points);
            var vl = protoVerts.Length;
            var pl = points.Length;
            List<Vector3> vertices = new List<Vector3>();
            List<Triangle> triangles = new List<Triangle>();
            AddCenterStrip();
            AddSide(0, false);
            AddSide(1, true);
            var output = new Mesh();
            output.name = "Generated Stairs";
            output.vertices = vertices.ToArray();
            output.triangles = MeshUtils.MakeNormalTriangleIndexArray(triangles);
            output.RecalculateBounds();
            output.RecalculateNormals();
            output.RecalculateTangents();
            return output;

            void AddCenterStrip () {
                var l0 = protoVerts[0];
                var l1 = protoVerts[1];
                for(int i=1; i<=pl; i++){
                    var vc = vertices.Count;
                    vertices.Add(l0);
                    vertices.Add(l1);
                    int j = 2*(i%pl);
                    var p0 = protoVerts[j+0];
                    var p1 = protoVerts[j+1];
                    vertices.Add(p0);
                    vertices.Add(p1);
                    triangles.Add(new Triangle(vc+0, vc+1, vc+2));
                    triangles.Add(new Triangle(vc+3, vc+2, vc+1));
                    l0 = p0;
                    l1 = p1;
                }
            }

            void AddSide (int indexOffset, bool flip) {
                var vc = vertices.Count;
                for(int i=0; i<pl; i++){
                    vertices.Add(protoVerts[(2*i)+indexOffset]);
                }
                for(int i=3; i<pl; i+=2){
                    var j = vc+i;
                    var t0 = new Triangle(j-2, j-1, j);
                    var t1 = new Triangle(j-2, j, vc);
                    triangles.Add(flip ? t0.reversed : t0);
                    triangles.Add(flip ? t1.reversed : t1);
                }
            }
        }

        Vector2[] Get2DPoints () {
            var output = new Vector2[2 + 2 + (2 * stepCount)];  // bottom, end flat and each step
            var totalLength = (stepCount * stepLength) + endLength;
            output[0] = new Vector2(totalLength, -1f);
            output[1] = new Vector2(0, -1f);
            float x = 0 * stepLength;
            float y = 1 * stepHeight;
            for(int i=0; i<stepCount; i++){
                output[2+(i*2)] = new Vector2(x, y);
                x = (i+1) * stepLength;
                output[3+(i*2)] = new Vector2(x, y);
                y = (i+2) * stepHeight;
            }
            output[output.Length-2] = new Vector2(x, y);
            output[output.Length-1] = new Vector3(totalLength, y);
            return output;
        }

        Vector3[] Make3DPoints (Vector2[] points) {
            var output = new Vector3[2 * points.Length];
            var w2 = width / 2f;
            for(int i=0; i<points.Length; i++){
                output[(2*i)+0] = new Vector3(w2, points[i].y, points[i].x);
                output[(2*i)+1] = new Vector3(-w2, points[i].y, points[i].x);
            }
            return output;
        }

        void OnDrawGizmosSelected () {
            if(!drawGizmos){
                return;
            }
            #if UNITY_EDITOR
            if(UnityEditor.Selection.activeGameObject != this.gameObject){
                return;
            }
            #endif
            var cc = Gizmos.color;
            Gizmos.color = gizmoColor;
            var points = Make3DPoints(Get2DPoints());
            var s0 = transform.TransformPoint(points[0]);
            var s1 = transform.TransformPoint(points[1]);
            var l0 = s0;
            var l1 = s1;
            for(int i=0; i<=points.Length; i+=2){
                Gizmos.DrawLine(l0, l1);
                var p0 = transform.TransformPoint(points[(i)%points.Length]);
                var p1 = transform.TransformPoint(points[(i+1)%points.Length]);
                Gizmos.DrawCube(p0, Vector3.one * 0.1f);
                Gizmos.DrawCube(p1, Vector3.one * 0.1f);
                Gizmos.DrawLine(p0, l0);
                Gizmos.DrawLine(p1, l1);
                l0 = p0;
                l1 = p1;
            }
            Gizmos.color = cc;
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(StairGenerator))]
    public class StairGeneratorEditor : Editor {
            
            StairGenerator sg;

            void OnEnable () {
                sg = target as StairGenerator;
            }

            public override void OnInspectorGUI () {
                DrawDefaultInspector();
                if(GUILayout.Button("Generate")){
                    sg.Generate();
                }
            }
            
    }
    #endif

}