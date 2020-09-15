using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prop Collection", menuName = "Prop Collection")]
public class PropCollection : ScriptableObject {

    [SerializeField] Rigidbody[] props;

    public int Count => props != null ? props.Length : 0;

    public Rigidbody this[int index] => props[index];

    public IEnumerator<Rigidbody> GetEnumerator () {
        if(props != null){
            foreach(var rb in props){
                yield return rb;
            }
        }
    }
	
}
