using System;
using System.Net.Mime;
using Topshelf;

namespace LibCECService
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(configurator =>
            {
                configurator.Service<Service>(service =>
                {
                    service.ConstructUsing(settings => new Service());
                    service.WhenStarted((service1, control) => service1.Start(control));
                    service.WhenStopped((service1, control) => service1.Stop(control));
                    service.WhenCustomCommandReceived((service1, control, arg3) => service1.Command(arg3));
                });

                configurator.SetServiceName("LibCECService");
                configurator.SetDisplayName("LibCECService");
                configurator.RunAsLocalSystem();
                configurator.StartAutomatically();
            });
            
            //This code only gets executed if the program is not run as a real service.
            var localService = new Service();

            while (true)
            {
                string input = Console.ReadLine();

                if (input == String.Empty)
                    continue;

                if (input == "exit")
                    return;

                var cmdIsInt = int.TryParse(input, out var cmd);
                if (cmdIsInt)
                    localService.Command(cmd);
                else
                    localService.Command(input);
            }
        }

    }
}