using System.IO.Ports;

namespace NextionTftUploader {
    /// <summary>
    /// Convenient Nextion specific SerialPort extensions for reading and writing data
    /// </summary>
    public static class NextionSerialPortExtensions {

        public static void NextionWrite(this SerialPort sp, string text) {
            sp.Write(text);
            sp.Write(new byte[] { 0xff, 0xff, 0xff }, 0, 3);
        }

        public static string NextionReadConnectResponse(this SerialPort sp) {
            return sp.ReadTo(new string(new char[] { '\xff', '\xff', '\xff' }));
        }

    }
}
