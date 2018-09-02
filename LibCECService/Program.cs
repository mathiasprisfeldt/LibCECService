using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using LibCECWrapper;

namespace LibCECService
{
    public static class Program
    {
        public const string SERVICE_NAME = "LibCECService";

        private static LibCECClient _client;

        private static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                using (var service = new MyService())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            InstallService();
                            break;
                        case "-uninstall":
                            UnInstallService();
                            break;
                    }

                    Console.ReadKey();
                    return;
                }

                Start(args);

                char currKey;
                while ((currKey = Console.ReadKey(true).KeyChar) != Convert.ToChar(27))
                {
                    var result = int.TryParse(currKey.ToString(), out var cmd);
                    if (result)
                        Command(cmd);
                }

                Stop();
            }
        }

        public static void Start(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                var logName = $@"{Directory.GetCurrentDirectory()}\{DateTime.Now:yyyy-MM-dd - hh;mm tt}.log";
                var sw = new StreamWriter(logName) {AutoFlush = true};
                Console.SetOut(sw);
            }

            Console.WriteLine("Service started.");

            _client = LibCECClient.Create();
        }

        public static void Stop()
        {
            Console.WriteLine("Service stopped.");

            _client.Close();
        }

        public static void Command(int command)
        {
            Console.WriteLine($"Received Command: {command}");
        }

        public static void InstallService()
        {
            Console.WriteLine("Installing LibCECService...");
            ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
        }

        public static void UnInstallService()
        {
            ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
        }
    }
}