using System;
using CecSharp;

namespace LibCECWrapper
{
    public class LibCECClient : CecCallbackMethods
    {
        public readonly LibCecSharp Lib;
        private readonly int _logLevel;

        public LibCECClient()
        {
            var config = new LibCECConfiguration();
            config.DeviceTypes.Types[0] = CecDeviceType.PlaybackDevice;
            config.DeviceName = "MP-S";
            config.ClientVersion = LibCECConfiguration.CurrentVersion;
            config.SetCallbacks(this);
            config.PhysicalAddress = 2000;
            config.HDMIPort = 2;
            config.ActivateSource = true;
            config.AdapterType = CecAdapterType.PulseEightExternal;
            config.TvVendor = CecVendorId.Sony;

            _logLevel = (int) CecLogLevel.All;

            Lib = new LibCecSharp(config);
            Lib.InitVideoStandalone();

            Console.WriteLine("LibCECClient: Successfully created LibCEC Parser with version: " +
                              Lib.VersionToString(config.ServerVersion));
        }

        public static LibCECClient Create(int timeout = int.MaxValue)
        {
            var client = new LibCECClient();

            if (client.Connect(timeout)) return client;

            Console.WriteLine("LibCECClient: Could not open a connection to the CEC adapter");
            return null;
        }

        public override int ReceiveLogMessage(CecLogMessage message)
        {
            if (((int) message.Level & _logLevel) == (int) message.Level)
            {
                var strLevel = Enum.GetName(typeof(CecLogLevel), message.Level)?.ToUpper() + ":   ";
                var strLog = $"LibCECClient: {strLevel} {message.Time,16} {message.Message}";
                Console.WriteLine(strLog);
            }

            return 1;
        }

        public bool Connect(int timeout)
        {
            var adapters = Lib.FindAdapters(string.Empty);

            if (adapters.Length > 0) return Connect(adapters[0].ComPort, timeout);

            Console.WriteLine("LibCECClient: Did not find any CEC adapters");
            return false;
        }

        public bool Connect(string port, int timeout)
        {
            return Lib.Open(port, timeout);
        }

        public void Close()
        {
            Lib.Close();
        }
    }
}