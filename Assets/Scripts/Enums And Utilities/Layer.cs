public class Layer {

    public static readonly Layer Default = new Layer(0, "Default");
    public static readonly Layer TransparentFX = new Layer(1, "TransparentFX");
    public static readonly Layer IgnoreRaycast = new Layer(2, "Ignore Raycast");
    // no layer 3
    public static readonly Layer Water = new Layer(4, "Water");
    public static readonly Layer UI = new Layer(5, "UI");
    // no layers 6 and 7
    public static readonly Layer PostProcessing = new Layer(8, "PostProcessing");
    public static readonly Layer PlayerControllerAndWorldModel = new Layer(9, "PlayerControllerAndWorldModel");

    public readonly int index;
    public readonly string name;

    public static implicit operator int (Layer l) => l.index;
    public static implicit operator string (Layer l) => l.name;

    private Layer (int index, string name) {
        this.index = index;
        this.name = name;
    }
	
}
