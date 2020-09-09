using UnityEngine;

namespace GeometryGenerators {

    public abstract class NoiseSource {

        [SerializeField] public float scale = 1f;
        [SerializeField, Range(0f, 1f)] public float strength = 0.5f;
        [SerializeField] public bool randomOffset = false;
        [SerializeField] public bool randomRotation = false;

        [System.NonSerialized] public Vector2 offset;
        [System.NonSerialized] public float rotation;

        public abstract float Evaluate (float x, float y);

        protected void TransformCoords (ref float x, ref float y) {
            if(randomOffset){
                x = x + offset.x;
                y = y + offset.y;
            }
            if(randomRotation){
                float sin = Mathf.Sin(rotation);
                float cos = Mathf.Cos(rotation);
                float ox = x;
                float oy = y;
                x = (cos * ox) + (sin * oy);
                y = (cos * oy) - (sin * ox);
            }
            x = x / scale;
            y = y / scale;
        }

    }

}