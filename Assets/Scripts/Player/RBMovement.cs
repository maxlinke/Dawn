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

        public override Vector3 WorldVelocity => rb.velocity;
        public override Vector3 LocalVelocity => rb.velocity;       // TODO either last state or actual calculation? (this is cached value, provide CalculateLocalVelocity)
        
        protected override Vector3 MoveVelocity { 
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
        Vector3 lastPos;

        public void Initialize (RBProperties rbProps, Transform head, Transform model, Transform smoothRotationParent) {
            base.Init(rbProps, head, model);
            this.rbProps = rbProps;
            this.smoothRotationParent = smoothRotationParent;
            contactPoints = new List<CollisionPoint>();
            triggerStays = new List<Collider>();
            lastPos = transform.position;
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
                    MoveVelocity = Vector3.zero;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(ControlMode)} \"{controlMode}\"!");
                    MoveVelocity = Vector3.zero;
                    break;
            }
            FinishMove(ref currentState);
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
            debugInfo += $"ilv: {currentState.localVelocity.magnitude:F3} m/s\n";
            debugInfo += $"c:   {shouldCrouch}\n";
            debugInfo += $"h:   {col.height}\n";
        }

        void FinishMove (ref MoveState currentState) {Color lineColor;
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
            var currentPos = transform.position;
            Debug.DrawLine(lastPos, currentPos, lineColor, 10f);
            lastPos = currentPos;
            
            currentState.worldVelocity = WorldVelocity;
            currentState.localVelocity = LocalVelocity;
            lastState = currentState;
        }
        
        void ExecuteMove (MoveInput moveInput, ref MoveState currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    MoveVelocity += AerialMovement(moveInput, ref currentState) * Time.deltaTime;
                    break;
                case MoveType.GROUND:
                    MoveVelocity += GroundedMovement(moveInput, ref currentState) * Time.deltaTime;
                    break;
                case MoveType.SLOPE:
                    MoveVelocity += SlopeMovement(moveInput, ref currentState) * Time.deltaTime;
                    break;
                case MoveType.WATER:
                    MoveVelocity += WaterMovement(moveInput, ref currentState) * Time.deltaTime;
                    break;
                case MoveType.LADDER:
                    MoveVelocity += LadderMovement(moveInput, ref currentState) * Time.deltaTime;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    MoveVelocity += Physics.gravity * Time.deltaTime;
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

        Vector3 GroundedMovement (MoveInput moveInput, ref MoveState currentState) {
            Vector3 rawInput = moveInput.horizontalInput;
            float rawInputMag = rawInput.magnitude;
            Vector3 targetDirection = PlayerTransform.TransformDirection(rawInput);
            if(currentState.ladderPoint != null && currentState.ladderPoint != currentState.surfacePoint){
                if(Vector3.Dot(targetDirection.normalized, currentState.ladderPoint.normal) < 0f){
                    return LadderMovement(moveInput, ref currentState);
                }
            }
            Vector3 localVelocity = currentState.localVelocity.ProjectOnPlane(currentState.surfacePoint.normal);
            Vector3 dragAccel = Vector3.zero;
            if(lastState.midJump){
                var brakeDeltaV = GetLandingBrake(localVelocity, moveInput.jump);
                localVelocity += brakeDeltaV;
                dragAccel += brakeDeltaV / Time.deltaTime;
            }
            float dragFriction = currentState.normedDynamicSurfaceFriction;
            float moveFriction = currentState.normedStaticSurfaceFriction;
            float drag = Mathf.Lerp(props.Air.Drag, props.Ground.Drag, dragFriction);
            dragAccel += DragAccel(drag, ref localVelocity, out float localSpeed);
            float rawTargetSpeed = props.Ground.Speed * RawSpeedMultiplier(moveInput.run);
            float targetSpeed = Mathf.Max(rawTargetSpeed, localSpeed);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            bool walkingIntoSlope = (Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < -0.01f);
            if(walkingIntoSlope){
                Vector3 targetVelocitySolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);
                Vector3 targetVelocityNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(targetVelocityNonSolid, targetVelocitySolid, currentState.surfaceSolidness);
            }
            float maxAccel = rawInputMag * Mathf.Lerp(props.Air.Accel, props.Ground.Accel, moveFriction);
            Vector3 moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, maxAccel);
            Vector3 gravity = Physics.gravity;
            if(moveInput.jump){
                float jumpStrength = Mathf.Lerp(1f - (currentState.surfaceAngle / 90f), 1f, moveFriction);
                moveAccel += GetJumpVelocity(localVelocity, jumpStrength) / Time.deltaTime;
                moveAccel += GetJumpSpeedBoost(localVelocity, localSpeed, rawTargetSpeed) / Time.deltaTime;
                currentState.startedJump = true;
                gravity *= 0.5f;
            }else{
                Vector3 stickGravity = -currentState.surfacePoint.normal * gravity.magnitude;
                float lerpFactor = currentState.surfaceSolidness * dragFriction;
                lerpFactor *= Mathf.Clamp01(-1f * Vector3.Dot(gravity.normalized, PlayerTransform.up));
                gravity = Vector3.Slerp(gravity, stickGravity, lerpFactor);
            }
            return dragAccel + moveAccel + gravity;
        }

        Vector3 WaterExitAcceleration (Vector3 localVelocity) {
            var verticalLocalVelocity = VerticalComponent(localVelocity);
            if(Vector3.Dot(verticalLocalVelocity, PlayerTransform.up) > 0){
                var jumpVelocity = PlayerTransform.up * JumpSpeed();
                return (jumpVelocity - verticalLocalVelocity) / Time.deltaTime;
            }
            return Vector3.zero;
        }

        Vector3 SlopeMovement (MoveInput moveInput, ref MoveState currentState) {
            Vector3 horizontalLocalVelocity = HorizontalComponent(currentState.localVelocity);
            float dragFriction = currentState.normedDynamicSurfaceFriction;
            float moveFriction = currentState.normedStaticSurfaceFriction;
            float drag = Mathf.Lerp(props.Air.Drag, props.Slope.Drag, dragFriction);
            Vector3 dragAccel = DragAccel(drag, ref horizontalLocalVelocity, out float horizontalLocalSpeed);
            Vector3 rawInput = moveInput.horizontalInput;
            float rawTargetSpeed = props.Slope.Speed * RawSpeedMultiplier(moveInput.run);
            float targetSpeed = Mathf.Max(rawTargetSpeed, horizontalLocalSpeed);
            Vector3 targetVelocity = PlayerTransform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            float maxAccel = rawInput.magnitude * Mathf.Lerp(props.Air.Accel, props.Slope.Accel, moveFriction);
            Vector3 moveAccel = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, maxAccel);
            if(currentState.isInWater && moveInput.waterExitJump){
                float facingAngle = Vector3.Angle(-PlayerTransform.forward, currentState.surfacePoint.normal.ProjectOnPlane(PlayerTransform.up));
                if(facingAngle <= 45f){
                    moveAccel += WaterExitAcceleration(currentState.localVelocity).ProjectOnPlane(currentState.surfacePoint.normal);
                }
            }else if(Vector3.Dot(moveAccel, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                Vector3 allowedMoveDirection = Vector3.Cross(currentState.surfacePoint.normal, PlayerTransform.up).normalized;
                moveAccel = moveAccel.ProjectOnVector(allowedMoveDirection);
            }
            return dragAccel + moveAccel + Physics.gravity;
        }

        Vector3 AerialMovement (MoveInput moveInput, ref MoveState currentState) {
            Vector3 rawInput = moveInput.horizontalInput;
            Vector3 targetDirection = PlayerTransform.TransformDirection(rawInput);
            Vector3 horizontalLocalVelocity = HorizontalComponent(currentState.localVelocity);
            float horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            float drag = props.Air.Drag;
            if(props.EnableFullFlightParabola && horizontalLocalSpeed > props.Air.Speed){
                float dirDot = Vector3.Dot(targetDirection, horizontalLocalVelocity.normalized);
                drag *= (1f - Mathf.Clamp01(dirDot));
            }
            Vector3 dragAccel = DragAccel(drag, ref horizontalLocalVelocity, out horizontalLocalSpeed);
            float rawTargetSpeed = props.Air.Speed * RawSpeedMultiplier(moveInput.run);
            float targetSpeed = Mathf.Max(rawTargetSpeed, horizontalLocalSpeed);
            Vector3 targetVelocity = targetDirection * targetSpeed;   // raw input magnitude is contained in raw input vector
            float maxAccel = rawInput.magnitude * props.Air.Accel;
            Vector3 moveAccel = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, maxAccel);
            if(currentState.isInWater && currentState.facingWall && moveInput.waterExitJump){
                moveAccel += WaterExitAcceleration(currentState.localVelocity);
            }
            return dragAccel + moveAccel + Physics.gravity;
        }

        Vector3 WaterMovement (MoveInput moveInput, ref MoveState currentState) {
            Vector3 localVelocity = currentState.localVelocity;
            Vector3 dragAccel = DragAccel(props.Water.Drag, ref localVelocity, out float localSpeed);
            float targetSpeed = Mathf.Max(props.Water.Speed * RawSpeedMultiplier(moveInput.run), localSpeed);
            Vector3 rawInput = WaterMoveVector(moveInput);
            Vector3 targetVelocity = head.TransformDirection(rawInput) * targetSpeed;
            float maxAccel = rawInput.magnitude * props.Water.Accel;
            Vector3 moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, maxAccel);
            if(currentState.waterBody != null){
                var buoyancy = currentState.waterBody.WaterPhysics.BuoyancyFromDensity(props.PlayerDensity);
                dragAccel += currentState.waterBody.GetBuoyancyAcceleration(rb, buoyancy);
            }
            return dragAccel + moveAccel + Physics.gravity;
        }

        Vector3 LadderMovement (MoveInput moveInput, ref MoveState currentState) {
            Vector3 rawInput = moveInput.horizontalInput;
            if(currentState.isInWater && Vector3.Dot(PlayerTransform.TransformDirection(rawInput), currentState.ladderPoint.normal) > 0){
                return AerialMovement(moveInput, ref currentState);
            }
            Vector3 localVelocity = currentState.localVelocity;
            Vector3 dragAccel = DragAccel(props.Ladder.Drag, ref localVelocity, out float localSpeed);
            float targetSpeed = Mathf.Max(localSpeed, props.Ladder.Speed * RawSpeedMultiplier(moveInput.run));
            Vector3 targetDirection = LadderMoveVector(rawInput, currentState.ladderPoint.normal);
            Vector3 targetVelocity = targetDirection * targetSpeed;
            float maxAccel = rawInput.magnitude * props.Ladder.Accel;
            Vector3 moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, maxAccel);
            Vector3 gravity;
            if(moveInput.jump){
                moveAccel += currentState.ladderPoint.normal * JumpSpeed() / Time.deltaTime;
                currentState.startedJump = true;
                gravity = Physics.gravity * 0.5f;
            }else{
                Vector3 stickGravity = -currentState.ladderPoint.normal * Physics.gravity.magnitude;
                float lerpFactor = Mathf.Clamp01(Vector3.Dot(Physics.gravity.normalized, currentState.ladderPoint.normal));
                gravity = Vector3.Slerp(stickGravity, Physics.gravity, lerpFactor);
            }
            return dragAccel + moveAccel + gravity;
        }

    }

}