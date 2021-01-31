using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace NextionTftUploader {
    public class NextionUploader {


        /// <summary>
        /// Uploads a TFT file to a Nextion device using the NEXTION HMI UPLOAD PROTOCOL (v1.0)
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="port"></param>
        /// <param name="baudRate"></param>
        public void Upload(string fileName, string port, int baudRate) {
            var sp = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);

            // This is very important when using any SerialPort methods that make use of string or char types
            // Byte values will be truncated to byte values allowed in codepage
            // \xff becomes \x3f for ASCII which is the default encoding
            // https://social.msdn.microsoft.com/Forums/en-US/efe127eb-b84b-4ae5-bd7c-a0283132f585/serial-port-sending-8-bit-problem?forum=Vsexpressvb
            sp.Encoding = Encoding.GetEncoding(28591);

            sp.Open();

            // NEXTION HMI UPLOAD PROTOCOL (v1.0) Reference:
            // https://nextion.tech/2017/09/15/nextion-hmi-upload-protocol/

            Console.WriteLine($"Connecting to device on serial port: {port}, Baud rate: {baudRate}...");

            // The reference says: "It is suggested to send an empty instruction before the connect instruction."
            sp.NextionWrite("");
            sp.NextionWrite("connect");

            // Read connect handshake
            var comok = sp.NextionReadConnectResponse();

            // Example response: comok 1,101,NX4024T032_011R,52,61488,D264B8204F0E1828,16777216
            var parts = comok.Substring(6).Split(",");
            if (parts.Length >= 7) {
                Console.WriteLine("Successfully connected!");
                Console.WriteLine();
                Console.WriteLine("DEVICE INFO");
                Console.WriteLine("============================================");
                Console.WriteLine(parts[0] == "1" ? "Nextion model with touch panel" : "Nextion model without touch panel");
                Console.WriteLine($"Model: {parts[2]}");
                Console.WriteLine($"Firmware version: {parts[3]}");
                Console.WriteLine($"MCU code: {parts[4]}");
                Console.WriteLine($"Device Serial Number: {parts[5]}");
                Console.WriteLine($"Flash size: {parts[6]} bytes");
                Console.WriteLine("============================================");
                Console.WriteLine();
            }


            Console.WriteLine($"Reading TFT file: {fileName}...");
            var data = File.ReadAllBytes(fileName);
            Console.WriteLine($"TFT file size is: {data.Length} bytes");

            Console.WriteLine($"Sending upload command to device...");
            sp.NextionWrite($"whmi-wri {data.Length},{baudRate},0");

            // Wait for ready byte
            Console.WriteLine("Waiting for device...");
            var s = sp.ReadByte();

            if (s != 0x05) {
                Console.WriteLine("Device not ready! Exiting...");
                sp.Close();
                return;
            }

            var chunkSize = 4096;

            // Send TFT data
            for (int offset = 0; offset <= data.Length; offset += chunkSize) {
                var length = ((data.Length - offset) >= chunkSize) ? chunkSize : data.Length - offset;

                Console.Write($"Writing {length} bytes, {offset} bytes written ...                      ");
                Console.CursorLeft = 0;
                sp.Write(data, offset, length);

                // The reference for NEXTION HMI UPLOAD PROTOCOL (v1.0) doesn't
                // specifically mention it, but we need to wait for 0x05 return byte before
                // sending the next chunk. This IS mentioned in the v1.1 reference.
                var np = sp.ReadByte();

                if (np != 0x05) {
                    Console.WriteLine("Device not ready for more data! Exiting...");
                    sp.Close();
                    return;
                }
            }

            Console.WriteLine();
            Console.WriteLine("TFT data written successfully! Device will restart...");
            Console.WriteLine();
            sp.Close();
        }

    }
}
