using CommandLine;
using NLog;

namespace WoWTrace.Backend
{
    internal abstract class Program
    {
        private static readonly ManualResetEvent _quitEvent = new(false);
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public class Options
        {
            [Option("enqueueAllBuilds", Required = false, HelpText = "Enqueue all available builds (just needed to process manuel inserted builds)")]
            public bool EnqueueAllBuilds { get; set; }

            [Option("enqueueAllBuildsEveryFiveHours", Required = false, HelpText = "Enqueue all available builds every 5 hours")]
            public bool EnqueueAllBuildsEveryFiveHours { get; set;}
        }

        public static void Main(string[] args)
        {
            PrintLogo();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => _ = new WoWTraceBackend(o));

            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            _quitEvent.WaitOne();
        }

        private static void PrintLogo()
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