using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class RBMovement : Movement {

        public struct MoveInput {
            public Vector3 horizontalInput;
            public Vector3 verticalInput;
            public float run;
            public bool jump;
            public bool waterExitJump;

            public static MoveInput None { get {
                MoveInput output;
                output.horizontalInput = Vector3.zero;
                output.verticalInput = Vector3.zero;
                output.run = 0f;
                output.jump = false;
                output.waterExitJump = false;
                return output;
            } }
        }

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
        Transform smoothRotationParent;
        ControlMode m_controlMode = ControlMode.FULL;

        List<CollisionPoint> contactPoints;
        List<Collider> triggerStays;

        public void Initialize (Properties pcProps, Transform head, Transform model, Transform smoothRotationParent) {
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

        public void Move (MoveInput moveInput) {
            if(!initialized){
                Debug.LogWarning($"{nameof(RBMovement)} isn't initialized yet!");
                return;
            }
            StartMove(out var currentState);
            UpdateColliderSizeIfNeeded(currentState);
            switch(controlMode){
                case ControlMode.FULL:
                    ExecuteMove(moveInput, ref currentState);
                    break;
                case ControlMode.BLOCK_INPUT:
                    ExecuteMove(MoveInput.None, ref currentState);
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
            debugInfo = string.Empty;
            debugInfo += $"sp:  {(currentState.surfacePoint == null ? "null" : currentState.surfacePoint.GetName())}\n";
            debugInfo += $"sa:  {currentState.surfaceAngle.ToString()}°\n";
            debugInfo += $"sd:  {currentState.surfaceDot.ToString()}\n";
            debugInfo += $"mt:  {currentState.moveType.ToString()}\n";
            debugInfo += $"nsf: {currentState.normedStaticSurfaceFriction.ToString()}\n";
            debugInfo += $"ndf: {currentState.normedDynamicSurfaceFriction.ToString()}\n";
            debugInfo += $"ilv: {currentState.incomingLocalVelocity.magnitude:F3} m/s\n";
            debugInfo += $"c:   {shouldCrouch}\n";
            debugInfo += $"h:   {col.height}\n";
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
        }
        
        void ExecuteMove (MoveInput moveInput, ref MoveState currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    AerialMovement(moveInput, ref currentState);
                    break;
                case MoveType.GROUND:
                    GroundedMovement(moveInput, ref currentState);
                    break;
                case MoveType.SLOPE:
                    SlopeMovement(moveInput, ref currentState);
                    break;
                case MoveType.WATER:
                    SetTryCrouch(false);
                    WaterMovement(moveInput, ref currentState);
                    break;
                case MoveType.LADDER:
                    SetTryCrouch(false);
                    LadderMovement(moveInput, ref currentState);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.deltaTime;
                    break;
            }
        }

        public void UpdateColliderSizeIfNeeded (MoveState currentState) {
            bool noHeightUpdateNeeded = false;
            noHeightUpdateNeeded |= (shouldCrouch && col.height == pcProps.CrouchHeight);
            noHeightUpdateNeeded |= (!shouldCrouch && col.height == pcProps.NormalHeight);
            if(noHeightUpdateNeeded){
                return;
            }
            float targetHeight;
            if(shouldCrouch || !CanUncrouch(onGround: currentState.surfacePoint != null)){
                targetHeight = pcProps.CrouchHeight;
            }else{
                targetHeight = pcProps.NormalHeight;
            }
            float deltaHeight = targetHeight - col.height;
            float maxDelta = pcProps.HeightChangeSpeed * Time.deltaTime;
            if(Mathf.Abs(deltaHeight) > maxDelta){
                deltaHeight = Mathf.Sign(deltaHeight) * maxDelta;
            }
            col.height += deltaHeight;
            col.center = new Vector3(0f, col.height / 2f, 0f);
            var srpDelta = TargetSmoothRotationParentPos - smoothRotationParent.localPosition;
            smoothRotationParent.localPosition += srpDelta;
            if(currentState.surfacePoint == null || lastState.startedJump){
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
                var maxDeltaSqr = maxDelta * maxDelta;
                var deltaHead = TargetHeadPos - head.localPosition;
                var deltaModel = TargetModelPos - model.localPosition;
                if(deltaHead.sqrMagnitude > maxDeltaSqr){
                    deltaHead = deltaHead.normalized * maxDelta;
                }
                if(deltaModel.sqrMagnitude > maxDeltaSqr){
                    deltaModel = deltaModel.normalized * maxDelta;
                }
                head.localPosition += deltaHead;
                model.localPosition += deltaModel;
            }
        }

        public void AlignWithGravityIfAllowed () {
            if(controlMode == ControlMode.ANCHORED){
                return;
            }
            var gravityRotation = GetGravityRotation(smoothRotationParent);
            var normedGravityStrength = Mathf.Clamp01(Physics.gravity.magnitude / pcProps.JumpCalcGravity);
            var degreesPerSecond = pcProps.GravityTurnSpeed * Mathf.Max(normedGravityStrength, pcProps.MinGravityTurnSpeedMultiplier);
            var newRotation = Quaternion.RotateTowards(smoothRotationParent.rotation, gravityRotation, Time.deltaTime * degreesPerSecond);
            smoothRotationParent.rotation = newRotation;
        }

        public void ApplySubRotation () {
            var wcPos = WorldCenterPos;
            PlayerTransform.rotation = smoothRotationParent.rotation;
            smoothRotationParent.localRotation = Quaternion.identity;
            WorldCenterPos = wcPos;
        }

        void GroundedMovement (MoveInput moveInput, ref MoveState currentState) {
            var rawInput = moveInput.horizontalInput;
            var rawInputMag = rawInput.magnitude;
            var targetDirection = PlayerTransform.TransformDirection(rawInput);
            if(currentState.ladderPoint != null && currentState.ladderPoint != currentState.surfacePoint){
                if(Vector3.Dot(targetDirection.normalized, currentState.ladderPoint.normal) < 0f){
                    LadderMovement(moveInput, ref currentState);
                    return;
                }
            }
            var localVelocity = currentState.incomingLocalVelocity;
            if(pcProps.JumpSpeedBoost != Properties.JumpSpeedBoostMode.Off){
                if(!lastState.touchingGround && (lastState.startedJump || lastState.midJump)){
                    if(!(pcProps.EnableBunnyHopping && moveInput.jump)){
                        var mode = Properties.JumpSpeedBoostMode.OmniDirectional;
                        var mult = 1f / pcProps.JumpSpeedBoostMultiplier;
                        var decel = GetJumpSpeedBoost(localVelocity, mode, mult);
                        localVelocity += decel;
                        Velocity += decel;
                        Debug.DrawRay(PlayerTransform.position, PlayerTransform.up * 2f, Color.red, 10f);
                    }else{
                        Debug.DrawRay(PlayerTransform.position, PlayerTransform.up * 2f, Color.green, 10f);
                    }
                }
            }
            var dragFriction = currentState.normedStaticSurfaceFriction;
            var moveFriction = currentState.normedDynamicSurfaceFriction;
            var frictionMag = Mathf.Lerp(pcProps.Air.Drag, pcProps.Ground.Drag, dragFriction);
            // if(dragFriction > 1){                // TODO revisit this when i do moving platforms (lateral ones) and they slide away from underneath me
            //     frictionMag *= dragFriction;
            // }
            ApplyDrag(frictionMag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var rawTargetSpeed = pcProps.Ground.Speed * RawSpeedMultiplier(moveInput.run) / Mathf.Max(1f, moveFriction);
            var targetSpeed = Mathf.Max(rawTargetSpeed, localSpeed);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            if(Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var tvSolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);    // <<< THIS!!!!! no ground snap needed, no extra raycasts. i still get launched slightly but it's negligible
                var tvNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(tvNonSolid, tvSolid, currentState.surfaceSolidness);
            }
            var accelMag = Mathf.Lerp(pcProps.Air.Accel, pcProps.Ground.Accel, moveFriction);
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * accelMag);
            if(moveInput.jump){
                var jumpStrength = Mathf.Lerp(1f - (currentState.surfaceAngle / 90f), 1f, moveFriction);
                moveAccel += PlayerTransform.up * JumpSpeed() * jumpStrength / Time.deltaTime;
                if(pcProps.EnableOverBoosting || localSpeed <= rawTargetSpeed){
                    moveAccel += GetJumpSpeedBoost(localVelocity, pcProps.JumpSpeedBoost, pcProps.JumpSpeedBoostMultiplier) / Time.deltaTime;
                }
                currentState.startedJump = true;
            }
            Vector3 gravity;
            if(currentState.startedJump){
                gravity = Physics.gravity * 0.5f;
            }else{
                var stickGravity = -currentState.surfacePoint.normal * Physics.gravity.magnitude;
                var lerpFactor = currentState.surfaceSolidness * dragFriction;
                lerpFactor *= Mathf.Clamp01(-1f * Vector3.Dot(Physics.gravity.normalized, PlayerTransform.up));
                gravity = Vector3.Slerp(Physics.gravity, stickGravity, lerpFactor);
            }
            Velocity += (moveAccel + gravity) * Time.deltaTime;
        }

        Vector3 WaterExitAcceleration (ref MoveState currentState) {
            var verticalLocalVelocity = VerticalComponent(currentState.incomingLocalVelocity);
            if(verticalLocalVelocity.y > 0){
                var jumpVelocity = PlayerTransform.up * JumpSpeed();
                return (jumpVelocity - verticalLocalVelocity) / Time.deltaTime;
            }
            return Vector3.zero;
        }

        void SlopeMovement (MoveInput moveInput, ref MoveState currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var dragFriction = currentState.normedStaticSurfaceFriction;
            var moveFriction = currentState.normedDynamicSurfaceFriction;
            var frictionMag = Mathf.Lerp(pcProps.Air.Drag, pcProps.Slope.Drag, dragFriction);
            ApplyDrag(frictionMag, ref horizontalLocalVelocity);
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = moveInput.horizontalInput;
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = pcProps.Slope.Speed * RawSpeedMultiplier(moveInput.run) / Mathf.Max(1f, moveFriction);
            targetSpeed = Mathf.Max(targetSpeed, horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var accelMag = Mathf.Lerp(pcProps.Air.Accel, pcProps.Slope.Accel, moveFriction);
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * accelMag);
            if(currentState.isInWater && moveInput.waterExitJump){
                moveAcceleration += WaterExitAcceleration(ref currentState).ProjectOnPlane(currentState.surfacePoint.normal);
            }else if(Vector3.Dot(moveAcceleration, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var allowedMoveDirection = Vector3.Cross(currentState.surfacePoint.normal, PlayerTransform.up).normalized;
                moveAcceleration = moveAcceleration.ProjectOnVector(allowedMoveDirection);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.deltaTime;
        }

        void AerialMovement (MoveInput moveInput, ref MoveState currentState) {
            var rawInput = moveInput.horizontalInput;
            var rawInputMag = rawInput.magnitude;
            var targetDirection = PlayerTransform.TransformDirection(rawInput);
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var drag = pcProps.Air.Drag;
            if(pcProps.EnableFullFlightParabola && horizontalLocalSpeed > pcProps.Air.Speed){
                var dirDot = Vector3.Dot(targetDirection, horizontalLocalVelocity.normalized);
                drag *= (1f - Mathf.Clamp01(dirDot));
            }
            ApplyDrag(drag, ref horizontalLocalVelocity);
            horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var targetSpeed = Mathf.Max(pcProps.Air.Speed * RawSpeedMultiplier(moveInput.run), horizontalLocalSpeed);
            var targetVelocity = targetDirection * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.Air.Accel);
            if(currentState.isInWater && currentState.touchingWall && moveInput.waterExitJump){
                moveAcceleration += WaterExitAcceleration(ref currentState);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.deltaTime;
        }

        void WaterMovement (MoveInput moveInput, ref MoveState currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(pcProps.Water.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var rawInput = moveInput.horizontalInput;
            rawInput += head.InverseTransformDirection(moveInput.verticalInput);
            if(rawInput.sqrMagnitude > 1f){
                rawInput = rawInput.normalized;
            }
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(pcProps.Water.Speed * RawSpeedMultiplier(moveInput.run), localSpeed);
            var targetVelocity = head.TransformDirection(rawInput) * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.Water.Accel);
            Velocity += (moveAcceleration + Physics.gravity) * Time.deltaTime;
            if(currentState.waterBody != null){
                var buoyancy = currentState.waterBody.WaterPhysics.BuoyancyFromDensity(pcProps.PlayerDensity);
                currentState.waterBody.AddBuoyancy(rb, buoyancy, Vector3.zero);
            }
        }

        void LadderMovement (MoveInput moveInput, ref MoveState currentState) {
            var rawInput = moveInput.horizontalInput;
            var rawInputMag = rawInput.magnitude;
            if(currentState.isInWater && Vector3.Dot(PlayerTransform.TransformDirection(rawInput), currentState.ladderPoint.normal) > 0){
                AerialMovement(moveInput, ref currentState);
                return;
            }
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(pcProps.Ladder.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var targetDirection = LadderMoveVector(rawInput, currentState.ladderPoint.normal);
            var targetSpeed = Mathf.Max(localSpeed, pcProps.Ladder.Speed * RawSpeedMultiplier(moveInput.run));
            var targetVelocity = targetDirection * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.Ladder.Accel);
            Vector3 gravity;
            if(moveInput.jump){
                moveAcceleration += currentState.ladderPoint.normal * JumpSpeed() / Time.deltaTime;
                currentState.startedJump = true;
                gravity = Physics.gravity * 0.5f;
            }else{
                var stickGravity = -currentState.ladderPoint.normal * Physics.gravity.magnitude;
                var lerpFactor = Mathf.Clamp01(Vector3.Dot(Physics.gravity.normalized, currentState.ladderPoint.normal));
                gravity = Vector3.Slerp(stickGravity, Physics.gravity, lerpFactor);
            }
            Velocity += (moveAcceleration + gravity) * Time.deltaTime;
        }

    }

}