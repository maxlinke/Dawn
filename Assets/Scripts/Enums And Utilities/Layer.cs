public class Layer {

    public static readonly Layer Default = new Layer(0, "Default");
    public static readonly Layer TransparentFX = new Layer(1, "TransparentFX");
    public static readonly Layer IgnoreRaycast = new Layer(2, "Ignore Raycast");
    // no layer 3
    public static readonly Layer Water = new Layer(4, "Water");
    public static readonly Layer UI = new Layer(5, "UI");
    // no layers 6 and 7
    public static readonly Layer PostProcessing = new Layer(8, "PostProcessing");
    public static readonly Layer PlayerController = new Layer(9, "PlayerController");

    public readonly int index;
    public readonly string name;

    private Layer (int index, string name) {
        this.index = index;
        this.name = name;
    }
	
}
