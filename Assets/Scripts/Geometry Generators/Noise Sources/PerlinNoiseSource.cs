using UnityEngine;

namespace GeometryGenerators {

    [System.Serializable]
    public class PerlinNoiseSource : NoiseSource {

        protected override float Eval01 (float x, float y) => Mathf.PerlinNoise(x, y);

    }

}