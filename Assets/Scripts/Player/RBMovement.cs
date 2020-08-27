﻿using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class RBMovement : Movement {

        [SerializeField] CapsuleCollider col = default;
        [SerializeField] Rigidbody rb = default;

        protected override Transform PlayerTransform => rb.transform;

        public override float LocalColliderHeight => col.height;
        public override float LocalColliderRadius => col.radius;
        public override Vector3 LocalColliderCenter => col.center;
        
        public override Vector3 Velocity { 
            get { return rb.velocity; }
            set { rb.velocity = value; }
        }

        public override ControlMode controlMode {
            get { return m_controlMode; }
            set { 
                if(value == ControlMode.ANCHORED){
                    rb.isKinematic = true;
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }else{
                    rb.isKinematic = false;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    rb.collisionDetectionMode = pcProps.CollisionDetection;
                }
                m_controlMode = value; 
            }
        }

        bool initialized = false;
        bool jumpInputCached = false;
        ControlMode m_controlMode = ControlMode.FULL;

        List<CollisionPoint> contactPoints;
        List<Collider> triggerStays;
        State lastState;

        public override void Initialize (PlayerControllerProperties pcProps, Transform head) {
            base.Initialize(pcProps, head);
            contactPoints = new List<CollisionPoint>();
            triggerStays = new List<Collider>();
            InitCol();
            InitRB();
            initialized = true;

            void InitCol () {
                PhysicMaterial pm = pcProps.PhysicMaterial;
                if(pm == null){
                    Debug.LogWarning("No default physic material for player assigned!");
                    pm = new PhysicMaterial();
                    pm.bounceCombine = PhysicMaterialCombine.Multiply;
                    pm.bounciness = 0;
                    pm.frictionCombine = PhysicMaterialCombine.Multiply;
                    pm.staticFriction = 0;
                    pm.dynamicFriction = 0;
                }
                col.sharedMaterial = pm;
            }

            void InitRB () {
                rb.mass = pcProps.PlayerMass;
                rb.useGravity = false;
                rb.drag = 0;
                rb.angularDrag = 0;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                this.controlMode = m_controlMode;
            }
        }

        void OnCollisionEnter (Collision collision) {
            CacheContacts(collision);
        }

        void OnCollisionStay (Collision collision) {
            CacheContacts(collision);
        }

        void OnTriggerStay (Collider otherCollider) {
            if(!initialized){
                return;
            }
            if(!triggerStays.Contains(otherCollider)){
                triggerStays.Add(otherCollider);
            }
        }

        void CacheContacts (Collision collision) {
            if(!initialized){
                return;
            }
            int cc = collision.contactCount;
            for(int i=0; i<cc; i++){
                contactPoints.Add(new CollisionPoint(collision.GetContact(i)));
            }
        }

        public void Move (bool readInput) {
            if(!initialized){
                Debug.LogWarning($"{nameof(RBMovement)} isn't initialized yet!");
                return;
            }
            StartMove(out var currentState);
            switch(controlMode){
                case ControlMode.FULL:
                    ExecuteMove(readInput, ref currentState);
                    break;
                case ControlMode.BLOCK_INPUT:
                    ExecuteMove(false, ref currentState);
                    break;
                case ControlMode.ANCHORED:
                    Velocity = Vector3.zero;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(ControlMode)} \"{controlMode}\"!");
                    Velocity = Vector3.zero;
                    break;
            }
            FinishMove(currentState);
        }

        void StartMove (out State currentState) {
            currentState = GetCurrentState(lastState, contactPoints, triggerStays);
            contactPoints.Clear();
            triggerStays.Clear();
            DEBUGTEXTFIELD.text = string.Empty;
            DEBUGTEXTFIELD.text += $"{currentState.surfaceAngle.ToString()}°\n";
            DEBUGTEXTFIELD.text += $"{currentState.surfaceDot.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.moveType.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.normedSurfaceFriction.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.incomingLocalVelocity.magnitude:F3} m/s\n";            
        }

        void FinishMove (State currentState) {Color lineColor;
            switch(currentState.moveType){
                case MoveType.AIR:
                    lineColor = Color.red;
                    break;
                case MoveType.GROUND: 
                    lineColor = Color.green;
                    break;
                case MoveType.SLOPE:
                    lineColor = Color.yellow;
                    break;
                case MoveType.WATER:
                    lineColor = Color.cyan;
                    break;
                default:
                    lineColor = Color.magenta;
                    break;
            }
            Debug.DrawLine(lastState.worldPosition, currentState.worldPosition, lineColor, 10f);
            lastState = currentState;
            jumpInputCached = false;
        }
        
        void ExecuteMove (bool readInput, ref State currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    AerialMovement(readInput, ref currentState);
                    break;
                case MoveType.GROUND:
                    GroundedMovement(readInput, ref currentState);
                    break;
                case MoveType.SLOPE:
                    SlopeMovement(readInput, ref currentState);
                    break;
                case MoveType.WATER:
                    WaterMovement(readInput, ref currentState);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.fixedDeltaTime;
                    break;
            }
            var gravityRotation = GetGravityRotation();
            var newRotation = Quaternion.RotateTowards(PlayerTransform.rotation, gravityRotation, Time.fixedDeltaTime * pcProps.GravityTurnDegreesPerSecond);
            var wcPosCache = WorldCenterPos;
            // rb.MoveRotation(newRotation);            // doesn't work. or it does, but not very well and only when i disable the view-rb-rotation-update
            PlayerTransform.rotation = newRotation;     // << laggy (because fixedupdate) but works
            WorldCenterPos = wcPosCache;
        }

        public void SetTryCrouch (bool value) {

        }

        public void ManageHeight (bool readInput) {
            // TODO binary crouch. yes or no. input.getkeydown and such. no getvalue
            // crouch target
            var currentNormedCrouch = (col.height - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight);
            if(readInput && controlMode == ControlMode.FULL){
                
            }
            if(lastState.surfacePoint != null){
                // down
            }else{
                // up
            }
        }

        // public void SetPositionToColliderBottom () {
        //     var bottomPos = WorldBottomPos;
        //     col.center = new Vector3(0f, col.height / 2f, 0f);
        //     PlayerTransform.position = bottomPos;
        // }

        public void CacheJumpInputIfNeeded () {
            if(!lastState.jumped && lastState.frame < Time.frameCount && lastState.moveType == MoveType.GROUND){
                jumpInputCached |= Bind.JUMP.GetKeyDown();
            }
        }

        void GroundedMovement (bool readInput, ref State currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            var frictionMag = Mathf.Lerp(pcProps.MinDrag, pcProps.GroundDrag, currentState.clampedNormedSurfaceFriction);
            var groundFriction = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, frictionMag, Time.fixedDeltaTime);
            groundFriction *= Time.fixedDeltaTime;
            Velocity += groundFriction;
            localVelocity += groundFriction;
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(RawTargetSpeed(readInput), localSpeed);
            var targetDirection = PlayerTransform.TransformDirection(rawInput);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            if(Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var tvSolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);    // <<< THIS!!!!! no ground snap needed, no extra raycasts. i still get launched slightly but it's negligible
                var tvNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(tvNonSolid, tvSolid, currentState.surfaceSolidness);
            }
            var accelMag = Mathf.Lerp(pcProps.MinAccel, pcProps.GroundAccel, currentState.clampedNormedSurfaceFriction);
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * accelMag, Time.fixedDeltaTime);
            if(readInput && (Bind.JUMP.GetKeyDown() || jumpInputCached)){
                var jumpMultiplier = Mathf.Lerp(1f - (currentState.surfaceAngle / 90f), 1f, currentState.clampedNormedSurfaceFriction);
                moveAccel += PlayerTransform.up * JumpSpeed() * jumpMultiplier / Time.fixedDeltaTime;
                currentState.jumped = true;
            }
            Vector3 gravity;
            if(currentState.jumped){
                gravity = Physics.gravity * 0.5f;
            }else{
                var stickGravity = -currentState.surfacePoint.normal * Physics.gravity.magnitude;
                var lerpFactor = currentState.surfaceSolidness * currentState.clampedNormedSurfaceFriction;
                lerpFactor *= Mathf.Clamp01(-1f * Vector3.Dot(Physics.gravity.normalized, PlayerTransform.up));
                gravity = Vector3.Slerp(Physics.gravity, stickGravity, lerpFactor);
            }
            Velocity += (moveAccel + gravity) * Time.fixedDeltaTime;
        }

        Vector3 WaterExitAcceleration (ref State currentState) {
            var verticalLocalVelocity = VerticalComponent(currentState.incomingLocalVelocity);
            if(verticalLocalVelocity.y > 0){
                var jumpVelocity = PlayerTransform.up * JumpSpeed();
                return (jumpVelocity - verticalLocalVelocity) / Time.fixedDeltaTime;
            }
            return Vector3.zero;
        }

        void SlopeMovement (bool readInput, ref State currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var frictionMag = Mathf.Lerp(pcProps.MinDrag, pcProps.SlopeDrag, currentState.clampedNormedSurfaceFriction);
            var slopeFriction = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, frictionMag, Time.fixedDeltaTime);
            slopeFriction *= Time.fixedDeltaTime;
            Velocity += slopeFriction;
            horizontalLocalVelocity += slopeFriction;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(RawTargetSpeed(readInput), horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            if(Vector3.Dot(targetVelocity, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var allowedMoveDirection = Vector3.Cross(currentState.surfacePoint.normal, PlayerTransform.up).normalized;
                targetVelocity = targetVelocity.ProjectOnVector(allowedMoveDirection);
            }
            var accelMag = Mathf.Lerp(pcProps.MinAccel, pcProps.SlopeAccel, currentState.clampedNormedSurfaceFriction);
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * accelMag, Time.fixedDeltaTime);
            if(Bind.JUMP.GetKey() && currentState.isInWater){
                moveAcceleration += WaterExitAcceleration(ref currentState).ProjectOnPlane(currentState.surfacePoint.normal);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

        void AerialMovement (bool readInput, ref State currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var dragDeceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, pcProps.AirDrag, Time.fixedDeltaTime);
            dragDeceleration *= Time.fixedDeltaTime;
            Velocity += dragDeceleration;
            horizontalLocalVelocity += dragDeceleration;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(RawTargetSpeed(readInput), horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.AirAccel, Time.fixedDeltaTime);
            if(Bind.JUMP.GetKey() && currentState.isInWater && currentState.touchingWall){
                moveAcceleration += WaterExitAcceleration(ref currentState);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

        void WaterMovement (bool readInput, ref State currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            var dragDeceleration = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, pcProps.WaterDrag, Time.fixedDeltaTime);
            dragDeceleration *= Time.fixedDeltaTime;
            Velocity += dragDeceleration;
            localVelocity += dragDeceleration;
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            if(readInput){
                var localUp = head.InverseTransformDirection(PlayerTransform.up);
                rawInput += localUp * Bind.JUMP.GetValue();
                rawInput -= localUp * (Bind.CROUCH_HOLD.GetValue() + Bind.CROUCH_TOGGLE.GetValue());
                if(rawInput.sqrMagnitude > 1f){
                    rawInput = rawInput.normalized;
                }
            }
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(RawTargetSpeed(readInput), localSpeed);
            var targetVelocity = head.TransformDirection(rawInput) * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.WaterAccel, Time.fixedDeltaTime);
            Velocity += moveAcceleration * Time.fixedDeltaTime; // no gravity in water.
        }

    }

}