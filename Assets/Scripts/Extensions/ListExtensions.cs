using System.Collections.Generic;

public static class ListExtensions {

    public static void RemoveNullEntries<T> (this List<T> list) where T : class {
        for(int i=0; i<list.Count; i++){
            if(list[i] == null){
                list.RemoveAt(i);
                i--;
            }
        }
    }

    public static string ContentsToString<T> (this List<T> list) {
        string output = string.Empty;
        for(int i=0; i<list.Count; i++){
            T t = list[i];
            if(t == null){
                output += $"{i} null\n";
            }else{
                output += $"{i} : {t.ToString()}\n";
            }
        }
        return output;
    }
	
}
