using System.Collections.Generic;
using UnityEngine;

namespace Testing {

    public class DeltaTimeTest : MonoBehaviour {

        [SerializeField, Range(1, 100)] int testCount = 10;
        [SerializeField] bool startTest = false;

        bool run;
        List<string> lines;
        System.Text.StringBuilder sb;

        void Start () {
            lines = new List<string>();
            sb = new System.Text.StringBuilder();
            run = false;
        }

        void Update () {
            run |= startTest;
            startTest = false;
            if(run){
                AddLineAndResetIfNeeded("--");
            }
        }

        void FixedUpdate () {
            if(run){
                AddLineAndResetIfNeeded("FU");
            }
        }

        void AddLineAndResetIfNeeded (string prefix) {
            lines.Add($"{Time.frameCount} {prefix} {Time.deltaTime:F5}");
            if(lines.Count >= testCount){
                foreach(var line in lines){
                    sb.AppendLine(line);
                }
                Debug.Log(sb.ToString());
                sb.Clear();
                lines.Clear();
                run = false;
            }
        }
        
    }

}