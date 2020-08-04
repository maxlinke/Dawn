using UnityEngine;

namespace PlayerController {

    public class Inputs : MonoBehaviour {

        public MovementInputs movementInputs { get; private set; }
        public ViewInputs viewInputs { get; private set; }
        
        private int lastInputFrame = -1;

        public void Initialize () {
            // TODO keybinds etc
            // should probably subscribe to whatever mananges that stuff
        }

        public void ExecuteUpdate () {
            GatherInputs();
        }

        public void ExecuteFixedUpdate () {
            GatherInputs();
        }

        void GatherInputs () {
            if(Time.frameCount == lastInputFrame){
                return;
            }
            lastInputFrame = Time.frameCount;

        }

        public struct MovementInputs {
            public readonly Vector3 localMoveDir;       // also contain up/down for swimming? (jump/crouch, usually discarded when flattening the vector)
            public readonly bool shouldWalk;
            public readonly bool shouldCrouch;
            public readonly bool shouldJump;
            public MovementInputs (Vector3 localMoveDir, bool shouldWalk, bool shouldCrouch, bool shouldJump) {
                this.localMoveDir = localMoveDir;
                this.shouldWalk = shouldWalk;
                this.shouldCrouch = shouldCrouch;
                this.shouldJump = shouldJump;
            }
        }

        public struct ViewInputs {
            public readonly Vector2 viewDirDelta;
            public ViewInputs (Vector2 viewDirDelta) {
                this.viewDirDelta = viewDirDelta;
            }
        }
        
    }

}