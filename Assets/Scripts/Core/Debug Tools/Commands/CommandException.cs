using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DebugTools {

    public class CommandException : Exception {

        public CommandException (string message) : base(message) {}

    }

}