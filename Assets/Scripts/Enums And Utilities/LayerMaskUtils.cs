using UnityEngine;

public static class LayerMaskUtils {

    public static readonly int EverythingMask = unchecked((int)0b_1111_1111_1111_1111_1111_1111_1111_1111);

	public static int CreateCollisionMask (int layer) {
		string[] names = new string[32];
		for(int i=0; i<32; i++){
			if(!Physics.GetIgnoreLayerCollision(layer, i)){
				names[i] = LayerMask.LayerToName(i);
			}
		}
		return LayerMask.GetMask(names);
	}

	public static int CreateCollisionMask (string layerName) {
		int layer = LayerMask.NameToLayer(layerName);
		return CreateCollisionMask(layer);
	}

    public static int CreateDirectMask (params Layer[] layers) {
        var output = 0;
        foreach(var layer in layers){
            output |= (1 << layer.index);
        }
        return output;
    }

    public static int CreateDirectMask (params int[] layerIndices) {
        var output = 0;
        foreach(var layerIndex in layerIndices){
            output |= (1 << layerIndex);
        }
        return output;
    }

	public static string MaskToBinaryString (int mask, bool firstCharacterIsFirstLayer = true) {
		string output = "";
		for(int i=0; i<32; i++){
			int andMask = 1 << i;
			string nextCharacter = (((mask & andMask) == 0) ? "0" : "1");
			if(firstCharacterIsFirstLayer){
				output = output + nextCharacter;
			}else{
				output = nextCharacter + output;
			}
		}
		return output;
	}

}
