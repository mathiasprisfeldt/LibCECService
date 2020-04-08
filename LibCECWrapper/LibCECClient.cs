using System;
using System.Threading;
using CecSharp;

namespace LibCECWrapper {
    public class LibCECClient : CecCallbackMethods {
        private const int CMD_DELAY = 250;

        private bool _settingActiveSource;
        private readonly int _logLevel;

        public readonly LibCecSharp Lib;

        public LibCECClient() {
            var config = new LibCECConfiguration();
            config.DeviceTypes.Types[0] = CecDeviceType.PlaybackDevice;
            config.DeviceName = "MP-S";
            config.ClientVersion = LibCECConfiguration.CurrentVersion;
            config.SetCallbacks(this);
            //config.PhysicalAddress = 1000;
            config.HDMIPort = 1;
            config.AdapterType = CecAdapterType.PulseEightExternal;
            config.TvVendor = CecVendorId.Philips;

            config.ActivateSource = false;

            _logLevel = (int) CecLogLevel.All;

            Lib = new LibCecSharp(config);
            Lib.InitVideoStandalone();

            Log("LibCECClient: Successfully created LibCEC Parser with version: " +
                Lib.VersionToString(config.ServerVersion));
        }

        public static void Log(string msg) {
            Console.WriteLine($"{DateTime.Now} LibCECClient: {msg}");
        }

        public static LibCECClient Create(int timeout = int.MaxValue) {
            var client = new LibCECClient();

            if (client.Connect(timeout))
                return client;

            Log("LibCECClient: Could not open a connection to the CEC adapter");
            return null;
        }

        public override int ReceiveLogMessage(CecLogMessage message) {
            if (((int) message.Level & _logLevel) == (int) message.Level) {
                var strLevel = Enum.GetName(typeof(CecLogLevel), message.Level)?.ToUpper() + ":   ";
                var strLog = $"LibCECClient: {strLevel} {message.Time,16} {message.Message}";
                Console.WriteLine(strLog);
            }

            return 1;
        }

        public void Transmit(CecCommand cmd, bool activateSource = false) {
            if (cmd.Opcode == CecOpcode.Standby &&
                Lib.GetDevicePowerStatus(CecLogicalAddress.Tv) != CecPowerStatus.On)
                return;

            if (_settingActiveSource)
                return;

            if (activateSource) {
                _settingActiveSource = true;
                Lib.SetActiveSource(CecDeviceType.PlaybackDevice);

                /*
                 * Since we dont know when its fully connected as a source,
                 * we have to take a guess on when its done, if we dont
                 * our queued cmd wont trigger.
                 */
                Thread.Sleep(CMD_DELAY);

                _settingActiveSource = false;
                Transmit(cmd);

                return;
            }

            //First try and transmit command, if that fails try and reconnect the send again.
            if (!Lib.Transmit(cmd)) {
                Transmit(cmd, true);
                return;
            }

            //We need some delay if we suceeded with the transmission,
            //else the tv will spazz out if overloaded with commands.
            Thread.Sleep(CMD_DELAY);
        }

        public bool Connect(int timeout) {
            var adapters = Lib.FindAdapters(string.Empty);

            if (adapters.Length > 0)
                return Connect(adapters[0].ComPort, timeout);

            Log("LibCECClient: Did not find any CEC adapters");
            return false;
        }

        public bool Connect(string port, int timeout) {
            return Lib.Open(port, timeout);
        }

        public void Close() {
            Lib.Close();
        }
    }
}