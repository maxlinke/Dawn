using UnityEngine;

public class UnitAttribute : PropertyAttribute {

    public const float DEFAULT_LABEL_WIDTH = 40f;

    public readonly string name;
    public readonly float labelWidth;

    public UnitAttribute (string name, float labelWidth = DEFAULT_LABEL_WIDTH) {
        this.name = name;
        this.labelWidth = labelWidth;
    }
	
}