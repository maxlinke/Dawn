using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugTools {

    public class FramerateDisplay : MonoBehaviour {

        [SerializeField] Canvas canvas;
        [SerializeField] RawImage image;

        bool visible {
            get {
                return canvas.enabled;
            } set {
                if(canvas.enabled != value){
                    canvas.enabled = value;
                    if(value){
                        OnShow();
                    }else{
                        OnHide();
                    }
                }
            }
        }

        void Start () {
            
        }

        void Update () {
            
        }

        void OnShow () {

        }

        void OnHide () {

        }
        
    }

}