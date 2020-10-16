using UnityEngine;

namespace PlayerController {

    public abstract partial class Movement {

        public struct CrouchControlInput {
            public bool toggleCrouch;
            public bool crouch;
            public bool uncrouch;

            public static CrouchControlInput None { get {
                CrouchControlInput output;
                output.toggleCrouch = false;
                output.crouch = false;
                output.uncrouch = false;
                return output;
            } }
        }

        public void UpdateCrouchState (CrouchControlInput ccInput, MoveState referenceState) {
            if(referenceState.isInWater && !referenceState.canCrouchInWater){
                shouldCrouch = false;
                return;
            }
            if(controlMode != ControlMode.FULL){
                ccInput = CrouchControlInput.None;
            }
            if(ccInput.toggleCrouch){
                shouldCrouch = !shouldCrouch;
            }
            if(ccInput.crouch){
                shouldCrouch = true;
            }
            if(ccInput.uncrouch){
                shouldCrouch = false;
            }
        }

        protected bool CanUncrouch (bool onGround) {
            if(onGround){
                return UncrouchPathFree(checkUpward: true);
            }else{
                if(!UncrouchPathFree(checkUpward: false)){
                    return UncrouchPathFree(checkUpward: true);
                }
            }
            return true;
        }

        protected bool UncrouchPathFree (bool checkUpward) {
            Vector3 rayStart, rayDir;
            if(checkUpward){
                rayStart = PlayerTransform.TransformPoint(LocalColliderTopSphere);
                rayDir = PlayerTransform.up;
            }else{
                rayStart = PlayerTransform.TransformPoint(LocalColliderBottomSphere);
                rayDir = -PlayerTransform.up;
            }
            var offset = 0.05f;
            var scale = PlayerTransform.lossyScale.Average();
            var rayRadius = (LocalColliderRadius * scale) - offset;
            var rayLength = ((props.NormalHeight - LocalColliderHeight) * scale) + offset;
            if(Physics.SphereCast(rayStart, rayRadius, rayDir, out var hit, rayLength, collisionCastMask)){
                // could do a mass check here if hit is a rigidbody
                return false;
            }
            return true;
        }

        public void UpdateColliderSizeIfNeeded (MoveState currentState, bool instantly, bool forceUpdate = false) {
            float currentHeight = LocalColliderHeight;
            bool noHeightUpdateNeeded = false;
            noHeightUpdateNeeded |= (shouldCrouch && currentHeight == props.CrouchHeight);
            noHeightUpdateNeeded |= (!shouldCrouch && currentHeight == props.NormalHeight);
            if(noHeightUpdateNeeded && !forceUpdate){
                return;
            }
            float targetHeight;
            if(shouldCrouch || !CanUncrouch(currentState.touchingGround)){
                targetHeight = props.CrouchHeight;
            }else{
                targetHeight = props.NormalHeight;
            }
            float deltaHeight = targetHeight - currentHeight;
            if(!instantly){
                float maxDelta = props.HeightChangeSpeed * Time.deltaTime;
                if(Mathf.Abs(deltaHeight) > maxDelta){
                    deltaHeight = Mathf.Sign(deltaHeight) * maxDelta;
                }
            }
            LocalColliderHeight += deltaHeight;
            LocalColliderCenter = new Vector3(0f, LocalColliderHeight / 2f, 0f);
            if(currentState.touchingGround){
                OnColliderSizeUpdated(true);
            }else{
                PlayerTransform.position += PlayerTransform.up * deltaHeight * -1f;
                OnColliderSizeUpdated(false);
            }
        }

        protected abstract void OnColliderSizeUpdated (bool onGround);

    }

}