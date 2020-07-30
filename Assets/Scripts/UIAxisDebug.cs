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
    [SerializeField] float scaling = 1.5f;

    RectTransform imageRT => image.rectTransform;
    Texture2D tex;

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
                ClearTexture();
            }
            float drawVal = Axis.GetAxis(axisID).GetUnsmoothed();
            valueField.text = drawVal.ToString();
            tex.SetPixel(nextX, ToScaledY(0), referenceColor);
            tex.SetPixel(nextX, ToScaledY(1), referenceColor);
            tex.SetPixel(nextX, ToScaledY(-1), referenceColor);
            int currentY = ToScaledY(drawVal);
            int minY = Mathf.Min(currentY, lastY);
            int maxY = Mathf.Max(currentY, lastY);
            for(int y=minY; y<=maxY; y++){
                if(y >= 0 && y < tex.height){
                    tex.SetPixel(nextX, y, drawColor);
                }
            }
            tex.Apply(false);
            nextX = (nextX + 1) % tex.width;
            lastY = currentY;

            int ToScaledY (float inputValue) {
                return (int)(((inputValue / (2f * scaling)) + 0.5f) * tex.height);
            }
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
        ClearTexture();
        nextX = 0;
    }

    void ClearTexture () {
        tex.SetPixels(clearColor, true, false);
    }
	
}
