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
                var b = Buoyancy(d, m);
                sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\tb?: {b}");
                // var sqrtD = Mathf.Sqrt(d);
                // var logM = Mathf.Log(m, 1000f);
                // sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\tsqrtD: {sqrtD}\tlogM: {logM}");
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    // good approximation, albeit a but wasteful with its functions...
    // what exactly does the value represent?
    // lower mass end is lower output than higher mass end
    // however the higher this value, the more buoyant the result should be
    float Buoyancy (float drag, float mass) {
        // return Mathf.Sqrt(drag);
        // return Mathf.Log10(mass);
        // return Mathf.Pow(mass, 0.001f);
        // return Mathf.Log(mass, 1000f);
        var sqrtD = Mathf.Sqrt(drag);
        var logM = Mathf.Log(mass, 1000f);
        // return sqrtD * logM * logM;
        return sqrtD * Mathf.Pow(1.78f, logM);      // 1.78 is 1/0.56. 0.56 is the approx limit of the ratio between drag values for every 1000x increase in mass...
    }

    void Update () {
        
    }
	
}
