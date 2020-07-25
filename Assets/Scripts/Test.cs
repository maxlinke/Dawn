using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    void Start () {
        // Persistence.SaveFileHelper.DeleteAllData();
        // Persistence.FileHelper.OpenDataDirectory();
    }

    void Update () {
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            Debug.Log(Random.value);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            Debug.LogWarning(Random.value);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            Debug.LogError(Random.value);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)){
            throw new System.Exception(Random.value.ToString());
        }
    }
	
}
