using UnityEngine;

public class Layer {

    public static readonly Layer Everything = new Layer(-1, "Everything");

    public static readonly Layer Default = new Layer(0, "Default");
    public static readonly Layer TransparentFX = new Layer(1, "TransparentFX");
    public static readonly Layer IgnoreRaycast = new Layer(2, "Ignore Raycast");
    public static readonly Layer Water = new Layer(4, "Water");
    public static readonly Layer UI = new Layer(5, "UI");
    public static readonly Layer PostProcessing = new Layer(8, "PostProcessing");

    public static readonly Layer PlayerControllerAndFirstPersonModel = new Layer(9, "PlayerControllerAndFirstPersonModel");
    public static readonly Layer Prop = new Layer(10, "Prop");
    public static readonly Layer SmallProp = new Layer(11, "SmallProp");
    public static readonly Layer EntityGround = new Layer(12, "EntityGround");
    public static readonly Layer PropGround = new Layer(13, "PropGround");
    public static readonly Layer FreeCam = new Layer(14, "FreeCam");
    public static readonly Layer PlayerHitboxesAndThirdPersonModel = new Layer(15, "PlayerHitboxesAndThirdPersonModel");    // TODO move this under layer 9 sooner than later!

    public readonly int index;
    public readonly string name;
    public readonly int mask;

    public static implicit operator int (Layer l) => l.index;
    public static implicit operator string (Layer l) => l.name;

    private Layer (int index, string name) {
        this.index = index;
        this.name = name;
        if(index == -1){
            this.mask = -1;
        }else{
            this.mask = 1 << index;
        }
    }

    public int CalculateCollisionMask () {
        int output = 0;
        for(int i=0; i<32; i++){
			if(!Physics.GetIgnoreLayerCollision(this.index, i)){
				output |= (1 << i);
			}
		}
        return output;
    }

    public static string MaskToString (int mask) {
        return System.Convert.ToString(mask, 2);
    }
	
}
