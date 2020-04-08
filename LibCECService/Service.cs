using System;
using System.IO;
using System.Threading;
using CecSharp;
using LibCECWrapper;
using Topshelf;

namespace LibCECService
{
    public class Service : ServiceControl
    {
        private LibCECClient client;

        public bool Start(HostControl hostControl)
        {
#if DEBUG
            if (!Environment.UserInteractive)
            {
                var logDir = Directory.GetCurrentDirectory() + @"\Logs";

                Directory.CreateDirectory(logDir);

                var logName = $@"{logDir}\{DateTime.Now:yyyy-MM-dd - hh;mm tt}.log";
                var sw = new StreamWriter(logName) { AutoFlush = true };
                Console.SetOut(sw);
            }
#endif

            client = LibCECClient.Create();
            
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            client.Lib.StandbyDevices(CecLogicalAddress.Tv);
            client.Close();

            if (Environment.UserInteractive)
                Environment.Exit(0);

            return true;
        }

        public void Command(int cmd) => client.CommandAsServiceCommand(cmd);

        public void AfterStartingService(HostStartedContext context)
        {
            //CLI for when running the service as a normal console program, runned in its own thread
            //because Topshelf reserves everything for itself on the main thread.
            new Thread(() =>
            {
                if (Environment.UserInteractive)
                {
                    while (true)
                    {
                        string input = Console.ReadLine();

                        if (string.IsNullOrEmpty(input))
                            continue;

                        var cmdIsInt = int.TryParse(input, out var cmd);
                        if (cmdIsInt)
                            client.Command(cmd);
                        else if (input.StartsWith("0x"))
                            client.CommandAsHex(input);
                        else
                            client.Command(input);
                    }
                }
            }).Start();
        }
    }
}
