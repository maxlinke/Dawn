using UnityEngine;

namespace PlayerController {

    public abstract class Properties : ScriptableObject {

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

        const float inf = float.PositiveInfinity;

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
        [SerializeField, Unit("g/cm³")] float playerDensity = 1f;
        [SerializeField, Unit("m/s²")] float normalGravity = 29.43f;
        [SerializeField, Unit("°/s")]   float gravityTurn = 360f;
        [SerializeField, Range(0f, 1f)] float minGravityTurn = 0f;

        public float PlayerDensity => playerDensity;
        public float NormalGravity => normalGravity;
        public float GravityTurnSpeed => gravityTurn;
        public float MinGravityTurnSpeedMultiplier => minGravityTurn;

        [Header("Camera")]
        [SerializeField, Unit("m")] float nearClipDist = 0.15f;
        [SerializeField, Unit("m")] float farClipDist = 1000f;

        public float NearClipDist => nearClipDist;
        public float FarClipDist => farClipDist;

        const string parabolaTip = "Deactivates drag if input is appropriate, allowing a full parabolic flight arc instead of a dampened one.";
        [Header("Movement")]
        [SerializeField, Unit("°")] float hardSlopeLimit = 60f;
        [SerializeField] float runSpeedMultiplier = 1f;
        [SerializeField] float walkSpeedMultiplier = 0.5f;
        [SerializeField] float crouchSpeedMultiplier = 0.5f;
        [SerializeField] MovementProperties ground = MovementProperties.GroundDefault;
        [SerializeField] MovementProperties slope = MovementProperties.SlopeDefault;
        [SerializeField] MovementProperties air = MovementProperties.AirDefault;
        [SerializeField] MovementProperties water = MovementProperties.WaterDefault;
        [SerializeField] MovementProperties ladder = MovementProperties.LadderDefault;
        [SerializeField, Tooltip(parabolaTip)] bool enableFullFlightParabola = false;
        
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
        
        const string overBoostTip = "Allow jump boost even when speed is greater than maximum move speed";
        const string bunnyHopTip = "Block deceleration on landing if a frame-perfect jump input is present";
        [Header("Jumping")]
        [SerializeField, Unit("m")] float standingJumpHeight = 1f;
        [SerializeField, Unit("m")] float crouchedJumpHeight = 0.5f;
        [SerializeField] JumpVelocityMode velocityMode = JumpVelocityMode.AddGlobalVelocity;
        [SerializeField, Range(0f, 1f)] float minJumpVelocity = 0f;
        [SerializeField] bool limitDescentJumpHeight = false;
        [SerializeField, CustomRange(1f, inf)] float boostMultiplier = 1.0f;
        [SerializeField] JumpBoostDirection boostDirection = JumpBoostDirection.OmniDirectional;
        [SerializeField, Tooltip(overBoostTip)] bool enableOverBoosting = false;
        [SerializeField, CustomRange(0f, 1f)] float landingSpeedMultiplier = 1.0f;
        [SerializeField, Tooltip(bunnyHopTip)] bool enableBunnyHopping = false;
        
        public float StandingJumpHeight => standingJumpHeight;
        public float CrouchedJumpHeight => crouchedJumpHeight;
        public JumpVelocityMode JumpVelocityMode => velocityMode;
        public float MinJumpVelocity => minJumpVelocity;
        public bool LimitDescentJumpHeight => limitDescentJumpHeight;
        public float BoostMultiplier => boostMultiplier;
        public JumpBoostDirection JumpBoostDirection => boostDirection;
        public bool EnableOverBoosting => enableOverBoosting;
        public float LandingMultiplier => landingSpeedMultiplier;
        public bool EnableBunnyHopping => enableBunnyHopping;

        [Header("Interaction")]
        [SerializeField, Unit("m")] float interactRange = 1.5f;

        public float InteractRange => interactRange;

    }

}