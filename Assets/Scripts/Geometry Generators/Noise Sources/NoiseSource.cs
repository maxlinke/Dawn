using UnityEngine;

namespace GeometryGenerators {

    public abstract class NoiseSource {

        [System.Flags]                  // enums have to have power of 2 indices! 0, 1, 2, 4, 8, 16, ...
        public enum Randomness {
            // unity takes care of none = 0
            RandomOffset = 1,
            RandomRotation = 2
        }

        public enum ValueRange {
            ZeroToOne,
            MinusOneToOne
        }

        private const float PI = Mathf.PI;

        [SerializeField, Range(0f, 1f)]  public float strength = 0.5f;
        [SerializeField, EnumFlags]      public Randomness randomness = 0;
        [SerializeField]                 public float size = 1f;
        [SerializeField]                 public ValueRange valueRange = default;

        [SerializeField]                 public Vector2 position = Vector2.zero;
        [SerializeField, Range(-PI, PI)] public float angle = 0f;
        [SerializeField]                 public Vector2 vecSize = Vector2.one;

        Matrix4x4 transform;
        System.Func<float, float> mapToRange;

        public virtual void Init (Vector2 inputOffset, float inputRotation) {
            var sx = size * vecSize.x;
            if(sx != 0) sx = 1f / sx;
            var sy = size * vecSize.y;
            if(sy != 0) sy = 1f / sy;
            var scale = new Matrix4x4(
                new Vector4(sx, 0f, 0f, 0f),
                new Vector4(0f, sy, 0f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f, 0f, 0f, 1f));
            var r = angle + (((randomness & Randomness.RandomRotation) == Randomness.RandomRotation) ? inputRotation : 0f);
            var cr = Mathf.Cos(r);
            var sr = Mathf.Sin(r);
            var rotation = new Matrix4x4(
                new Vector4(cr,-sr, 0f, 0f),
                new Vector4(sr, cr, 0f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f, 0f, 0f, 1f));
            var p = position + (((randomness & Randomness.RandomOffset) == Randomness.RandomOffset) ? inputOffset : Vector2.zero);
            var translation = new Matrix4x4(
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(0f, 1f, 0f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(p.x, p.y, 0f, 1f));
            transform = scale * rotation * translation;
            if(this is TextureNoiseSource){
                var texDelta = new Matrix4x4(
                    new Vector4(1f, 0f, 0f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0f, 0f, 1f, 0f),
                    new Vector4(0.5f, 0.5f, 0f, 1f));
                transform = texDelta * transform;
            }

            mapToRange = GetRangeFunc();

            System.Func<float, float> GetRangeFunc () {
                switch(valueRange){
                    case ValueRange.ZeroToOne:
                        return (f) => f;
                    case ValueRange.MinusOneToOne:
                        return (f) => ((2f * f) - 1f);
                    default:
                        Debug.Log($"Unknown {nameof(ValueRange)} \"{valueRange}\"!");
                        return (f) => f;
                }
            }
        }

        public float Evaluate (float x, float z) {
            TransformCoords(ref x, ref z);
            return strength * mapToRange(Eval01(x, z));
        }

        protected abstract float Eval01 (float x, float y);

        protected void TransformCoords (ref float x, ref float y) {
            var vec = transform * new Vector4(x, y, 0f, 1f);
            x = vec.x;
            y = vec.y;
        }

    }

}