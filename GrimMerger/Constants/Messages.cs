using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimMerger.Constants
{
    internal struct CommandLineMessages
    {
        internal struct Logic
        {
            internal static string GoToDir => "cd \"{0}\"";
            internal static string ExtractFiles => ".\\archivetool \"{0}\" -extract \"{1}\"";
            internal static string ExtractDatabase => ".\\archivetool \"{0}\" -database \"{1}\"";
            internal static string PackFiles => ".\\archivetool \"{0}\" -update . \"{1}\" 6";
            internal static string PackDatabase => "{0}\\arzedit build \"{1}\" \"{2}\" -g \"{3}\" -t \"{4} \\database\\templates\" -A -R";
        }

        internal static string CouldNotProcessMessageToCommandPrompt => "Could not process unknown type of message.";

        internal static string Start => "Command Prompt was started.";
        internal static string Stop => "Command Prompt was closed.";
    }
}
