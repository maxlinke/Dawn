// credit to "ffleurey", they posted the json-array-thing here : https://forum.unity.com/threads/how-to-load-an-array-with-jsonutility.375735/

using UnityEngine;

public static class JsonHelper {

    public static string ToJsonArray<T> (T[] array, bool prettyPrint = false) {
        var wrapper = new Wrapper<T>();
        wrapper.array = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static T[] GetJsonArray<T> (string json) {
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] array;
    }
	
}
