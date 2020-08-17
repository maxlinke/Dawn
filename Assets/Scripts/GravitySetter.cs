using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GravitySetter : MonoBehaviour {

    [SerializeField] Vector3 targetGravityDirection = Vector3.down;
    [SerializeField] float targetGravityStrength = 9.81f;
    [SerializeField] bool setOnStart = true;

    void Start () {
        if(setOnStart){
            UpdateGravity();
        }
    }

    public void UpdateGravity () {
        Physics.gravity = targetGravityDirection * targetGravityStrength;
    }
	
}

#if UNITY_EDITOR
[CustomEditor(typeof(GravitySetter))]
public class GravitySetterEditor : Editor {

    GravitySetter gs;

    void OnEnable () {
        gs = target as GravitySetter;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(EditorApplication.isPlaying){
            GUILayout.Label($"current: {Physics.gravity}");
            if(GUILayout.Button("Update Gravity")){
                gs.UpdateGravity();
            }
        }
    }

}
#endif