using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class RBMovement : Movement {

        [SerializeField] CapsuleCollider col = default;
        [SerializeField] Rigidbody rb = default;

        protected override Transform PlayerTransform => rb.transform;
        protected override Transform GravityAlignmentReferenceTransform => smoothRotationParent;

        public override float LocalColliderHeight {
            get => col.height;
            protected set => col.height = value;
        }

        public override float LocalColliderRadius {
            get => col.radius;
            protected set => col.radius = value;
        }

        public override Vector3 LocalColliderCenter {
            get => col.center;
            protected set => col.center = value;
        }

        protected Vector3 TargetSmoothRotationParentPos => col.center;
        protected Vector3 TargetHeadPos => new Vector3(0f, 0.5f * col.height + props.EyeOffset, 0f);
        protected Vector3 TargetModelPos => new Vector3(0f, -0.5f * col.height, 0f);
        
        public override Vector3 Velocity { 
            get => rb.velocity;
            set => rb.velocity = value;
        }

        public override ControlMode controlMode {
            get => m_controlMode;
            set { 
                if(value == ControlMode.ANCHORED){
                    rb.isKinematic = true;
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }else{
                    rb.isKinematic = false;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    rb.collisionDetectionMode = rbProps.CollisionDetection;
                }
                m_controlMode = value; 
            }
        }

        bool initialized = false;
        ControlMode m_controlMode = ControlMode.FULL;

        RBProperties rbProps;
        Transform smoothRotationParent;

        List<CollisionPoint> contactPoints;
        List<Collider> triggerStays;

        public void Initialize (RBProperties rbProps, Transform head, Transform model, Transform smoothRotationParent) {
            base.Init(rbProps, head, model);
            this.rbProps = rbProps;
            this.smoothRotationParent = smoothRotationParent;
            contactPoints = new List<CollisionPoint>();
            triggerStays = new List<Collider>();
            InitCol();
            InitRB();
            initialized = true;

            void InitCol () {
                PhysicMaterial pm = rbProps.PhysicMaterial;
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
                rb.mass = rbProps.PlayerMass;
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
            UpdateColliderSizeIfNeeded(currentState, instantly: false);
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
                    WaterMovement(moveInput, ref currentState);
                    break;
                case MoveType.LADDER:
                    LadderMovement(moveInput, ref currentState);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.deltaTime;
                    break;
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
                var maxDelta = props.HeightChangeSpeed * Time.deltaTime;
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

        protected override bool ColliderIsSolid (Collider otherCollider) {
            if(otherCollider == null) return false;
            var otherRB = otherCollider.attachedRigidbody;
            if(otherRB == null) return true;
            return otherRB.isKinematic;
        }

        protected override void GetVelocityAndSolidness (CollisionPoint surfacePoint, out Vector3 velocity, out float solidness) {
            var otherRB = surfacePoint.otherRB;
            velocity = (otherRB == null ? Vector3.zero : otherRB.velocity);
            if(ColliderIsSolid(surfacePoint.otherCollider)){
                solidness = 1f;    
            }else if(otherRB != null){
                solidness = Mathf.Clamp01((otherRB.mass - rbProps.FootRBNonSolidMass) / (rbProps.FootRBSolidMass - rbProps.FootRBNonSolidMass));
            }else{
                solidness = 0f;
            }
        }

        protected override void OnColliderSizeUpdated (bool onGround) {
            var srpDelta = TargetSmoothRotationParentPos - smoothRotationParent.localPosition;
            smoothRotationParent.localPosition += srpDelta;
            if(onGround){
                head.localPosition -= srpDelta;
                model.localPosition -= srpDelta;
            }else{
                head.localPosition += srpDelta;
                model.localPosition += srpDelta;
            }
        }
        
        protected override void ApplyGravityRotation (Quaternion newRotation) {
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
            var localVelocity = currentState.incomingLocalVelocity.ProjectOnPlane(currentState.surfacePoint.normal);
            if(lastState.midJump){  // can i move this into the state thingy?
                var landingAccel = GetLandingBrake(localVelocity, moveInput.jump);
                localVelocity += landingAccel;
                Velocity += landingAccel;
            }
            var dragFriction = currentState.normedStaticSurfaceFriction;
            var moveFriction = currentState.normedDynamicSurfaceFriction;
            var frictionMag = Mathf.Lerp(props.Air.Drag, props.Ground.Drag, dragFriction);
            // if(dragFriction > 1){                // TODO revisit this when i do moving platforms (lateral ones) and they slide away from underneath me
            //     frictionMag *= dragFriction;
            // }
            ApplyDrag(frictionMag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var rawTargetSpeed = props.Ground.Speed * RawSpeedMultiplier(moveInput.run) / Mathf.Max(1f, moveFriction);
            var targetSpeed = Mathf.Max(rawTargetSpeed, localSpeed);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            if(Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var tvSolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);    // <<< THIS!!!!! no ground snap needed, no extra raycasts. i still get launched slightly but it's negligible
                var tvNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(tvNonSolid, tvSolid, currentState.surfaceSolidness); // TODO also make the "regular" slope movement slower, is slerp really needed?
            }
            var accelMag = Mathf.Lerp(props.Air.Accel, props.Ground.Accel, moveFriction);
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * accelMag);
            if(moveInput.jump){
                var jumpStrength = Mathf.Lerp(1f - (currentState.surfaceAngle / 90f), 1f, moveFriction);
                moveAccel += GetJumpVelocity(localVelocity, jumpStrength) / Time.deltaTime;
                moveAccel += GetJumpSpeedBoost(localVelocity, localSpeed, rawTargetSpeed) / Time.deltaTime;
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

                if(props.StickProactively){
                    if((localSpeed - 0.01f) <= (props.Ground.Speed * props.RunSpeedMultiplier)){        // TODO gravity strength, velocitycomesfrommove
                        if(TryEnforceGroundStick(ref currentState, targetSpeed, localVelocity)){
                            moveAccel = Vector3.zero;
                            // gravity = Vector3.zero;
                        }
                    }
                }
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
            var frictionMag = Mathf.Lerp(props.Air.Drag, props.Slope.Drag, dragFriction);
            ApplyDrag(frictionMag, ref horizontalLocalVelocity);
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = moveInput.horizontalInput;
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = props.Slope.Speed * RawSpeedMultiplier(moveInput.run) / Mathf.Max(1f, moveFriction);
            targetSpeed = Mathf.Max(targetSpeed, horizontalLocalSpeed);
            var targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var accelMag = Mathf.Lerp(props.Air.Accel, props.Slope.Accel, moveFriction);
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * accelMag);
            if(currentState.isInWater && moveInput.waterExitJump){
                var facingAngle = Vector3.Angle(-PlayerTransform.forward, currentState.surfacePoint.normal.ProjectOnPlane(PlayerTransform.up));
                if(facingAngle <= 45f){
                    moveAcceleration += WaterExitAcceleration(ref currentState).ProjectOnPlane(currentState.surfacePoint.normal);
                }
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
            float drag = props.Air.Drag;
            if(props.EnableFullFlightParabola && horizontalLocalSpeed > props.Air.Speed){
                var dirDot = Vector3.Dot(targetDirection, horizontalLocalVelocity.normalized);
                drag *= (1f - Mathf.Clamp01(dirDot));
            }
            ApplyDrag(drag, ref horizontalLocalVelocity);
            horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawTargetSpeed = props.Air.Speed * RawSpeedMultiplier(moveInput.run);
            var targetSpeed = Mathf.Max(rawTargetSpeed, horizontalLocalSpeed);
            var targetVelocity = targetDirection * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * props.Air.Accel);
            if(currentState.isInWater && currentState.facingWall && moveInput.waterExitJump){
                moveAcceleration += WaterExitAcceleration(ref currentState);
            }
            Velocity += (moveAcceleration + Physics.gravity) * Time.deltaTime;
        }

        void WaterMovement (MoveInput moveInput, ref MoveState currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(props.Water.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var rawInput = moveInput.horizontalInput;
            rawInput += head.InverseTransformDirection(moveInput.verticalInput);
            if(rawInput.sqrMagnitude > 1f){
                rawInput = rawInput.normalized;
            }
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(props.Water.Speed * RawSpeedMultiplier(moveInput.run), localSpeed);
            var targetVelocity = head.TransformDirection(rawInput) * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * props.Water.Accel);
            Velocity += (moveAcceleration + Physics.gravity) * Time.deltaTime;
            if(currentState.waterBody != null){
                var buoyancy = currentState.waterBody.WaterPhysics.BuoyancyFromDensity(props.PlayerDensity);
                currentState.waterBody.AddBuoyancy(rb, buoyancy, Vector3.zero);     // TODO water buoyancy should always happen, not just in here
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
            ApplyDrag(props.Ladder.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var targetDirection = LadderMoveVector(rawInput, currentState.ladderPoint.normal);
            var targetSpeed = Mathf.Max(localSpeed, props.Ladder.Speed * RawSpeedMultiplier(moveInput.run));
            var targetVelocity = targetDirection * targetSpeed;
            var moveAcceleration = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * props.Ladder.Accel);
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