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
                    ExecuteMove(readInput, currentState);
                    break;
                case ControlMode.BLOCK_INPUT:
                    ExecuteMove(false, currentState);
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
            var sp = DetermineSurfacePoint();
            currentState.surfacePoint = sp;
            if(sp == null){
                currentState.moveType = MoveType.AIR;
                currentState.localVelocity = this.Velocity; // TODO potentially check for a trigger (such as in a moving train car...)
            }else{
                currentState.moveType = MoveType.GROUND;
                currentState.localVelocity = this.Velocity - currentState.surfacePoint.otherVelocity;
            }
            currentState.jumped = false;
            contactPoints.Clear();

            // might need an alternative here...
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

        void FinishMove (State currentState) {
            lastState = currentState;
        }
        
        void ExecuteMove (bool readInput, State currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    AerialMovement(readInput, currentState);
                    break;
                case MoveType.GROUND:
                    GroundedMovement(readInput, currentState);
                    break;
                default:
                    break;
            }
        }

        void GroundedMovement (bool readInput, State currentState) {
            var localVelocity = currentState.localVelocity;
            var groundFriction = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, pcProps.GroundDrag, Time.fixedDeltaTime);
            localVelocity += groundFriction * Time.fixedDeltaTime;
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), localSpeed);
            var targetDirection = GroundMoveVector(rb.transform.TransformDirection(rawInput), currentState.surfacePoint.normal);
            var targetVelocity = targetDirection.normalized * rawInputMag * targetSpeed;
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.GroundAccel, Time.deltaTime);
            if(readInput && Bind.JUMP.GetKey()){    // TODO cache jump wish and such...
                moveAccel = new Vector3(moveAccel.x, JumpSpeed() / Time.fixedDeltaTime, moveAccel.z);
            }
            Velocity += (groundFriction + moveAccel + Physics.gravity) * Time.fixedDeltaTime;
        }

        void AerialMovement (bool readInput, State currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.localVelocity);
            // var decelFactor = (hVelocityMag > pcProps.MoveSpeed) ? 1 : (1f - rawInputMag);
            var dragDeceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, pcProps.AirDrag, Time.fixedDeltaTime);
            horizontalLocalVelocity += dragDeceleration * Time.fixedDeltaTime;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), horizontalLocalSpeed);
            var targetVelocity = rb.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.AirAccel, Time.deltaTime);
            Velocity += (dragDeceleration + moveAcceleration + Physics.gravity) * Time.fixedDeltaTime;
        }

    }

}