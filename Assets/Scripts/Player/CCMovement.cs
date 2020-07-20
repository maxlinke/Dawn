using UnityEngine;

namespace PlayerUtils {

    public class CCMovement : Movement {

        [SerializeField] CharacterController cc = default;

        public override float Height => cc.height;
        public override Vector3 Velocity => Vector3.zero;   // TODO

        public override void Initialize (PlayerConfig config, Player player) {
            base.Initialize(config, player);
        }

        public override void ExecuteUpdate () {
            if(!initialized){
                return;
            }
            var localMoveInput = TEMP_GetLocalSpaceMoveInput();
            var worldMoveInput = transform.TransformDirection(localMoveInput);
            cc.Move(worldMoveInput * config.MoveSpeed * Time.deltaTime);
        }

        public override void ExecuteFixedUpdate () {

        }
    
    }

}