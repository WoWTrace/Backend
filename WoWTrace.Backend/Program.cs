using CommandLine;
using System;
using System.Threading;

namespace WoWTrace.Backend
{
    internal class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public class Options
        {
            [Option("enqueueAllBuilds", Required = false, HelpText = "Enqueue all availabe builds (just needed to process manuel instered builds)")]
            public bool EnqueueAllBuilds { get; set; }


            [Option("enqueueAllBuildsEveryFiveHours", Required = false, HelpText = "Enqueue all availabe builds every 5 hours")]
            public bool EnqueueAllBuildsEveryFiveHours { get; set;}
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => new WoWTraceBackend(o));



            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            _quitEvent.WaitOne();
        }
    }
}
