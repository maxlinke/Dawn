using UnityEngine;

// [CreateAssetMenu(fileName = "Rigidbody Mass Helper", menuName = "Rigidbody Mass Helper")]
public class RigidbodyMassHelper : ScriptableObject {

    const float inf = float.PositiveInfinity;

    [SerializeField, RangedUnit("kg", 0f, inf)] float mass = 1f;
    [SerializeField, RangedUnit("g/cm³", 0f, inf)] float materialDensity = 1f;
    [SerializeField, RangedUnit("m", 0f, inf)] float radius = 1f;
	
}