using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputSystem {

    public class Bind {

        public enum ID {          
            GAME_PAUSE_UNPAUSE,
            PLAYER_MOVE_FWD,
            PLAYER_MOVE_BWD,
            PLAYER_MOVE_LEFT,
            PLAYER_MOVE_RIGHT,
            PLAYER_LOOK_UP,         // TODO update the properties and switch
            PLAYER_LOOK_DOWN,
            PLAYER_LOOK_LEFT,
            PLAYER_LOOK_RIGHT
        }

        public static Bind GAME_PAUSE_UNPAUSE { get; private set; }
        public static Bind PLAYER_MOVE_FWD { get; private set; }
        public static Bind PLAYER_MOVE_BWD { get; private set; }
        public static Bind PLAYER_MOVE_LEFT { get; private set; }
        public static Bind PLAYER_MOVE_RIGHT { get; private set; }

        public static Bind GetBind (ID id) {
            switch(id){
                case ID.GAME_PAUSE_UNPAUSE: return GAME_PAUSE_UNPAUSE;
                case ID.PLAYER_MOVE_FWD: return PLAYER_MOVE_FWD;
                case ID.PLAYER_MOVE_BWD: return PLAYER_MOVE_BWD;
                case ID.PLAYER_MOVE_LEFT: return PLAYER_MOVE_LEFT;
                case ID.PLAYER_MOVE_RIGHT: return PLAYER_MOVE_RIGHT;
                default: 
                    Debug.LogError($"Unknown {nameof(ID)} \"{id}\"!");
                    return null;
            }
        }

        // TODO pretty much copy what i did with axis and axisconfig
        // save and load stuff via persistence and json (but not as explicitly done here)
        // more like iterate over all and save the ones that aren't immutable
        // loading via inputsystem to ensure no dupes

        // TODO also make some kind of ingame log so i can debug builds easier
        // for issues with saving and loading data...
        // would be cool if i could use reflection to literally call functions like the getlog for axisconfig

        // for loading, do the immutables first, then try to load the rest from disk
        // and if that ain't workin, do defaults.

        public readonly ID id;
        public readonly string name;

        private bool m_immutable;
        public bool immutable {
            get {
                return m_immutable;
            } set {
                m_immutable &= value;
            }
        }

        private Queue<InputMethod> inputs = new Queue<InputMethod>();

        public Bind (ID id, string name) {
            this.id = id;
            this.name = name;
            m_immutable = false;
        }

        // TODO limit number of inputs, do some returns for if can, what if added and replaced etc.
        // this shit isn't done realtime so we gucci fam
        // well, it is realtime but not every frame.

    	// inputsystem will have to manage duplicate-avoidance
        // shit, how do i go about categories? as in wasd for movement but also wasd for the tank?
        // i guess another category enum
        public void Add (InputMethod newInput) {
            if(immutable){
                return;     // < also complain
            }
            // if queue length is over limit, dequeue
            // OR add and dequeue until satisfied.
        }

        public bool Remove (InputMethod inputToRemove) {
            return false;
        }

        public void Clear () {
            inputs.Clear();
        }

        public bool Uses (InputMethod otherInput) {
            foreach(var input in inputs){
                if(input.Equals(otherInput)){
                    return true;
                }
            }
            return false;
        }

        public bool GetKeyDown () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Down;
            }
            return output;
        }

        public bool GetKey () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Hold;
            }
            return output;
        }

        public bool GetKeyUp () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Up;
            }
            return output;
        }

        public float GetValue () {
            var output = 0f;
            foreach(var input in inputs){
                // output = Mathf.Max(output, input.Value);
                output += input.Value;
            }
            return output;
        }

    }

}
