using UnityEngine;

namespace DebugTools.DebugCommands {

    public class LogCommand : Command {

        public override string name => "log";
        public override string helpText => "Logs the following argument(s).";

        protected override bool ValidArgumentCount (int input) => true;

        protected override string Execute(string[] args) {
            string output = string.Empty;
            foreach(var arg in args){
                output += $"{arg} ";
            }
            log(output);
            return string.Empty;
        }

        protected virtual System.Action<string> log => Debug.Log;
    }

    public class LogWarningCommand : LogCommand {

        public override string name => "logwarning";
        public override string helpText => "Logs the following argument(s) as a warning.";

        protected override System.Action<string> log => Debug.LogWarning;

    }

    public class LogErrorCommand : LogCommand {

        public override string name => "logerror";
        public override string helpText => "Logs the following argument(s) as an error.";

        protected override System.Action<string> log => Debug.LogError;

    }

}