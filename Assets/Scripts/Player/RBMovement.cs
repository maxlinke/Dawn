using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        // public void Move (bool readInput) {
        //     if(!initialized){
        //         Debug.LogWarning($"{nameof(RBMovement)} isn't initialized yet!");
        //         return;
        //     }
        //     StartMove(out var currentState);
        //     switch(controlMode){
        //         case ControlMode.FULL:
        //             ExecuteMove(readInput, currentState);
        //             break;
        //         case ControlMode.BLOCK_INPUT:
        //             ExecuteMove(false, currentState);
        //             break;
        //         case ControlMode.ANCHORED:
        //             Velocity = Vector3.zero;
        //             break;
        //         default:
        //             Debug.LogError($"Unknown {nameof(ControlMode)} \"{controlMode}\"!");
        //             Velocity = Vector3.zero;
        //             break;
        //     }
        //     FinishMove(currentState);
        // }
        
    }

}