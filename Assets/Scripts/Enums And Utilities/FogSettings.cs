using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct FogSettings {

    [SerializeField] public bool fogEnabled;
    [SerializeField] public FogMode fogMode;
    [SerializeField] public Color fogColor;
    [SerializeField] public float fogDensity;
    [SerializeField] public float fogStartDistance;
    [SerializeField] public float fogEndDistance;

    public void Apply () {
        RenderSettings.fog = fogEnabled;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

    public static FogSettings Default { get {
        FogSettings output;
        output.fogEnabled       = true;
        output.fogMode          = FogMode.Exponential;
        output.fogColor         = Color.grey;
        output.fogDensity       = 0.01f;
        output.fogStartDistance = 10f;
        output.fogEndDistance   = 100f;
        return output;
    } }

    public static FogSettings Off { get {
        FogSettings output;
        output.fogEnabled       = false;
        output.fogMode          = FogMode.Exponential;
        output.fogColor         = Color.clear;
        output.fogDensity       = 0f;
        output.fogStartDistance = 0f;
        output.fogEndDistance   = float.PositiveInfinity;
        return output;
    } }

    public static FogSettings GetCurrent () {
        FogSettings output;
        output.fogEnabled =       RenderSettings.fog;
        output.fogMode =          RenderSettings.fogMode;
        output.fogColor =         RenderSettings.fogColor;
        output.fogDensity =       RenderSettings.fogDensity;
        output.fogStartDistance = RenderSettings.fogStartDistance;
        output.fogEndDistance =   RenderSettings.fogEndDistance;
        return output;
    }

    public static FogSettings Lerp (FogSettings a, FogSettings b, float lerp) {
        return LerpUnclamped(a, b, Mathf.Clamp01(lerp));
    }

    public static FogSettings LerpUnclamped (FogSettings a, FogSettings b, float lerp) {
        FogSettings output;
        if(lerp <= 0f){
            output.fogEnabled = a.fogEnabled;
        }else if(lerp >= 1f){
            output.fogEnabled = b.fogEnabled;
        }else{
            output.fogEnabled = a.fogEnabled || b.fogEnabled;
        }
        output.fogMode = (lerp < 0.5f ? a.fogMode : b.fogMode);
        output.fogColor = Color.LerpUnclamped(a.fogColor, b.fogColor, lerp);
        output.fogDensity = Mathf.LerpUnclamped(a.fogDensity, b.fogDensity, lerp);
        output.fogStartDistance = Mathf.LerpUnclamped(a.fogStartDistance, b.fogStartDistance, lerp);
        output.fogEndDistance = Mathf.LerpUnclamped(a.fogEndDistance, b.fogEndDistance, lerp);
        return output;
    }

    public override bool Equals (object obj) {
        if(obj == null){
            return false;
        }
        if(obj is FogSettings other){
            return (other.fogEnabled       == this.fogEnabled)
                && (other.fogMode          == this.fogMode)
                && (other.fogColor         == this.fogColor)
                && (other.fogDensity       == this.fogDensity)
                && (other.fogStartDistance == this.fogStartDistance)
                && (other.fogEndDistance   == this.fogEndDistance);

        }
        return false;
    }

    public override int GetHashCode () {
        return base.GetHashCode();
    }

    public override string ToString () {
        return base.ToString();
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(FogSettings))]
public class FogSettingsDrawer : PropertyDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        var guiEnabledCache = GUI.enabled;
        var lw = EditorGUIUtility.labelWidth;
        var slh = EditorGUIUtility.singleLineHeight;
        var svs = EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PrefixLabel(position, label);
        EditorGUI.indentLevel++;
        DrawProperties();
        EditorGUI.indentLevel--;
        GUI.enabled = guiEnabledCache;
        EditorGUI.EndProperty();

        void DrawProperties () {
            var fogOnProp = property.FindPropertyRelative(nameof(FogSettings.fogEnabled));
            EditorGUI.PropertyField(NextLine(), fogOnProp);
            var localGUIEnabled = guiEnabledCache && fogOnProp.boolValue;
            GUI.enabled = localGUIEnabled;
            EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative(nameof(FogSettings.fogColor)));
            var modeProp = property.FindPropertyRelative(nameof(FogSettings.fogMode));
            EditorGUI.PropertyField(NextLine(), modeProp);
            var fm = (FogMode)(modeProp.enumValueIndex + 1);    // for some godforsaken reason it's +1. is it because the enum doesn't start at 0 ? 
            GUI.enabled = localGUIEnabled && (fm == FogMode.Exponential || fm == FogMode.ExponentialSquared);
            EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative(nameof(FogSettings.fogDensity)));
            GUI.enabled = localGUIEnabled && (fm == FogMode.Linear);
            EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative(nameof(FogSettings.fogStartDistance)));
            EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative(nameof(FogSettings.fogEndDistance)));
        }

        Rect NextLine () {
            position = new Rect(position.x, position.y + slh + svs, position.width, position.height - slh - svs);
            return new Rect(position.x, position.y, position.width, slh);
        }
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return 7 * EditorGUIUtility.singleLineHeight + 6 * EditorGUIUtility.standardVerticalSpacing;
    }

}

#endif