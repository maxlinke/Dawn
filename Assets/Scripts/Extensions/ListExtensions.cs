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
	
}
