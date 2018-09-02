using System.ComponentModel;
using System.ServiceProcess;

namespace LibCECService
{
    [DesignerCategory("Code")]
    public class MyService : ServiceBase
    {
        public MyService()
        {
            ServiceName = Program.SERVICE_NAME;
        }

        protected override void OnStart(string[] args)
        {
            Program.Start(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
        }

        protected override void OnCustomCommand(int command)
        {
            Program.Command(command);
        }
    }
}