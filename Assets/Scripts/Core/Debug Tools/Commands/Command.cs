namespace DebugTools.DebugCommands {

    public abstract class Command {

        public abstract string name { get; }
        public abstract string helpText { get; }

        protected abstract bool ValidArgumentCount (int input);
        protected abstract string Execute (string[] args);

        public string Run (string[] args) {
            if(!ValidArgumentCount(args.Length)){
                throw new CommandException($"Wrong number of arguments ({args.Length})!");
            }
            return Execute(args);
        }
        
    }

}