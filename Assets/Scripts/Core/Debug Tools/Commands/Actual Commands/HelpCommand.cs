namespace DebugTools.DebugCommands {

    public class HelpCommand : Command {

        public override string name => "help";
        public override string helpText => "Gives help for a given command.";

        protected override bool ValidArgumentCount (int input) => (input == 1);

        protected override string Execute (string[] args) {
            var targetCommand = args[0].ToLower();
            foreach(var cmd in Commands.GetCommands()){
                if(cmd.name.Equals(targetCommand)){
                    return cmd.helpText;
                }
            }
            return $"Unknown command \"{targetCommand}\"!";
        }

    }

}