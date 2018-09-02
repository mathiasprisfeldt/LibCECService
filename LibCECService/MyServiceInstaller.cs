using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace LibCECService
{
    [DesignerCategory("Code")]
    [RunInstaller(true)]
    public class MyServiceInstaller : Installer
    {
        public MyServiceInstaller()
        {
            var spi = new ServiceProcessInstaller();
            var si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Username = null;
            spi.Password = null;

            si.DisplayName = Program.SERVICE_NAME;
            si.ServiceName = Program.SERVICE_NAME;
            si.StartType = ServiceStartMode.Automatic;

            Installers.Add(spi);
            Installers.Add(si);
        }
    }
}