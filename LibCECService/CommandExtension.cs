using System;
using System.Globalization;
using System.Linq;
using CecSharp;
using LibCECWrapper;

namespace LibCECService
{
    /// <summary>
    /// A set of ways to send a command to an instance of <see cref="LibCECClient"/>.
    /// </summary>
    public static class CommandExtension
    {
        public static void Command(this LibCECClient client, params byte[] values)
        {
            var cmd = new CecCommand();

            foreach (byte value in values)
                cmd.PushBack(value);

            client.Transmit(cmd);
        }

        public static void Command(this LibCECClient client, int cmd)
        {
            if (Enum.IsDefined(typeof(CecOpcode), cmd))
                client.Command((CecOpcode)cmd);
        }

        /// <summary>
        /// Send command via the command enum <see cref="CecOpcode"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd"></param>
        public static void Command(this LibCECClient client, CecOpcode cmd)
        {
            Console.WriteLine($"Received Command: {cmd}");

            CecCommand newCommand = new CecCommand
            {
                Initiator = CecLogicalAddress.PlaybackDevice1,
                Destination = CecLogicalAddress.Tv,
                Opcode = cmd
            };

            client.Transmit(newCommand);
        }

        public static void CommandAsServiceCommand(this LibCECClient client, int cmd)
        {
            var cmdNames = Enum.GetNames(typeof(CecOpcode));
            var index = cmd - 128;

            if (index < 0 || index > cmdNames.Length)
            {
                Console.WriteLine($"Received Command: {cmd} " +
                                  "Tried to interpret as service command, but couldn\'t. " +
                                  "Value has to be between 128 and 255!");
                return;
            }

            client.Command(cmdNames[index]);
        }

        /// <summary>
        /// Send command via a string representation of a <see cref="CecOpcode"/>.
        /// <para />
        /// Note: Command is Case Insensitive
        /// <para />
        /// <example>
        /// Ex:
        /// "Standby", "TextViewOn", "standby", "textviewon"
        /// </example>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd"></param>
        public static void Command(this LibCECClient client, string cmd)
        {
            string[] strings = Enum.GetNames(typeof(CecOpcode));
            if (!strings.Contains(cmd, StringComparer.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($"{cmd} is not a CEC command!");
                return;
            }

            CecOpcode cecCmd = (CecOpcode)Enum.Parse(typeof(CecOpcode), cmd, true);
            client.Command(cecCmd);
        }

        public static void CommandAsHex(this LibCECClient client, string cmd)
        {
            bool succeded = true;

            if (cmd.StartsWith("0x"))
            {
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

                client.Command(hexParsed);
            }

            if (!succeded)
                Console.WriteLine($"Tried to parse CMD \"{cmd}\" as hex values, but failed.");
        }

        public static void CommandAsHex(this LibCECClient client, int cmd)
        {
            string hex = cmd.ToString("X");

            int insertIndex = 1;
            for (var i = 0; i < hex.Length - insertIndex; i++)
                if (i % 2 == 1)
                {
                    hex = hex.Insert(i + insertIndex, ":");
                    insertIndex++;
                }

            client.CommandAsHex($"0x{hex}");
        }
    }
}
