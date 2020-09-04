using System.Collections.Generic;
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

        protected Vector3 TargetSmoothRotationParentPos => col.center;
        protected Vector3 TargetHeadPos => new Vector3(0f, 0.5f * col.height + pcProps.EyeOffset, 0f);
        protected Vector3 TargetModelPos => new Vector3(0f, -0.5f * col.height, 0f);
        
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
        bool cachedJumpKeyDown = false;
        Transform smoothRotationParent;
        ControlMode m_controlMode = ControlMode.FULL;

        List<CollisionPoint> contactPoints;
        List<Collider> triggerStays;

        public void Initialize (PlayerControllerProperties pcProps, Transform head, Transform model, Transform smoothRotationParent) {
            base.Init(pcProps, head, model);
            this.smoothRotationParent = smoothRotationParent;
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
            UpdateColliderSizeIfNeeded(currentState, Time.fixedDeltaTime);
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

        void StartMove (out MoveState currentState) {
            currentState = GetCurrentState(contactPoints, triggerStays);
            contactPoints.Clear();
            triggerStays.Clear();
            DEBUGTEXTFIELD.text = string.Empty;
            DEBUGTEXTFIELD.text += $"{currentState.surfaceAngle.ToString()}°\n";
            DEBUGTEXTFIELD.text += $"{currentState.surfaceDot.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.moveType.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{(currentState.surfacePoint == null ? "null" : currentState.surfacePoint.GetName())}\n";
            DEBUGTEXTFIELD.text += $"{currentState.normedSurfaceFriction.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.incomingLocalVelocity.magnitude:F3} m/s\n";            
            DEBUGTEXTFIELD.text += $"crouch: {shouldCrouch}\n";
            DEBUGTEXTFIELD.text += $"height: {col.height}\n";
        }

        void FinishMove (MoveState currentState) {Color lineColor;
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
                case MoveType.LADDER:
                    lineColor = Color.blue;
                    break;
                default:
                    lineColor = Color.magenta;
                    break;
            }
            Debug.DrawLine(lastState.worldPosition, currentState.worldPosition, lineColor, 10f);
            lastState = currentState;
            cachedJumpKeyDown = false;
        }
        
        void ExecuteMove (bool readInput, ref MoveState currentState) {
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
                    SetTryCrouch(false);
                    WaterMovement(readInput, ref currentState);
                    break;
                case MoveType.LADDER:
                    SetTryCrouch(false);
                    LadderMovement(readInput, ref currentState);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.fixedDeltaTime;
                    break;
            }
        }

        public void UpdateColliderSizeIfNeeded (MoveState currentState, float timeStep) {
            bool noHeightUpdateNeeded = false;
            noHeightUpdateNeeded |= (shouldCrouch && col.height == pcProps.CrouchHeight);
            noHeightUpdateNeeded |= (!shouldCrouch && col.height == pcProps.NormalHeight);
            if(noHeightUpdateNeeded){
                return;
            }
            float targetHeight;
            if(shouldCrouch || !CanUncrouch(checkUpward: currentState.surfacePoint != null)){
                targetHeight = pcProps.CrouchHeight;
            }else{
                targetHeight = pcProps.NormalHeight;
            }
            float deltaHeight = targetHeight - col.height;
            float maxDelta = pcProps.HeightChangeSpeed * timeStep;
            if(Mathf.Abs(deltaHeight) > maxDelta){
                deltaHeight = Mathf.Sign(deltaHeight) * maxDelta;
            }
            col.height += deltaHeight;
            col.center = new Vector3(0f, col.height / 2f, 0f);
            var srpDelta = TargetSmoothRotationParentPos - smoothRotationParent.localPosition;
            smoothRotationParent.localPosition += srpDelta;
            if(currentState.surfacePoint == null || lastState.jumped){
                PlayerTransform.position += PlayerTransform.up * deltaHeight * -1f;
                head.localPosition += srpDelta;
                model.localPosition += srpDelta;
            }else{
                head.localPosition -= srpDelta;
                model.localPosition -= srpDelta;
            }
        }

        public void UpdateHeadAndModelPosition (bool instantly) {
            var srpDelta = TargetSmoothRotationParentPos - smoothRotationParent.localPosition;
            smoothRotationParent.localPosition += srpDelta;
            if(instantly){
                head.localPosition = TargetHeadPos;
                model.localPosition = TargetModelPos;
            }else{
                head.localPosition -= srpDelta;
                model.localPosition -= srpDelta;
                var maxDelta = pcProps.HeightChangeSpeed * Time.deltaTime;
                var deltaHead = TargetHeadPos - head.localPosition;
                var deltaModel = TargetModelPos - model.localPosition;
                if(deltaHead.sqrMagnitude > (maxDelta * maxDelta)){
                    deltaHead = deltaHead.normalized * maxDelta;
                }
                if(deltaModel.sqrMagnitude > (maxDelta * maxDelta)){
                    deltaModel = deltaModel.normalized * maxDelta;
                }
                head.localPosition += deltaHead;
                model.localPosition += deltaModel;
            }
        }

        public void AlignWithGravityIfAllowed (float timeStep) {
            if(controlMode == ControlMode.ANCHORED){
                return;
            }
            var gravityRotation = GetGravityRotation(smoothRotationParent);
            var newRotation = Quaternion.RotateTowards(smoothRotationParent.rotation, gravityRotation, timeStep * pcProps.GravityTurnDegreesPerSecond);
            smoothRotationParent.rotation = newRotation;
        }

        public void ApplySubRotation () {
            var wcPos = WorldCenterPos;
            PlayerTransform.rotation = smoothRotationParent.rotation;
            smoothRotationParent.localRotation = Quaternion.identity;
            WorldCenterPos = wcPos;
        }

        public void CacheSingleFrameInputs () {
            if(Time.frameCount == lastState.frame){
                return;
            }
            // either check states where JUMPING is possible here
            // or the ones where you can't jump (i.e. AIR, WATER, SLOPE)
            if(!lastState.jumped && (lastState.moveType == MoveType.GROUND || lastState.moveType == MoveType.LADDER)){
                cachedJumpKeyDown |= Bind.JUMP.GetKeyDown();
            }
        }

        void ApplyDrag (float drag, ref Vector3 localVelocity) {
            ApplyDrag(drag, Time.fixedDeltaTime, ref localVelocity);
        }

        void GroundedMovement (bool readInput, ref MoveState currentState) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetDirection = PlayerTransform.TransformDirection(rawInput);
            if(currentState.ladderPoint != null && currentState.ladderPoint != currentState.surfacePoint){
                if(Vector3.Dot(targetDirection.normalized, currentState.ladderPoint.normal) < 0f){
                    LadderMovement(readInput, ref currentState);
                    return;
                }
            }
            var localVelocity = currentState.incomingLocalVelocity;
            var frictionMag = Mathf.Lerp(pcProps.Air.Drag, pcProps.Ground.Drag, currentState.clampedNormedSurfaceFriction);
            ApplyDrag(frictionMag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var targetSpeed = pcProps.Ground.Speed * RawSpeedMultiplier(readInput) / Mathf.Max(1f, currentState.normedSurfaceFriction);
            targetSpeed = Mathf.Max(targetSpeed, localSpeed);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            if(Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var tvSolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);    // <<< THIS!!!!! no ground snap needed, no extra raycasts. i still get launched slightly but it's negligible
                var tvNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(tvNonSolid, tvSolid, currentState.surfaceSolidness);
            }
            var accelMag = Mathf.Lerp(pcProps.Air.Accel, pcProps.Ground.Accel, currentState.clampedNormedSurfaceFriction);
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * accelMag, Time.fixedDeltaTime);
            if(readInput && (Bind.JUMP.GetKeyDown() || cachedJumpKeyDown)){
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

        Vector3 WaterExitAcceleration (ref MoveState currentState) {
            var verticalLocalVelocity = VerticalComponent(currentState.incomingLocalVelocity);
            if(verticalLocalVelocity.y > 0){
                var jumpVelocity = PlayerTransform.up * JumpSpeed();
                return (jumpVelocity - verticalLocalVelocity) / Time.fixedDeltaTime;
            }
            return Vector3.zero;
        }

        void SlopeMovement (bool readInput, ref MoveState currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var frictionMag = Mathf.Lerp(pcProps.Air.Drag, pcProps.Slope.Drag, currentState.clampedNormedSurfaceFriction);
            ApplyDrag(frictionMag, ref horizontalLocalVelocity);
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = pcProps.Slope.Speed * RawSpeedMultiplier(readInput) / Mathf.Max(1f, currentState.normedSurfaceFriction);
            targetSpeed = Mathf.Max(targetSpeed, horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            if(Vector3.Dot(targetVelocity, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var allowedMoveDirection = Vector3.Cross(currentState.surfacePoint.normal, PlayerTransform.up).normalized;
                targetVelocity = targetVelocity.ProjectOnVector(allowedMoveDirection);
            }
            var accelMag = Mathf.Lerp(pcProps.Air.Accel, pcProps.Slope.Accel, currentState.clampedNormedSurfaceFriction);
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * accelMag, Time.fixedDeltaTime);
            if(Bind.JUMP.GetKey() && currentState.isInWater){
                moveAcceleration += WaterExitAcceleration(ref currentState).ProjectOnPlane(currentState.surfacePoint.normal);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

        void AerialMovement (bool readInput, ref MoveState currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            ApplyDrag(pcProps.Air.Drag, ref horizontalLocalVelocity);
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(pcProps.Air.Speed * RawSpeedMultiplier(readInput), horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.Air.Accel, Time.fixedDeltaTime);
            if(Bind.JUMP.GetKey() && currentState.isInWater && currentState.touchingWall){
                moveAcceleration += WaterExitAcceleration(ref currentState);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

        void WaterMovement (bool readInput, ref MoveState currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(pcProps.Water.Drag, ref localVelocity);
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
            var targetSpeed = Mathf.Max(pcProps.Water.Speed * RawSpeedMultiplier(readInput), localSpeed);
            var targetVelocity = head.TransformDirection(rawInput) * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.Water.Accel, Time.fixedDeltaTime);
            Velocity += moveAcceleration * Time.fixedDeltaTime; // no gravity in water.
        }

        void LadderMovement (bool readInput, ref MoveState currentState) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            if(currentState.isInWater && Vector3.Dot(PlayerTransform.TransformDirection(rawInput), currentState.ladderPoint.normal) > 0){
                AerialMovement(readInput, ref currentState);
                return;
            }
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(pcProps.Ladder.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var targetDirection = LadderMoveVector(rawInput, currentState.ladderPoint.normal);
            var targetSpeed = Mathf.Max(localSpeed, pcProps.Ladder.Speed * RawSpeedMultiplier(readInput));
            var targetVelocity = targetDirection * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.Ladder.Accel, Time.fixedDeltaTime);
            Vector3 gravity;
            if(readInput && (Bind.JUMP.GetKeyDown() || cachedJumpKeyDown)){
                moveAcceleration += currentState.ladderPoint.normal * JumpSpeed() / Time.fixedDeltaTime;
                currentState.jumped = true;
                gravity = Physics.gravity * 0.5f;
            }else{
                var stickGravity = -currentState.ladderPoint.normal * Physics.gravity.magnitude;
                var lerpFactor = Mathf.Clamp01(Vector3.Dot(Physics.gravity.normalized, currentState.ladderPoint.normal));
                gravity = Vector3.Slerp(stickGravity, Physics.gravity, lerpFactor);
            }
            Velocity += (moveAcceleration + gravity) * Time.fixedDeltaTime;
        }

    }

}