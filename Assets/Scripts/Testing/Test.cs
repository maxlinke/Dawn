using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    [SerializeField] string a = default;
    [SerializeField] string b = default;

    [SerializeField] string error = default;
    [SerializeField] bool equals = false;
    [SerializeField] bool notEquals = false;
    [SerializeField] bool greater = false;
    [SerializeField] bool less = false;

    void Awake () {

    }

    void Update () {
        try{
            Version va = new Version(a);
            Version vb = new Version(b);
            equals = (va == vb);
            notEquals = (va != vb);
            greater = (va > vb);
            less = (va < vb);
            error = string.Empty;
        }catch(System.Exception e){
            equals = false;
            notEquals = false;
            greater = false;
            less = false;
            error = e.Message;
        }        
    }
	
}

