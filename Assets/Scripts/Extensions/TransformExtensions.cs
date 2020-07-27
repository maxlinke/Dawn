using UnityEngine;

public static class TransformExtensions {

    public static bool TryGetUniformLocalScale (this Transform transform, out Vector3 scale) {
        var x = transform.localScale.x;
        var y = transform.localScale.y;
        var z = transform.localScale.z;
        var avg = (x + y + z) / 3f;
        scale = new Vector3(avg, avg, avg);
        return (x == y && x == z);
    }

    public static bool HasInHierarchy (this Transform thisTransform, Transform otherTransform) {
       while(otherTransform != null){
            if(otherTransform == thisTransform){
                return true;
            }
            otherTransform = otherTransform.parent;
        }
        return false;
    }

    public static bool HasInHierarchy(this Transform thisTransform, Component otherComponent) {
        if(otherComponent == null){
            return false;
        }
        return thisTransform.HasInHierarchy(otherComponent.transform);
    }

    public static bool HasInHierarchy(this Transform thisTransform, GameObject otherGameObject) {
        if(otherGameObject == null){
            return false;
        }
        return thisTransform.HasInHierarchy(otherGameObject.transform);
    }
	
}
