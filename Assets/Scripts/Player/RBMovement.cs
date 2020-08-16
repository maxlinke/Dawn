using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class RBMovement : Movement {

        [SerializeField] CapsuleCollider col = default;
        [SerializeField] Rigidbody rb = default;

        public override float Height => col.height;
        public override Vector3 WorldCenterPos => rb.transform.TransformPoint(col.center);
        public override Vector3 WorldFootPos => rb.transform.TransformPoint(col.center + 0.5f * col.height * Vector3.down);
        
        protected override Transform PlayerTransform => rb.transform;
        protected override Vector3 WorldLowerCapsuleSphereCenter => rb.transform.TransformPoint(col.center + (Vector3.down * ((col.height / 2f) - col.radius)));
        protected override float CapsuleRadius => col.radius * rb.transform.localScale.Average();

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

        List<ContactPoint> contactPoints;
        State lastState;

        public override void Initialize (PlayerControllerProperties pcProps) {
            base.Initialize(pcProps);
            contactPoints = new List<ContactPoint>();
            InitCol();
            InitRB();
            initialized = true;

            void InitCol () {
                var pm = new PhysicMaterial();
                pm.bounceCombine = PhysicMaterialCombine.Multiply;
                pm.bounciness = 0;
                pm.frictionCombine = PhysicMaterialCombine.Multiply;
                pm.staticFriction = 0;
                pm.dynamicFriction = 0;
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

        void CacheContacts (Collision collision) {
            if(!initialized){
                return;
            }
            int cc = collision.contactCount;
            for(int i=0; i<cc; i++){
                contactPoints.Add(collision.GetContact(i));
            }
        }

        // move. if moving into normal on ground (local velocity n shit), flatten local velocity and move there)
        // result: if already airborne, move forward instead of "up" again with slope vector. 
        // also needs velocitycomesfrommove. only if velocity comes from move.

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

        // TODO needs velocity comes from move
        void StartMove (out State currentState) {
            var surfacePoint = DetermineSurfacePoint();
            currentState = GetCurrentState(surfacePoint, lastState);
            contactPoints.Clear();
            DEBUGTEXTFIELD.text = string.Empty;
            DEBUGTEXTFIELD.text += $"{currentState.surfaceAngle.ToString()}°\n";
            DEBUGTEXTFIELD.text += $"{currentState.surfaceDot.ToString()}\n";
            DEBUGTEXTFIELD.text += $"{currentState.moveType.ToString()}\n";
            // if(currentState.surfacePoint != null && currentState.surfacePoint.otherCollider != null){
            //     DEBUGTEXTFIELD.text += $"{currentState.surfacePoint.otherCollider.sharedMaterial.staticFriction.ToString()}\n";
            //     DEBUGTEXTFIELD.text += $"{currentState.surfacePoint.otherCollider.sharedMaterial.dynamicFriction.ToString()}\n";
            // }

            SurfacePoint DetermineSurfacePoint () {
                int flattestPoint = -1;
                float maxDot = 0.0175f;     // cos(89°), to exclude walls
                for(int i=0; i<contactPoints.Count; i++){
                    var dot = Vector3.Dot(contactPoints[i].normal, PlayerTransform.up);
                    if(dot > maxDot){
                        flattestPoint = i;
                        maxDot = dot;
                    }
                }
                return (flattestPoint != -1) ? new SurfacePoint(contactPoints[flattestPoint]) : null;
            }
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
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.fixedDeltaTime;
                    break;
            }
        }

        public void ExecuteUpdate () {
            if(!lastState.jumped && lastState.frame < Time.frameCount && lastState.moveType == MoveType.GROUND){
                jumpInputCached |= Bind.JUMP.GetKeyDown();
            }
        }

        void GroundedMovement (bool readInput, ref State currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            var groundFriction = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, pcProps.GroundFriction, Time.fixedDeltaTime);
            groundFriction *= Time.fixedDeltaTime;
            Velocity += groundFriction;
            localVelocity += groundFriction;
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), localSpeed);
            var targetDirection = rb.transform.TransformDirection(rawInput);
            Vector3 targetVelocity = GroundMoveVector(targetDirection, currentState.surfacePoint.normal);
            targetVelocity = targetVelocity.normalized * rawInputMag * targetSpeed;
            if(Vector3.Dot(targetDirection, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var tvSolid = targetVelocity.ProjectOnPlaneAlongVector(PlayerTransform.up, currentState.surfacePoint.normal);    // <<< THIS!!!!! no ground snap needed, no extra raycasts. i still get launched slightly but it's negligible
                var tvNonSolid = targetVelocity * targetDirection.normalized.ProjectOnPlane(currentState.surfacePoint.normal).magnitude;
                targetVelocity = Vector3.Slerp(tvNonSolid, tvSolid, currentState.surfaceSolidness);
            }
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.GroundAccel, Time.fixedDeltaTime);
            if(readInput && (Bind.JUMP.GetKeyDown() || jumpInputCached)){
                moveAccel += PlayerTransform.up * JumpSpeed() / Time.fixedDeltaTime;
                currentState.jumped = true;
            }
            Vector3 gravity;
            if(currentState.jumped){
                gravity = Physics.gravity * 0.5f;
            }else{
                gravity = Vector3.Slerp(Physics.gravity, -currentState.surfacePoint.normal * Physics.gravity.magnitude, currentState.surfaceSolidness);
            }
            Velocity += (moveAccel + gravity) * Time.fixedDeltaTime;
        }

        void SlopeMovement (bool readInput, ref State currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            var slopeFriction = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, pcProps.SlopeFriction, Time.fixedDeltaTime);
            slopeFriction *= Time.fixedDeltaTime;
            Velocity += slopeFriction;
            horizontalLocalVelocity += slopeFriction;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), horizontalLocalSpeed);
            var targetVelocity = rb.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            if(Vector3.Dot(targetVelocity, currentState.surfacePoint.normal) < 0){          // if vector points into ground/slope
                var allowedMoveDirection = Vector3.Cross(currentState.surfacePoint.normal, PlayerTransform.up).normalized;
                targetVelocity = targetVelocity.ProjectOnVector(allowedMoveDirection);
            }
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.SlopeAccel, Time.fixedDeltaTime);
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
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), horizontalLocalSpeed);
            var targetVelocity = rb.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.AirAccel, Time.fixedDeltaTime);
            Velocity += (moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

    }

}