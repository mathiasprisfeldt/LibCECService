using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
            if (!Environment.UserInteractive)
            {
                var logName = $@"{Directory.GetCurrentDirectory()}\{DateTime.Now:yyyy-MM-dd - hh;mm tt}.log";
                var sw = new StreamWriter(logName) { AutoFlush = true };
                Console.SetOut(sw);
            }

            Console.WriteLine("Service started.");

            _client = LibCECClient.Create();
            _client.Lib.StandbyDevices(CecLogicalAddress.Broadcast);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Console.WriteLine("Service stopped.");

            _client.Close();

            return true;
        }

        public void Command(CecOpcode cmd)
        {
            Console.WriteLine($"Received Command: {cmd}");

            CecCommand newCommand = new CecCommand
            {
                Initiator = CecLogicalAddress.PlaybackDevice1,
                Destination = CecLogicalAddress.Tv,
                Opcode = cmd
            };

            _client.Lib.Transmit(newCommand);
        }

        public void Command(string cmd)
        {
            if (cmd.StartsWith("0x"))
            {
                bool succeded = true;
                cmd = cmd.Remove(0, 2);

                var hexValues = cmd.Split(':');

                if (hexValues.Length == 0)
                    succeded = false;

                byte[] hexParsed = new byte[hexValues.Length];
                for (var i = 0; i < hexValues.Length; i++)
                {
                    var hexVal = hexValues[i];
                    bool parsed = Byte.TryParse(hexVal, NumberStyles.HexNumber, null, out hexParsed[i]);
                    succeded = parsed;
                }

                if (!succeded)
                {
                    Console.WriteLine($"Tried to parse CMD \"{cmd}\" as hex values, but failed.");
                    return;
                }

                Command(hexParsed);
                return;
            }

            string[] strings = Enum.GetNames(typeof(CecOpcode));
            if (!strings.Contains(cmd, StringComparer.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($"{cmd} is not a CEC command!");
                return;
            }

            CecOpcode cecCmd = (CecOpcode)Enum.Parse(typeof(CecOpcode), cmd, true);
            Command(cecCmd);
        }

        public void Command(int cmd)
        {
            //If the cmd has a value below cmd is registered as representated in hex values.
            if (cmd < 0)
            {
                cmd *= -1;
                string hex = cmd.ToString("X");

                int insertIndex = 1;
                for (var i = 0; i < hex.Length - insertIndex; i++)
                    if (i % 2 == 1)
                    {
                        hex = hex.Insert(i + insertIndex, ":");
                        insertIndex++;
                    }

                Command($"0x{hex}");
            }

            if (cmd == 129)
            {
                _client.Lib.StandbyDevices(CecLogicalAddress.Tv);
                return;
            }

            if (cmd == 130)
            {
                _client.Lib.PowerOnDevices(CecLogicalAddress.Tv);
                return;
            }

            if (Enum.IsDefined(typeof(CecOpcode), cmd))
                Command((CecOpcode)cmd);
        }

        public void Command(params byte[] values)
        {
            var cmd = new CecCommand();

            foreach (byte value in values)
                cmd.PushBack(value);

            _client.Lib.Transmit(cmd);
        }
    }
}
