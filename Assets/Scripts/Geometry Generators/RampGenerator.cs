using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Triangle = MeshUtils.Triangle;

namespace GeometryGenerators {

    public class RampGenerator : MonoBehaviour {

        public const float MIN_FRONT_ANGLE = 1f;
        public const float MAX_FRONT_ANGLE = 90f;

        public const float MIN_REAR_ANGLE = -90f;
        public const float MAX_REAR_ANGLE = 90f;

        public const float MIN_LENGTH = 0f;
        public const float MAX_LENGTH = 10f;

        public const float MIN_WIDTH = 0f;
        public const float MAX_WIDTH = 4f;

        [Header("Gizmos")]
        [SerializeField] bool drawGizmos = true;
        [SerializeField] Color gizmoColor = Color.cyan;

        [Header("Settings")]
        [SerializeField] bool multiMaterial = false;
        [Range(MIN_FRONT_ANGLE, MAX_FRONT_ANGLE)] public float frontAngle = 45f;
        [Range(MIN_REAR_ANGLE, MAX_REAR_ANGLE)] public float rearAngle = 0f;
        [Range(MIN_LENGTH, MAX_LENGTH)] public float frontLength = 5f;
        [Range(MIN_LENGTH, MAX_LENGTH)] public float rearLength = 5f;
        [Range(MIN_WIDTH, MAX_WIDTH)] public float width = 2f;

        public void Generate () {
            var mf = GetComponent<MeshFilter>();
            if(mf == null){
                Debug.LogError("No mesh filter assigned!");
                return;
            }
            var mesh = CreateMesh();
            mf.sharedMesh = mesh;
            var mc = GetComponent<MeshCollider>();
            if(mc != null){
                mc.sharedMesh = mesh;
            }
        }

        Mesh CreateMesh () {
            var points = Get2DPoints();
            var protoVerts = Make3DPoints(points);
            var vl = protoVerts.Length;
            var pl = points.Length;
            List<Vector3> vertices = new List<Vector3>();
            List<Triangle> triangles = new List<Triangle>();
            AddSide(0, false);
            AddSide(pl, true);
            int rampStart = triangles.Count;
            int rampEnd = triangles.Count;
            AddSurface(0);
            if(rearAngle == 0f){
                rampEnd = triangles.Count;
            }
            AddSurface(1);
            if(rearAngle != 0f){
                rampEnd = triangles.Count;
            }
            if(pl > 3){
                AddSurface(2);
            }
            var output = new Mesh();
            output.name = "Generated Ramp";
            output.vertices = vertices.ToArray();
            if(!multiMaterial){
                output.triangles = MeshUtils.MakeNormalTriangleIndexArray(triangles);
            }else{
                var ramp = new List<Triangle>();
                var rest = new List<Triangle>();
                for(int i=0; i<triangles.Count; i++){
                    if(i>=rampStart && i<rampEnd){
                        ramp.Add(triangles[i]);
                    }else{
                        rest.Add(triangles[i]);
                    }
                }
                output.subMeshCount = 2;
                output.SetTriangles(MeshUtils.MakeNormalTriangleIndexArray(ramp), 0, false);
                output.SetTriangles(MeshUtils.MakeNormalTriangleIndexArray(rest), 1, false);
            }
            output.RecalculateBounds();
            output.RecalculateNormals();
            output.RecalculateTangents();
            // output.subMeshCount
            return output;

            void AddSide (int protoVertOffset, bool flip) {
                int v = vertices.Count;
                for(int i=0; i<pl; i++){
                    vertices.Add(protoVerts[i+protoVertOffset]);
                }
                if(pl > 3){
                    var t1 = new Triangle(v+0, v+1, v+3);
                    var t2 = new Triangle(v+1, v+2, v+3);
                    triangles.Add(flip ? t1.reversed : t1);
                    triangles.Add(flip ? t2.reversed : t2);
                }else{
                    var t = new Triangle(v+0, v+1, v+2);
                    triangles.Add(flip ? t.reversed : t);
                }
            }

            void AddSurface (int startIndex) {
                int i = vertices.Count;
                vertices.Add(protoVerts[startIndex]);
                vertices.Add(protoVerts[(startIndex+1)%pl]);
                vertices.Add(protoVerts[startIndex+pl]);
                vertices.Add(protoVerts[((startIndex+1)%pl)+pl]);
                triangles.Add(new Triangle(i+2, i+1, i+0));
                triangles.Add(new Triangle(i+1, i+2, i+3));
            }
        }

        Vector2[] Get2DPoints () {
            var rFrontAngle = frontAngle * Mathf.Deg2Rad;
            var rRearAngle = rearAngle * Mathf.Deg2Rad;
            var cosFront = Mathf.Cos(rFrontAngle);
            var sinFront = Mathf.Sin(rFrontAngle);
            var cosRear = Mathf.Cos(rRearAngle);
            var sinRear = Mathf.Sin(rRearAngle);
            var p1 = new Vector2(-cosFront, -sinFront);
            var p2 = new Vector2(cosFront, sinFront) * frontLength;
            var p3 = p2 + (new Vector2(cosRear, -sinRear) * rearLength);
            if(p3.y <= p1.y){
                p3 = new Vector2(p2.x + ((p2.y - p1.y) / Mathf.Tan(rRearAngle)), p1.y);
                return new Vector2[]{p1, p2, p3};
            }else{
                var p4 = new Vector2(p3.x, p1.y);
                return new Vector2[]{p1, p2, p3, p4};
            }
        }

        Vector3[] Make3DPoints (Vector2[] twoDPoints) {
            var output = new Vector3[twoDPoints.Length * 2];
            for(int i=0; i<twoDPoints.Length; i++){
                var p = twoDPoints[i];
                output[i] = new Vector3(width / 2f, p.y, p.x);
                output[i + twoDPoints.Length] = new Vector3(-width / 2f, p.y, p.x);
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
            var l = points.Length / 2;
            for(int i=0; i<l; i++){
                var a = transform.TransformPoint(points[i]);
                var b = transform.TransformPoint(points[i+l]);
                Gizmos.DrawCube(a, Vector3.one * 0.1f);
                Gizmos.DrawCube(b, Vector3.one * 0.1f);
                Gizmos.DrawLine(a, b);
                var a2 = transform.TransformPoint(points[(i+1)%l]);
                var b2 = transform.TransformPoint(points[((i+1)%l)+l]);
                Gizmos.DrawLine(a, a2);
                Gizmos.DrawLine(b, b2);
            }
            Gizmos.color = cc;
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(RampGenerator))]
    public class RampGeneratorEditor : Editor {
            
            RampGenerator rg;

            void OnEnable () {
                rg = target as RampGenerator;
            }

            public override void OnInspectorGUI () {
                DrawDefaultInspector();
                if(GUILayout.Button("Generate")){
                    rg.Generate();
                }
            }
            
    }
    #endif

}