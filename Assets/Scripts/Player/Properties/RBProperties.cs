using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    [CreateAssetMenu(menuName = "Player/Player Controller Properties (RB)", fileName = "New RBProperties")]
    public class RBProperties : Properties {

        [Header("Rigidbody Settings")]
        [SerializeField, Unit("kg")] float playerMass = 80f;
        [SerializeField] PhysicMaterial physicMaterial = null;
        [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.ContinuousDynamic;

        public float PlayerMass => playerMass;
        public PhysicMaterial PhysicMaterial => physicMaterial;
        public CollisionDetectionMode CollisionDetection => collisionDetection;

        [Header("Environment Physics")]
        [SerializeField, Unit("kg")] float footRBNonSolidMass = 160f;
        [SerializeField, Unit("kg")] float footRBSolidMass = 400f;

        public float FootRBNonSolidMass => footRBNonSolidMass;
        public float FootRBSolidMass => footRBSolidMass;

    }

}