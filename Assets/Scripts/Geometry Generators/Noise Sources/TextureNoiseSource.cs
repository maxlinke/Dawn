using UnityEngine;

namespace GeometryGenerators {

    [System.Serializable]
    public class TextureNoiseSource : NoiseSource {

        [SerializeField] public Texture2D texture = null;

        System.Func<float, float> texWrap;

        public override void Init (Vector2 offset, float rotation) {
            base.Init(offset, rotation);
            if(texture == null){
                return;
            }
            texWrap = GetWrapping();

            System.Func<float, float> GetWrapping () {
                switch(texture.wrapMode){
                    case TextureWrapMode.Clamp:
                        return Mathf.Clamp01;
                    case TextureWrapMode.Mirror:
                        return (f) => {return Mathf.PingPong(f, 1f);};
                    case TextureWrapMode.MirrorOnce:
                        return (f) => {return Mathf.PingPong(Mathf.Clamp(f, -1f, 2f), 1f);};
                    case TextureWrapMode.Repeat:
                        return (f) => {return Mathf.Repeat(f, 1f);};
                    default:
                        Debug.LogError($"Unknown {nameof(TextureWrapMode)} \"{texture.wrapMode}\"!");
                        return (f) => f;
                }
            }
        }

        /// <summary>
        /// Does a texture lookup at the given uv-coordinates. Output is the texture's luminance denormalized to [-1, 1]
        /// </summary>
        public override float Evaluate (float x, float y) {
            if(texture == null){
                return 0f;
            }
            TransformCoords(ref x, ref y);
            x = texWrap(x);
            y = texWrap(y);
            Color col = texture.GetPixelBilinear(x, y);
            float lum = 0.299f * col.r + 0.587f * col.g + 0.115f * col.b;
            return strength * ((2f * lum) - 1f);
        }

    }

}