﻿using UnityEngine;
using System.Collections;

// https://forum.unity.com/threads/physics-drag-formula.252406/
// https://answers.unity.com/questions/652010/how-drag-is-calculated-by-unity-engine.html
    
namespace Testing {

    public class TransformDropTest : MonoBehaviour {
        
        public float mass = 1f;
        public float drag;
        public Vector3 acceleration = Vector3.zero;
        public Vector3 gravity = Physics.gravity;
        
        private int fixedUpdateTicks = -1;
        public bool atRest = false;

        public bool logInfo = true;
        
        void FixedUpdate() {
        
            //Stop moving if we're at rest.
            if (atRest)
                return;
        
            //Log debugging information
            fixedUpdateTicks++;
            if(logInfo){
                Debug.Log (string.Format (
                    "Fixed Update Ticks: {0}\n" +
                    "Position: {1}\n" +
                    //To derive velocity from acceleration, we need to divide by time
                    "Velocity: {2}\n", fixedUpdateTicks, transform.position, acceleration.magnitude/Time.fixedDeltaTime));
            }
        
            //Add the force
            AddForce (gravity, ForceMode.Acceleration);
        
            //Apply Linear Damping (drag)
            ApplyDrag ();
        
            //Move the object
            transform.position += acceleration;
        
        }
        
        void OnTriggerEnter() {
            atRest = true;
        }
        
        public void AddForce(Vector3 force, ForceMode forceType) {
            switch (forceType) {
            case ForceMode.Force:
                //Force mode moves an object according to it's mass
                acceleration = acceleration + force * mass * Time.fixedDeltaTime * Time.fixedDeltaTime;
                break;
            case ForceMode.Acceleration:
                //Acceleration ignores mass
                acceleration = acceleration + force * Time.fixedDeltaTime * Time.fixedDeltaTime;
                break;
            default:
                throw new UnityException("Force mode not supported!");
            }
        }
        
        //Apply Linear Damping
        public void ApplyDrag() {
            acceleration = acceleration * (1 - Time.deltaTime * drag);       // Time.deltaTime is no error, inside FixedUpdate it is the same as Time.fixedDeltaTime!
        }
    }
           
}