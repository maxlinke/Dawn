using UnityEngine;

public class CustomRangeAttribute : PropertyAttribute, IRangeAttribute {

    public float fMin { get; private set; }
    public float fMax { get; private set; }
    public int iMin { get; private set; }
    public int iMax { get; private set; }

    public bool useSlider { get; private set; }

    public CustomRangeAttribute (float min, float max, bool useSlider = false) {
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
    }

    public CustomRangeAttribute (int min, int max, bool useSlider = false) {
        this.iMin = Mathf.Min(min, max);
        this.iMax = Mathf.Max(min, max);
        this.fMin = iMin;
        this.fMax = iMax;
        this.useSlider = useSlider;
    }
	
}
