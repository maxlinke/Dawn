using UnityEngine;

namespace PlayerUtils {

    public class RBMovement : Movement {

        [SerializeField] CapsuleCollider cc = default;
        [SerializeField] Rigidbody rb = default;

        public override float Height => cc.height;
        public override Vector3 Velocity => rb.velocity;    // TODO maybe use an internal variable? this might fluctuate in unexpected ways...

        public override void Initialize (PlayerConfig config, Player player) {
            base.Initialize(config, player);
            cc.height = config.NormalHeight;
            cc.radius = config.ColliderRadius;
            cc.center = new Vector3(0f, cc.height / 2f, 0f);
            cc.material = config.PhysMat;
            rb.mass = config.MoveMass;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void OnDrawGizmos () {
            if(!initialized){
                return;
            }
            var gc = Gizmos.color;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(rb.worldCenterOfMass, 0.5f * cc.radius);
            Gizmos.color = gc;
        }

        public override void ExecuteUpdate () {
            // TODO cache jump inputs for next fixedupdate BUT also check for current jump wish!!! in the fixedupdate...
        }

        // TODO local speed conversion. not just for surfaces, also for volumes... volume takes priority, then surface, then nothing.
        // TODO input method thingy.. where i can ask the float value of an input that can be either an axis or a key etc...
        // TODO mass for player < "mass" that it simulates via force down...
        //      so that the player doesn't exert a ridiculous force from moving alone (high acceleration and all that)

        public override void ExecuteFixedUpdate () {
            
        }
    }


}