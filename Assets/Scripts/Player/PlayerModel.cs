﻿using UnityEngine;

namespace PlayerController {

    public class PlayerModel : MonoBehaviour {

        [SerializeField] Transform bottomSphere = default;
        [SerializeField] Transform topSphere = default;

        Properties pcProps;
        Movement movement;
        Transform head;

        public void Initialize (Properties pcProps, Movement movement, Transform head) {
            this.pcProps = pcProps;
            this.movement = movement;
            this.head = head;
        }

        public void UpdateSpherePositions () {
            var r = movement.LocalColliderRadius;
            bottomSphere.localPosition = new Vector3(0f, r, 0f);
            topSphere.position = head.position - (transform.up * (pcProps.EyeOffset + r));
            var scale = 2f * r * Vector3.one;
            bottomSphere.localScale = scale;
            topSphere.localScale = scale;
        }
        
    }

}