// credit to "ffleurey", they posted the json-array-thing here : https://forum.unity.com/threads/how-to-load-an-array-with-jsonutility.375735/

using UnityEngine;

public static class JsonHelper {

    public static T[] GetJsonArray<T> (string json) {
        string newJson = $"{{ \" {nameof(Wrapper<T>.array)} : {json} }}";
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] array;
    }
	
}
