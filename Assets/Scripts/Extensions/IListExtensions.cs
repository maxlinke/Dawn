using System.Collections.Generic;

public static class IListExtensions {

    public static T GetFirst<T> (this IList<T> list) {
        return list[0];
    }

    public static T GetLast<T> (this IList<T> list) {
        return list[list.Count - 1];
    }

    public static T GetWrapped<T> (this IList<T> list, int index) {
        if(index < 0){
            index = list.Count - (-index % list.Count);
        }
        if(index >= list.Count){
            index = index % list.Count;
        }
        return list[index];
    }

    public static string ToStringPerElement<T> (this IList<T> list) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i=0; i<list.Count; i++){
            T t = list[i];
            if(t == null){
                sb.AppendLine($"{i}: null");
            }else{
                sb.AppendLine($"{i}: {t.ToString()}");
            }
        }
        return $"[{sb.ToString().Trim()}]";
    }
	
}
