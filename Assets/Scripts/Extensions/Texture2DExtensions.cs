using UnityEngine;

public static class Texture2DExtensions {

    public static void SetPixels32 (this Texture2D tex, Color32 color32, bool apply = true, bool updateMipmaps = true) {
        var cols32 = new Color32[tex.width * tex.height];
        for(int i=0; i<cols32.Length; i++){
            cols32[i] = color32;
        }
        tex.SetPixels32(cols32);
        if(apply){
            tex.Apply(updateMipmaps);
        }
    }

    public static void SetPixels (this Texture2D tex, Color color, bool apply = true, bool updateMipmaps = true) {
        tex.SetPixels32(color, apply, updateMipmaps);
    }
    
}
