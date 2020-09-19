using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    [System.Serializable]
    public struct mat {
        public string name;
        public float density;
    }

    [SerializeField] float[] diameters = default;
    [SerializeField] mat[] materials = default;

    [ContextMenu("Run")]
    void Awake () {
        if(diameters == null){
            diameters = new float[0];
        }
        float[] areas = new float[diameters.Length];
        float[] volumes = new float[diameters.Length];
        for(int i=0; i<diameters.Length; i++){
            var r = diameters[i] / 2f;
            areas[i] = Mathf.PI * r * r;
            volumes[i] = (4f / 3f) * r * areas[i];
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i=0; i<materials.Length; i++){
            var mat = materials[i];
            sb.AppendLine(mat.name);
            for(int j=0; j<diameters.Length; j++){
                var m = volumes[j] * mat.density * 1000f;
                var g = 9.81f;
                var dt = 0.02f;
                var vt = RigidbodyDragHelper.CalculateRealTerminalVelocity(
                    mass: m, 
                    gravity: g,
                    airDensity: 1.27f,
                    dragCoefficient: 0.47f,
                    area: areas[j]
                );
                var d = RigidbodyDragHelper.CalculateDrag(
                    terminalVelocity: vt,
                    gravity: g,
                    deltaTime: dt
                );
                // var b = Buoyancy(d, m);
                // var bd = b / mat.density;
                // sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\tb?: {b}\tratio: {bd}");
                var r = Radius(d, m);
                // var v = (4f / 3f) * Mathf.PI * r * r * r;
                var v = r * r * r * 150f;
                var dens = m / v;
                var rd = dens / mat.density;
                // var rr = r / diameters[i];
                sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\td?: {dens}\tratio: {rd}");
                // var sqrtD = Mathf.Sqrt(d);
                // var logM = Mathf.Log(m, 1000f);
                // sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\tsqrtD: {sqrtD}\tlogM: {logM}");
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    float Radius (float drag, float mass) {
        // float consts = 9.81f * 1.27f * 0.47f;
        // var d = (1f / drag) - 0.02f;
        // float area = (2f * mass) / (consts * d * d);
        // return Mathf.Sqrt(area) / Mathf.PI;
        var d = (1f / drag) - 0.02f;
        return Mathf.Sqrt(mass / (d * d));
    }

    // float Buoyancy (float drag, float mass) {    
    //     var sqrtD = Mathf.Sqrt(drag);
    //     var logM = Mathf.Log(mass);
    //     return sqrtD * Mathf.Pow(Mathf.Exp(1f/12f), logM);
    // }

    void Update () {
        
    }
	
}
