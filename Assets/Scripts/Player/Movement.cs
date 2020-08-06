using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class Movement : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            ANCHORED
        }

        PlayerControllerProperties pcProps;
        Player player;
        Rigidbody rb;
        CapsuleCollider col;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public float Height => col.height;
        public Vector3 Velocity { get; }
        public Vector3 WorldCenter => rb.transform.TransformPoint(col.center);

        List<ContactPoint> contactPoints;

        public void Initialize (PlayerControllerProperties pcProps, Player player, Rigidbody rb, CapsuleCollider col) {
            this.pcProps = pcProps;
            this.player = player;
            this.rb = rb;       // see if i notice a difference between the different collision detection modes. i shouldn't get THIS fast... but the capsule also isn't the biggest object... at 40 m/s i move the entire diameter of the capsule
            this.col = col;
            contactPoints = new List<ContactPoint>();
            m_initialized = true;
        }

        Vector3 GetLocalSpaceMoveInput () {
            float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
            float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
            var output = new Vector3(strafe, 0, move);
            if(output.sqrMagnitude > 1){
                return output.normalized;
            }
            return output;
        }

        Vector3 HorizontalComponent (Vector3 vector) {
            return Vector3.ProjectOnPlane(vector, rb.transform.up);
        }

        Vector3 VerticalComponent (Vector3 vector) {
            return Vector3.Project(vector, rb.transform.up);
        }

        Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / Time.fixedDeltaTime;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }

        Vector3 GroundMoveVector (Vector3 worldMoveInput, Vector3 groundNormal) {
            return ProjectOnPlaneAlongVector(worldMoveInput, groundNormal, rb.transform.up);
        }

        Vector3 ProjectOnPlaneAlongVector (Vector3 vector, Vector3 normal, Vector3 projectVector) {
            float x = Vector3.Dot(normal, vector) / Vector3.Dot(normal, projectVector);
            return (vector - (x * projectVector));
        }

        public void ExecuteUpdate () {
            // if current state is grounded and get jump input, cache it (unless we just jumped this frame, because update comes after fixedupdate)
        }

        public void Move (bool readInput) {
            StartMove(out var surfacePoint);
            if(surfacePoint == null){
                AerialMovement(readInput);
            }else{
                GroundedMovement(readInput, surfacePoint);
            }
            // gravity? per movement i guess...
            FinishMove();
        }

        void StartMove (out SurfacePoint surfacePoint) {
            // if i do the spherecast method, i can process collisions immediately, right?
            // collide with ground, DEFINITELY grounded (but check, just to be sure)
            // no collisions at all, not grounded
            // collide with ladder, laddered
            // a) capsulecast exists
            // b) cast rb.down or gravity? rb.down...

            int flattestPoint = -1;
            // float maxDot = 0;
            float maxDot = 0.0175f;     // cos(89°), to exclude walls
            for(int i=0; i<contactPoints.Count; i++){
                var dot = Vector3.Dot(contactPoints[i].normal, rb.transform.up);
                if(dot > maxDot){
                    flattestPoint = i;
                    maxDot = dot;
                }
            }
            surfacePoint = (flattestPoint != -1) ? new SurfacePoint(contactPoints[flattestPoint]) : null;

            // or go through the contacts
            // raycast along "normal" with a bit more length than the capsule radius
            // if hit, get that normal and rinse repeat
        }

        void FinishMove () {
            contactPoints.Clear();
        }

        void GroundedMovement (bool readInput, SurfacePoint surfacePoint) {
            // doesn't take slope limit into account
            // so gravity still has to point into the slope, right?
            // and sliding down is projecting gravity onto the normal and using that as acceleration, right?
            // *sigh* guess i'll look at my old code then...
            // also i kinda want braking to have more acceleration than regular accelerating...
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetVelocity = GroundMoveVector(rb.transform.TransformDirection(rawInput), surfacePoint.normal).normalized * rawInputMag * pcProps.MoveSpeedRun;
            var targetAccel = pcProps.GroundAccel;
            rb.velocity += ClampedDeltaVAcceleration(rb.velocity, targetVelocity, targetAccel) * Time.fixedDeltaTime;
            rb.velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        void AerialMovement (bool readInput) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetVelocity = rb.transform.TransformDirection(rawInput) * rawInputMag * pcProps.MoveSpeedRun;
            var targetAccel = rawInputMag * pcProps.AirAccel;
            rb.velocity += ClampedDeltaVAcceleration(HorizontalComponent(rb.velocity), HorizontalComponent(targetVelocity), targetAccel) * Time.fixedDeltaTime;
            rb.velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        // ladder = trigger
        // reason: so you can have those caged ladders when going like this _|'''
        // ladder movement is basically free movement
        // transform input vector by head instead of body
        // what about jumping off?

        // surface point instead of the flattest one maybe the "closest" one?
        // and i don't mean strictly closest to the capsule's lower center
        // i mean within a margin. flatness also matters.

        // "target speed" lerp between walk and run depending on the VALUE of that key
        // then lerp between that and the crouch speed using the normalized collider height

        // at least in 2018.4 collision STAY still occurs only on the next fixed update
        void OnCollisionEnter (Collision collision) {
            CacheContacts(collision);
        }

        void OnCollisionStay (Collision collision) {
            CacheContacts(collision);
        }

        void CacheContacts (Collision collision) {
            int cc = collision.contactCount;
            for(int i=0; i<cc; i++){
                contactPoints.Add(collision.GetContact(i));
            }
        }

    }

}