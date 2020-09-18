using UnityEngine;

public class RangedUnitAttribute : PropertyAttribute, IRangeAttribute {

    public readonly string name;
    public readonly float labelWidth;

    public float fMin { get; private set; }
    public float fMax { get; private set; }
    public int iMin { get; private set; }
    public int iMax { get; private set; }

    public bool useSlider { get; private set; }

    public RangedUnitAttribute (string name, float min, float max, bool useSlider = false, float labelWidth = UnitAttribute.DEFAULT_LABEL_WIDTH) {
        this.name = name;
        if(float.IsNaN(min)){
            min = 0f;
        }
        if(float.IsNaN(max)){
            max = 0f;
        }
        this.fMin = Mathf.Min(min, max);
        this.fMax = Mathf.Max(min, max);
        this.iMin = (int)fMin;
        this.iMax = (int)fMax;
        this.useSlider = useSlider;
        this.labelWidth = labelWidth;
    }

    public RangedUnitAttribute (string name, int min, int max, bool useSlider = false, float labelWidth = UnitAttribute.DEFAULT_LABEL_WIDTH) {
        this.name = name;
        this.iMin = Mathf.Min(min, max);
        this.iMax = Mathf.Max(min, max);
        this.fMin = iMin;
        this.fMax = iMax;
        this.useSlider = useSlider;
        this.labelWidth = labelWidth;
    }
	
}