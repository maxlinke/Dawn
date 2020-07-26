using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    void Start () {

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
            ExceptionalMethod();
        }
    }

    void ExceptionalMethod () {
        ReallyExceptionalMethod();
    }

    void ReallyExceptionalMethod () {
        DudeImTellingYouThisIsExceptional();
    }

    void DudeImTellingYouThisIsExceptional () {
        throw new System.ArgumentOutOfRangeException("This is a really long exception message, my dude. I'm just typing some text because I want a long message. Did I mention that this message should be long? Yeah, I did. I hope this is long enough...");   
    }
	
}
