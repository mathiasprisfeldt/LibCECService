using System;
using System.ServiceProcess;
using System.Threading;

namespace LibCECServiceCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceController sc = new ServiceController(args[0], Environment.MachineName);

            for (var index = 1; index < args.Length; index++)
            {
                var s = args[index];
                sc.ExecuteCommand(Convert.ToInt32(s));
            }
        }
    }
}
