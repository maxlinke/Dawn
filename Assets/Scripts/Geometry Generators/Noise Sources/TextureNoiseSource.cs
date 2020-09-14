using UnityEngine;

namespace GeometryGenerators {

    [System.Serializable]
    public class TextureNoiseSource : NoiseSource {

        public Texture2D texture;

        System.Func<float, float> texWrap;

        public override void Init () {
            base.Init();
            if(texture == null){
                return;
            }
            texWrap = GetWrapping();
            Debug.Log($"{randomness:G}");

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
            // return Mathf.Clamp01(x + y);
            // return Mathf.Clamp01(Mathf.Abs(x) + Mathf.Abs(y));
        }

        // void ApplyWrapping (ref float x, ref float y) {
        //     switch(texture.wrapMode){
        //         case TextureWrapMode.Clamp:

        //             return;
        //         case TextureWrapMode.Repeat:
        //             x = Mathf.Repeat(x, 1f);
        //             y = Mathf.Repeat(y, 1f);


        //     }
        // }

    }

}