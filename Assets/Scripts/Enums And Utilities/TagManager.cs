using System.Collections.Generic;
using UnityEngine;

public static class TagManager{

    private const int MAX_MULTI_TAG_CACHE_SIZE = 32;

    private static Dictionary<GameObject, MultiTag> multiTagCache = new Dictionary<GameObject, MultiTag>();
    private static Queue<GameObject> cachedQueue = new Queue<GameObject>();

    public static void CacheMultiTag (GameObject obj, MultiTag multiTag) {
        try{
            multiTagCache.Add(obj, multiTag);
            cachedQueue.Enqueue(obj);
            if(cachedQueue.Count > MAX_MULTI_TAG_CACHE_SIZE){
                multiTagCache.Remove(cachedQueue.Dequeue());
            }
        }catch(System.Exception e){
            Debug.LogError(e.Message);
        }
    }

    public static void RemoveCachedMultiTag (GameObject obj) {
        if(multiTagCache.ContainsKey(obj)){
            multiTagCache.Remove(obj);
        }
    }

	public static bool CompareTag (string tag, GameObject obj) {
        if(obj == null){
            return false;
        }
		if(obj.CompareTag(tag)){
			return true;
		}
        if(obj.CompareTag(Tag.MultiTag)){
            MultiTag multiTag;
            if(!multiTagCache.TryGetValue(obj, out multiTag)){
                multiTag = obj.GetComponent<MultiTag>();
                CacheMultiTag(obj, multiTag);
            }
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
