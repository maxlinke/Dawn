using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;

public class UIAxisDebug : MonoBehaviour {

    [Header("Important")]
    [SerializeField] Axis.ID axisID = default;

    [Header("Less important")]
    [SerializeField] RawImage image = default;
    [SerializeField] Text textField = default;
    [SerializeField] Text valueField = default;
    [SerializeField] Color clearColor = Color.black;
    [SerializeField] Color drawColor = Color.magenta;
    [SerializeField] Color referenceColor = Color.cyan;
    [SerializeField] Color fixedUpdateColor = Color.grey;
    [SerializeField] float scaling = 1.5f;

    RectTransform imageRT => image.rectTransform;
    Texture2D tex;
    int texWidth;
    int texHeight;
    Color32[] pixels;
    int nextX = 0;
    int lastY = 0;

    void Start () {
        if(image == null){
            return;
        }
        ResetTexture();
        image.texture = tex;
    }

    void Update () {
        var input = InputSystem.CurrentlyHeldInput();
        if(input == null){
            textField.text = string.Empty;
        }else{
            textField.text = input.Name;
        }
        textField.color = drawColor;
        valueField.color = drawColor;
        DrawGraph();

        void DrawGraph () {
            if(tex == null){
                return;
            }
            if(nextX == 0){
                for(int i=0; i<pixels.Length; i++){
                    pixels[i] = clearColor;
                }
            }
            float drawVal = Axis.GetAxis(axisID).GetUnsmoothed();
            valueField.text = drawVal.ToString();
            pixels[ToIndex(nextX, ToScaledY(0))] = referenceColor;
            pixels[ToIndex(nextX, ToScaledY(1))] = referenceColor;
            pixels[ToIndex(nextX, ToScaledY(-1))] = referenceColor;
            int currentY = ToScaledY(drawVal);
            int minY = Mathf.Min(currentY, lastY);
            int maxY = Mathf.Max(currentY, lastY);
            for(int y=minY; y<=maxY; y++){
                if(y >= 0 && y < texHeight){
                    pixels[ToIndex(nextX, y)] = drawColor;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
            nextX = (nextX + 1) % texWidth;
            lastY = currentY;

            int ToScaledY (float inputValue) {
                return (int)(((inputValue / (2f * scaling)) + 0.5f) * texHeight);
            }

            int ToIndex (int x, int y) {
                return (y * texWidth) + x;
            }
        }
    }

    void FixedUpdate () {
        var pl = pixels.Length;
        for(int i = nextX; i<pl; i+=texWidth){
            pixels[i] = fixedUpdateColor;
        }
    }

    [ContextMenu("Reset Texture")]
    void ResetTexture () {
        if(tex != null){
            tex.Resize(
                width: (int)(imageRT.rect.width), 
                height: (int)(imageRT.rect.height)
            );
        }else{
            tex = new Texture2D(
                width: (int)(imageRT.rect.width), 
                height: (int)(imageRT.rect.height), 
                textureFormat: TextureFormat.RGBA32,
                mipChain: false,
                linear: false
            );
        }
        texWidth = tex.width;
        texHeight = tex.height;
        tex.SetPixels(clearColor, false, false);
        pixels = tex.GetPixels32();
        nextX = 0;
    }
	
}
