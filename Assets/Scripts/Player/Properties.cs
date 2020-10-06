using UnityEngine;

namespace PlayerController {

    [CreateAssetMenu(menuName = "Player Controller Properties", fileName = "New PlayerControllerProperties")]
    public class Properties : ScriptableObject {

        [System.Serializable]
        public struct MovementProperties {
            [SerializeField] private float speed;
            [SerializeField] private float accel;
            [SerializeField] private float drag;

            public float Speed => speed;
            public float Accel => accel;
            public float Drag => drag;

            private static MovementProperties Make (float speed, float accel, float drag) {
                MovementProperties output;
                output.speed = speed;
                output.accel = accel;
                output.drag = drag;
                return output;
            }

            public static MovementProperties GroundDefault => Make(speed: 12f, accel: 64f, drag: 32f);
            public static MovementProperties SlopeDefault =>  Make(speed: 12f, accel: 32f, drag: 12f);
            public static MovementProperties AirDefault =>    Make(speed: 12f, accel: 24f, drag: 4f);
            public static MovementProperties WaterDefault =>  Make(speed: 12f, accel: 24f, drag: 0f);
            public static MovementProperties LadderDefault => Make(speed: 6f,  accel: 64f, drag: 32f);
        }

        public enum JumpSpeedBoostMode {
            Off,
            Forward,
            ForwardAndBack,
            OmniDirectional
        }

        [Header("Dimensions")]
        [SerializeField, Unit("m")] float normalHeight = 1.8f;
        [SerializeField, Unit("m")] float crouchHeight = 0.9f;
        [SerializeField, Unit("s")] float crouchUncrouchTime = 0.25f;
        [SerializeField, Unit("m"), Tooltip("From collider top")] float eyeOffset = -0.15f;
        [SerializeField, Unit("m"), Tooltip("From collider top")] float swimOffset = -0.4f;
        [SerializeField, Unit("m")] float colliderRadius = 0.4f;

        public float NormalHeight => normalHeight;
        public float CrouchHeight => crouchHeight;
        public float CrouchUncrouchTime => crouchUncrouchTime;
        public float EyeOffset => eyeOffset;
        public float SwimOffset => swimOffset;
        public float ColliderRadius => colliderRadius;

        public float HeightChangeSpeed => (normalHeight - crouchHeight) / crouchUncrouchTime;

        [Header("Phyics")]
        [SerializeField, Unit("kg")]    float playerMass = 80f;
        [SerializeField, Unit("g/cm³")] float playerDensity = 1f;
        [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.ContinuousDynamic;
        [SerializeField] PhysicMaterial physicMaterial = null;
        [SerializeField, Unit("kg")]    float footRBNonSolidMass = 160f;
        [SerializeField, Unit("kg")]    float footRBSolidMass = 400f;
        [SerializeField, Unit("°/s")]   float gravityTurnSpeed = 360f;
        [SerializeField, Range(0f, 1f)] float minGravityTurnSpeedMultiplier = 0f;

        public float PlayerMass => playerMass;
        public float PlayerDensity => playerDensity;
        public CollisionDetectionMode CollisionDetection => collisionDetection;
        public PhysicMaterial PhysicMaterial => physicMaterial;
        public float FootRBNonSolidMass => footRBNonSolidMass;
        public float FootRBSolidMass => footRBSolidMass;
        public float GravityTurnSpeed => gravityTurnSpeed;
        public float MinGravityTurnSpeedMultiplier => minGravityTurnSpeedMultiplier;

        [Header("Camera")]
        [SerializeField, Unit("m")] float nearClipDist = 0.15f;
        [SerializeField, Unit("m")] float farClipDist = 1000f;

        public float NearClipDist => nearClipDist;
        public float FarClipDist => farClipDist;

        [Header("Movement")]
        [SerializeField, Unit("°")] float hardSlopeLimit = 60f;
        [SerializeField] float runSpeedMultiplier = 1f;
        [SerializeField] float walkSpeedMultiplier = 0.5f;
        [SerializeField] float crouchSpeedMultiplier = 0.5f;
        [SerializeField] MovementProperties ground = MovementProperties.GroundDefault;
        [SerializeField] MovementProperties slope = MovementProperties.SlopeDefault;
        [SerializeField] MovementProperties air = MovementProperties.AirDefault;
        const string parabolaTip = "At high speeds, applying move input in the flight direction will result in no drag being applied, allowing a parabolic arc instead of a shortened one.";
        [SerializeField, Tooltip(parabolaTip)] bool enableFullFlightParabola = false;
        [SerializeField] MovementProperties water = MovementProperties.WaterDefault;
        [SerializeField] MovementProperties ladder = MovementProperties.LadderDefault;
        
        public float HardSlopeLimit => hardSlopeLimit;
        public float RunSpeedMultiplier => runSpeedMultiplier;
        public float WalkSpeedMultiplier => walkSpeedMultiplier;
        public float CrouchSpeedMultiplier => crouchSpeedMultiplier;
        public MovementProperties Ground => ground;
        public MovementProperties Slope => slope;
        public MovementProperties Air => air;
        public bool EnableFullFlightParabola => enableFullFlightParabola;
        public MovementProperties Water => water;
        public MovementProperties Ladder => ladder;

        [Header("Jumping")]
        [SerializeField, Unit("m")] float standingJumpHeight = 1f;
        [SerializeField, Unit("m")] float crouchedJumpHeight = 0.5f;
        [SerializeField, Unit("m/s²")] float jumpCalcGravity = 29.43f;
        [SerializeField] JumpSpeedBoostMode jumpSpeedBoost = JumpSpeedBoostMode.Off;
        [SerializeField] float jumpSpeedBoostMultiplier = 1.0f;
        const string overBoostTip = "Allow jump boost even when speed is greater than maximum move speed";
        [SerializeField, Tooltip(overBoostTip)] bool enableOverBoosting = false;
        const string bunnyHopTip = "Block deceleration on landing if a frame-perfect jump input is present";
        [SerializeField, Tooltip(bunnyHopTip)] bool enableBunnyHopping = false;
        
        public float StandingJumpHeight => standingJumpHeight;
        public float CrouchedJumpHeight => crouchedJumpHeight;
        public float JumpCalcGravity => jumpCalcGravity;
        public JumpSpeedBoostMode JumpSpeedBoost => jumpSpeedBoost;
        public float JumpSpeedBoostMultiplier => jumpSpeedBoostMultiplier;
        public bool EnableOverBoosting => enableOverBoosting;
        public bool EnableBunnyHopping => enableBunnyHopping;

        [Header("Interaction")]
        [SerializeField, Unit("m")] float interactRange = 1.5f;

        public float InteractRange => interactRange;

    }

}