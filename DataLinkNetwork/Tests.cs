using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using DataLinkNetwork.Abstractions;
using DataLinkNetwork.BitArrayRoutine;
using DataLinkNetwork.Checksum;
using DataLinkNetwork.Communication;

namespace DataLinkNetwork
{
    public class Tests
    {
        public static void Test1()
        {
            BitArray testData = new BitArray(64);

            IChecksumBuilder builder = new VerticalOddityChecksumBuilder();

            var checksum = builder.Build(testData);

            Console.WriteLine(checksum.ToBinString());
        }

        public static void Test2()
        {
            BitArray testData = new BitArray(64);
            Random random = new Random(DateTime.Now.Millisecond);

            for (var i = 0; i < testData.Count; i++)
            {
                testData[i] = random.Next(0, 100) >= 50;
            }

            Frame frame = new Frame(testData, new BitArray(C.AddressSize), new BitArray(C.ControlSize));
            var rawFrameBits = frame.Build();

            Console.WriteLine(frame.ToString());
            Console.WriteLine(rawFrameBits.ToBinString());
        }

        public static void Test3()
        {
            BitArray testData = new BitArray(5);
            Random random = new Random(DateTime.Now.Millisecond);

            for (var i = 0; i < testData.Count; i++)
            {
                testData[i] = random.Next(0, 100) >= 50;
            }

            Frame frame = new Frame(testData, new BitArray(C.AddressSize), new BitArray(C.ControlSize));
            var rawFrameBits = frame.Build();

            var parsedHdlcFrame = Frame.Parse(rawFrameBits);

            Console.WriteLine(frame.ToString());
            Console.WriteLine(parsedHdlcFrame.ToString());
        }

        public static void Test4()
        {
            ISender sender = new Sender();
            IReceiver receiver = new Receiver();

            sender.Connect(receiver);

            Task.Run(() =>
            {
                sender.Send(
                    Encoding.UTF8.GetBytes(
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                    )
                );
                sender.Disconnect(receiver);
            });
            Task.Run(() =>
            {
                var receivedBytes = receiver.Receive();

                var receive = Encoding.UTF8.GetString(receivedBytes);

                Console.WriteLine(receive);
            });

            Console.ReadKey();
        }
    }
}