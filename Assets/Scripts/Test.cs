using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    void Start () {
        // Persistence.SaveFileHelper.DeleteAllData();
        // Persistence.FileHelper.OpenDataDirectory();
        var q = new Queue<int>();
        for(int i=0; i<10; i++){
            q.Enqueue(i);
        }
        var log = string.Empty;
        foreach(var element in q){
            log += $"{element}\n";
        }
        Debug.Log(log);
    }
	
}
