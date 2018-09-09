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
        private LibCECClient _client;

        public bool Start(HostControl hostControl)
        {
#if DEBUG
            if (!Environment.UserInteractive)
            {
                var logName = $@"{Directory.GetCurrentDirectory()}\{DateTime.Now:yyyy-MM-dd - hh;mm tt}.log";
                var sw = new StreamWriter(logName) { AutoFlush = true };
                Console.SetOut(sw);
            }
#endif

            _client = new LibCECClient();
            
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _client.Lib.StandbyDevices(CecLogicalAddress.Broadcast);
            _client.Close();

            if (Environment.UserInteractive)
                Environment.Exit(0);

            return true;
        }

        public void Command(int cmd) => _client.CommandAsServiceCommand(cmd);

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
                            _client.Command(cmd);
                        else if (input.StartsWith("0x"))
                            _client.CommandAsHex(input);
                        else
                            _client.Command(input);
                    }
                }
            }).Start();
        }
    }
}
