using System;
using System.Net.Mime;
using Topshelf;

namespace LibCECService
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var rc = HostFactory.Run(configurator =>
            {
                configurator.Service<Service>(service =>
                {
                    Service newService = new Service();

                    service.ConstructUsing(settings => newService);
                    service.WhenStarted((service1, control) => service1.Start(control));
                    service.AfterStartingService(context => newService.AfterStartingService(context));
                    service.WhenStopped((service1, control) => service1.Stop(control));
                    service.WhenCustomCommandReceived((service1, control, arg3) => service1.Command(arg3));
                });

                configurator.SetServiceName("LibCECService");
                configurator.SetDisplayName("LibCECService");
                configurator.RunAsLocalSystem();
                configurator.StartAutomatically();
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }

    }
}