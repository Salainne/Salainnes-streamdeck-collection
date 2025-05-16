using CommandLine;
using Streamdeck_collection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            //streamdeck restart com.elgato.hello-world
            //streamdeck link [path to output folder]

            // Uncomment this line of code to allow for debugging
            // while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            // The command line args parser expects all args to use `--`, so, let's append
            for (int count = 0; count < args.Length; count++)
            {
                if (args[count].StartsWith("-") && !args[count].StartsWith("--"))
                {
                    args[count] = $"-{args[count]}";
                }
            }

            Parser parser = new Parser((with) =>
            {
                with.EnableDashDash = true;
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.IgnoreUnknownArguments = true;
                with.HelpWriter = Console.Error;
            });

            ParserResult<StreamdeckOptions> options = parser.ParseArguments<StreamdeckOptions>(args);
            options.WithParsed<StreamdeckOptions>(o => RunPlugin(o));
        }
    }
}
