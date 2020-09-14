using UnityEngine;

namespace GeometryGenerators {

    [System.Serializable]
    public class TextureNoiseSource : NoiseSource {

        [SerializeField] public Texture2D texture = null;
        [SerializeField] public float filterSize = 0f;

        System.Func<float, float> texWrap;
        System.Func<float, float, float> eval;
        Vector2[] filterOffsets;

        public override void Init (Vector2 offset, float rotation) {
            base.Init(offset, rotation);
            if(texture == null){
                return;
            }
            texWrap = GetWrapping();
            eval = GetEval();

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

            System.Func<float, float, float> GetEval () {
                if(filterSize == 0f){
                    return EvalTex;
                }
                filterOffsets = new Vector2[9];
                for(int i=0; i<9; i++){
                    var dx = i%3 - 1;
                    var dy = i/3 - 1;
                    filterOffsets[i] = filterSize * new Vector2(dx, dy);
                }
                return (x, y) => {
                    var output = 0f;
                    for(int f=0; f<filterOffsets.Length; f++){
                        var delta = filterOffsets[f];
                        output += EvalTex(x + delta.x, y + delta.y);
                    }
                    return output / filterOffsets.Length;
                };
            }
        }

        protected override float Eval01 (float x, float y) {
            if(texture == null){
                return 0f;
            }
            return eval(x, y);
        }

        private float EvalTex (float x, float y) {
            x = texWrap(x);
            y = texWrap(y);
            Color col = texture.GetPixelBilinear(x, y);
            float lum = 0.299f * col.r + 0.587f * col.g + 0.115f * col.b;
            return lum;
        }

    }

}