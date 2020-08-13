using System.Collections.Generic;
using UnityEngine;

public class MultiTag : MonoBehaviour{

	[SerializeField] string[] tags;

    void Awake () {
        if(gameObject.tag != Tag.MultiTag){
            Debug.LogWarning($"GameObject \"{gameObject.name}\" has \"{nameof(MultiTag)}\" component, but is tagged \"{gameObject.tag}\"! This should already be \"{Tag.MultiTag}\", changing it now...");
            gameObject.tag = Tag.MultiTag;
        }
    }

    public IEnumerator<string> GetEnumerator () {
        foreach(var tag in tags){
            yield return tag;
        }
    }

    public bool HasTag (string otherTag) {
        if(otherTag == null){
            return false;
        }
        foreach(var tag in tags){
            if(otherTag.Equals(tag)){
                return true;
            }
        }
        return false;
    }

}
