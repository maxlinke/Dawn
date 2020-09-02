using System.Collections.Generic;
using UnityEngine;

public static class TagManager{

	public static bool CompareTag (string tag, GameObject obj) {
        if(obj == null){
            return false;
        }
		if(obj.CompareTag(tag)){
			return true;
		}
        if(obj.CompareTag(Tag.MultiTag)){
			var multiTag = obj.GetComponent<MultiTag>();
			if(multiTag == null){
				Debug.LogError($"GameObject \"{obj.name}\" has tag \"{Tag.MultiTag}\" but no \"{nameof(MultiTag)}\"-Component!");
                return false;
			}
            return multiTag.HasTag(tag);
		}
        return false;
	}

	public static GameObject[] FindWithTag (string tag) {
		if(tag.Equals(Tag.MultiTag)){
			return GameObject.FindGameObjectsWithTag(Tag.MultiTag);
		}else{
            var multi = GameObject.FindGameObjectsWithTag(Tag.MultiTag);
			var regular = GameObject.FindGameObjectsWithTag(tag);
			var output = new List<GameObject>();
			output.AddRange(regular);
			foreach(var mt in multi){
				if(CompareTag(tag, mt)){
					output.Add(mt);
				}
			}
			return output.ToArray();
		}
	}

}
