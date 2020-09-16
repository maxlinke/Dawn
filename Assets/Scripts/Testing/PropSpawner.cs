using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : MonoBehaviour {

    [Header("Source")]
    [SerializeField] PropCollection props = default;

    [Header("Settings")]
    [SerializeField] float spacing = 1f;
    [SerializeField] float zigZag = 0f;
    [SerializeField] Vector3 startEuler = Vector3.zero;
    [SerializeField] Vector3 startVelocity = Vector3.zero;

    [Header("Gizmos")]
    [SerializeField] Color gizmoColor = Color.green;
    [SerializeField] float gizmoSize = 0.1f;

    void Start () {
        if(props != null){
            int i = 0;
            var r = Quaternion.Euler(startEuler);
            foreach(var p in GetSpawnPoints(props.Count)){
                var prefab = props[i];
                if(prefab != null){
                    var newProp = Instantiate(prefab, p, r, this.transform);
                    newProp.velocity = startVelocity;
                }
                i++;
            }
        }
    }

    public IEnumerable<Vector3> GetSpawnPoints (int count) {
        var spacingDelta = transform.right * spacing;
        var zigZagDelta = transform.forward * spacing * zigZag;
        var origin = transform.position - (spacingDelta * 0.5f * (count- 1)) - (zigZagDelta * 0.5f);
        var current = origin;
        for(int i=0; i<count; i++){
            yield return current;
            current += spacingDelta + zigZagDelta;
            zigZagDelta *= -1f;
        }
    }

    void OnDrawGizmosSelected () {
        if(props != null){
            var gc = Gizmos.color;
            Gizmos.color = gizmoColor;
            var cs = Vector3.one * gizmoSize;
            foreach(var p in GetSpawnPoints(props.Count)){
                Gizmos.DrawCube(p, cs);
                Gizmos.DrawRay(p, startVelocity);
            }
            Gizmos.color = gc;
        }
    }
	
}
