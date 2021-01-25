using System;

namespace NextionTftUploader {

    class Program {

        static void Main(string[] args) {

            if (args.Length < 3) {
                Console.WriteLine("Too few arguments!");
                Console.WriteLine("Expecting: {TFT file} {COM port} {Baud rate}");
                return;
            }

            var u = new NextionUploader();

            var file = args[0];
            var port = args[1];
            var baudRate = int.Parse(args[2]);

            u.Upload(file, port, baudRate);
        }

    }

}
