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

    const float g = 9.81f;
    const float dA = 1.27f;
    const float dT = 0.02f;

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
        var norm = WaterPhyicsSettings.GetApproxDensityNormalizer(g, dA);
        sb.AppendLine($"norm: {norm}");
        for(int i=0; i<materials.Length; i++){
            var mat = materials[i];
            sb.AppendLine($"{mat.name}, d={mat.density}");
            for(int j=0; j<diameters.Length; j++){
                var m = volumes[j] * mat.density * 1000f;
                var vt = RigidbodyDragHelper.CalculateRealTerminalVelocity(
                    mass: m, 
                    gravity: g,
                    airDensity: dA,
                    dragCoefficient: 0.47f,
                    area: areas[j]
                );
                var d = RigidbodyDragHelper.CalculateDrag(
                    terminalVelocity: vt,
                    gravity: g,
                    deltaTime: dT
                );
                // var dens = ApproxDensity(d, m);
                var dens = norm * WaterPhyicsSettings.FastApproxDensity(m, d, dT);
                // var dens = WaterPhyicsSettings.ApproxDensity(m, d, g, dA, dT);
                var rd = dens / mat.density;
                sb.AppendLine($"dia: {diameters[j]}\tmass: {m},\tvt: {vt},\tdrag: {d}\td?: {dens}\tratio: {rd}");
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    // returns approximated density in g/cm³
    // float ApproxDensity (float drag, float mass) {
    //     var consts = 9.81f * 1.27f * 0.47f;
    //     var d = (1f / drag) - 0.02f;
    //     var area = (2f * mass) / (consts * d * d);
    //     var radius = Mathf.Sqrt(area / Mathf.PI);
    //     var volume = (4f / 3f) * Mathf.PI * radius * radius * radius;
    //     var density = mass / volume;
    //     return density / 1000f;
    // }

    void Update () {

    }
	
}
