using CommandLine;
using Streamdeck_collection.Model;
using Streamdeck_collection.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Streamdeck_collection
{
    internal partial class Program
    {
        private static readonly Dictionary<string, MethodInfo> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);

        static void Main(string[] args)
        {
            //streamdeck restart com.elgato.hello-world
            //streamdeck link [path to output folder]

            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            // Load all the actions in the assembly
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed); // static classes

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<StreamdeckActionAttribute>();
                if (attr == null) continue;

                var runMethod = type.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                if (runMethod != null)
                {
                    _actionHandlers[attr.Action] = runMethod;
                }
            }

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
