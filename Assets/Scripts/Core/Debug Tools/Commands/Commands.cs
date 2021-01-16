using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using DebugTools.DebugCommands;

namespace DebugTools {

    public static class Commands {

        // help with no args just lists all commands 
        // help with 1 arg does the help for that arg
        // 2d arg array thingies for variants?
        // descriptions per variant?
        // ->
        // help help
        //   Gives help
        //   {no args} Lists all commands
        //   {command:string} Gives help for command "command"
        // help logTime
        //   Logs the current system time to the console
        //   {no args}
        // help god
        //   God Mode (invincibility)
        //   {value:bool} on or off
        // god true
        //   God mode enabled

        // TODO think about this stuff ^^^^
        // TODO non-debug-log thing to enter the commands into the console
        // probably best to rename it to console
        // activate text input when cursor is unlocked
        // auto clear text input on return
        // TODO KISS. don't make it too complicated. if it works, that's fine. 

        private static Dictionary<string, Command> commands;

        // TODO this happens every time on startup but is hardly necessary, unless i just added new commands
        // make use of code generation to generate a file that has all the things in it, so i do the reflection once offline and not during runtime
        // also copy baste's layer code generation thingy and make it do my stuff. so not layer.mask.default but rather layer.default.mask
        static Commands () {
            commands = new Dictionary<string, Command>();
            List<string> problematicCommands = new List<string>();
            List<string> invalidCommands = new List<string>();
            try{
                var assembly = Assembly.GetAssembly(typeof(Command));
                var types = assembly.GetTypes();
                foreach(var type in types){
                    if(!type.IsAbstract && type.IsSubclassOf(typeof(Command))){
                        var newCommand = (Command)(Activator.CreateInstance(type));
                        var rawName = newCommand.name;
                        var newName = rawName.Trim().ToLower();
                        if(newName.Split(null).Length > 1){
                            invalidCommands.Add(rawName);
                            continue;
                        }
                        if(!newName.Equals(rawName)){
                            problematicCommands.Add(rawName);
                        }
                        commands.Add(newName, newCommand);
                    }
                }
            }catch(Exception e){
                Debug.LogException(e);
            }
            LogListIfNotEmpty("Problematic Commands", problematicCommands, Debug.LogWarning);
            LogListIfNotEmpty("Invalid Commands", invalidCommands, Debug.LogError);

            void LogListIfNotEmpty (string title, List<string> list, System.Action<string> log) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if(list.Count > 0){
                    sb.AppendLine(title);
                    foreach(var cmd in list){
                        sb.AppendLine($" - \"{cmd}\"");
                    }
                    log(sb.ToString());
                    sb.Clear();
                }
            }
        }

        public static IEnumerable<Command> GetCommands () {
            return commands.Values;
        }

        public static string Execute (string input) {
            try{
                var commandName = ParseCommandNameAndArguments(input, out var args);
                if(!commands.TryGetValue(commandName, out var command)){
                    throw new CommandException($"Invalid command \"{commandName}\".");
                }
                return command.Run(args);
            }catch(Exception e){
                if(!(e is CommandException)){
                    Debug.LogException(e);
                }
                return e.Message;
            }
        }

        private static string ParseCommandNameAndArguments (string input, out string[] args) {
            if(string.IsNullOrWhiteSpace(input)){
                throw new CommandException("No command given.");
            }
            var split = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            var command = split[0];
            args = new string[split.Length-1];
            for(int i=1; i<split.Length; i++){
                args[i-1] = split[i];
            }
            return command.ToLower();
        }
        
    }

}