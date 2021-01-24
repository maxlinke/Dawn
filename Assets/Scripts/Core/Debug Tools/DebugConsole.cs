using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugTools.DebugCommands;
using CustomInputSystem;
using UnityEngine.EventSystems;

namespace DebugTools {

    public class DebugConsole : MonoBehaviour, ICoreComponent {

        [Header("Settings")]
        [SerializeField] bool selfInit = false;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] ScrollingTextDisplay textDisplay = default;

        static DebugConsole instance;

        // literally the same as debuglog and frameratedisplay. do i smell some inheritance?
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

        void Awake () {
            if(selfInit){
                InitializeCoreComponent(null);
            }
        }

        void OnDestroy () {
            if(instance == this){
                instance = null;
            }
        }

        void OnShow () {
            textDisplay.SetGOActive(true);
        }

        // TODO this eventsystem thing can also go in the inheritance...
        void OnHide () {
            textDisplay.SetGOActive(false);
            if(EventSystem.current != null){
                if(this.transform.HasInHierarchy(EventSystem.current.currentSelectedGameObject)){
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        // TODO the custom input override thingy for the ui event system
        // and of course binds to go with it
        // can't i put the cursorlockmanager stuff into the onshow and onhide?

        void Update () {
            if(Bind.TOGGLE_DEBUG_CONSOLE.GetKeyDown()){
                visible = !visible;
                if(visible){
                    CursorLockManager.AddUnlocker(this);
                }else{
                    CursorLockManager.RemoveUnlocker(this);
                }
            }
        }

        public void InitializeCoreComponent(IEnumerable<ICoreComponent> allCoreComponents) {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(DebugConsole)} is not null!");
                Destroy(this.gameObject);
                return;
            }

        }


        
    }

}