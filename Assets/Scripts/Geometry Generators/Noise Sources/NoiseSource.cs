using UnityEngine;

namespace GeometryGenerators {

    public abstract class NoiseSource {

        [System.Flags]                  // enums have to have power of 2 indices! 0, 1, 2, 4, 8, 16, ...
        public enum Randomness {
            OFFSET = 1,
            ROTATION = 2
        }

        [SerializeField] public Vector2 scale = Vector2.one;
        [SerializeField, Range(0f, 1f)] public float strength = 0.5f;
        [SerializeField, EnumFlags] public Randomness randomness;

        [System.NonSerialized] public Vector2 offset;
        [System.NonSerialized] public float rotation;
        bool applyRotation;
        bool applyOffset;

        public virtual void Init () {
            applyRotation = (randomness & Randomness.ROTATION) == Randomness.ROTATION;
            applyOffset = (randomness & Randomness.OFFSET) == Randomness.OFFSET;
            Debug.Log(applyRotation + " " + applyOffset);
        }

        public abstract float Evaluate (float x, float y);

        protected void TransformCoords (ref float x, ref float y) {
            if(scale.x != 0f){
                x = x / scale.x;
            }
            if(scale.y != 0f){
                y = y / scale.y;
            }
            if(this is TextureNoiseSource){
                x += 0.5f;
                y += 0.5f;
            }
            if(applyRotation){
                float sin = Mathf.Sin(rotation);
                float cos = Mathf.Cos(rotation);
                float ox = x;
                float oy = y;
                x = (cos * ox) + (sin * oy);
                y = (cos * oy) - (sin * ox);
            }
            if(applyOffset){
                x = x + offset.x;
                y = y + offset.y;
            }
        }

    }

}