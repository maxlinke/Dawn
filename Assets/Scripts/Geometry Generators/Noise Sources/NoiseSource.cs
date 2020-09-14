using UnityEngine;

namespace GeometryGenerators {

    public abstract class NoiseSource {

        [System.Flags]                  // enums have to have power of 2 indices! 0, 1, 2, 4, 8, 16, ...
        public enum Randomness {
            // unity takes care of none = 0
            OFFSET = 1,
            ROTATION = 2
        }

        private const float PI = Mathf.PI;

        [SerializeField]                 public Vector2 position = Vector2.zero;
        [SerializeField, Range(-PI, PI)] public float angle = 0f;
        [SerializeField]                 public Vector2 size = Vector2.one;

        [SerializeField, Range(0f, 1f)]  public float strength = 0.5f;
        [SerializeField, EnumFlags]      public Randomness randomness = 0;

        Matrix4x4 transform;

        public virtual void Init (Vector2 inputOffset, float inputRotation) {            
            var sx = size.x != 0 ? 1f / size.x : 0f;
            var sy = size.y != 0 ? 1f / size.y : 0f;
            var scale = new Matrix4x4(
                new Vector4(sx, 0f, 0f, 0f),
                new Vector4(0f, sy, 0f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f, 0f, 0f, 1f));
            var r = angle + (((randomness & Randomness.ROTATION) == Randomness.ROTATION) ? inputRotation : 0f);
            var cr = Mathf.Cos(r);
            var sr = Mathf.Sin(r);
            var rotation = new Matrix4x4(
                new Vector4(cr,-sr, 0f, 0f),
                new Vector4(sr, cr, 0f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f, 0f, 0f, 1f));
            var p = position + (((randomness & Randomness.OFFSET) == Randomness.OFFSET) ? inputOffset : Vector2.zero);
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
        }

        public abstract float Evaluate (float x, float y);

        protected void TransformCoords (ref float x, ref float y) {
            var vec = transform * new Vector4(x, y, 0f, 1f);
            x = vec.x;
            y = vec.y;
        }

    }

}