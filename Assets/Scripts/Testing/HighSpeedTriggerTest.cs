using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Testing {

    public class HighSpeedTriggerTest : MonoBehaviour {

        [SerializeField] Rigidbody rb = default;
        [SerializeField] float speed = default;
        [SerializeField, Range(0f, 1f)] float offset = default;

        Tester tester;
        float nextReset;

        void Start () {
            if(rb != null){
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.drag = 0f;
                tester = rb.gameObject.AddComponent<Tester>();
                tester.triggerEnter = true;     // so it doesn't log on the first "reset"
                nextReset = -1f;
            }
        }

        void FixedUpdate () {
            if(tester != null && Time.time > nextReset){
                if(!tester.triggerEnter){
                    Debug.Log($"{Time.frameCount} no trigger enter!");
                }
                rb.transform.position = GetStartPoint();
                rb.velocity = transform.forward * speed;
                tester.triggerEnter = false;
                nextReset = Time.time + 1f;
            }
        }

        Vector3 GetStartPoint () {
            return transform.position + transform.forward * speed * Time.fixedDeltaTime * offset;
        }

        void OnDrawGizmosSelected () {
            var start = GetStartPoint();
            var cs = Vector3.one * 0.1f;
            var gc = Gizmos.color;
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(start, cs);
            var last = start;
            for(int i=0; i<16; i++){
                var next = last + transform.forward * speed * Time.fixedDeltaTime;
                Gizmos.DrawLine(last, next);
                Gizmos.DrawCube(next, cs);
                last = next;
            }
        }

        class Tester : MonoBehaviour {

            public bool triggerEnter = false;

            void OnTriggerEnter (Collider other) {
                triggerEnter = true;
            }

        }
        
    }

}