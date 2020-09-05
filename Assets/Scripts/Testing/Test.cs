using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    class ASDF { 
        public string name;
        public ASDF (string name) {
            this.name = name;
        }
    }

    void Awake () {
        Dictionary<ASDF, int> map = new Dictionary<ASDF, int>();
        var a = new ASDF("a");
        var b = new ASDF("b");
        map.Add(a, 0);
        map.Add(b, 1);
        LogMapContents();
        map.Add(a, 2);
        LogMapContents();        

        void LogMapContents () {
            string log = "keys:";
            foreach(var key in map.Keys){
                log += $"\n{key.name}";
            }
            log += "\nvalues:";
            foreach(var value in map.Values){
                log += $"\n{value}";
            }
            Debug.Log(log);
        }
    }

    void Update () {
        
    }
	
}
