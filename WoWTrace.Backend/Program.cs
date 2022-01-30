using CommandLine;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Threading;

namespace WoWTrace.Backend
{
    internal class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        static Logger logger = LogManager.GetCurrentClassLogger();

        public class Options
        {
            [Option("enqueueAllBuilds", Required = false, HelpText = "Enqueue all availabe builds (just needed to process manuel instered builds)")]
            public bool EnqueueAllBuilds { get; set; }


            [Option("enqueueAllBuildsEveryFiveHours", Required = false, HelpText = "Enqueue all availabe builds every 5 hours")]
            public bool EnqueueAllBuildsEveryFiveHours { get; set;}
        }

        static void Main(string[] args)
        {
            PrintLogo();

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => new WoWTraceBackend(o));

            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            _quitEvent.WaitOne();
        }

        static void PrintLogo()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(@"__        __ __        _______                   ");
            Console.WriteLine(@"\ \      / /_\ \      / /_   _| __ __ _  ___ ___ ");
            Console.WriteLine(@" \ \ /\ / / _ \ \ /\ / /  | || '__/ _` |/ __/ _ \");
            Console.WriteLine(@"  \ V  V / (_) \ V  V /   | || | | (_| | (_|  __/");
            Console.WriteLine(@"   \_/\_/ \___/ \_/\_/    |_||_|  \__,_|\___\___|");
            Console.WriteLine("    https://wowtrace.net                  Backend\n");
            Console.ResetColor();
        }
    }
}
