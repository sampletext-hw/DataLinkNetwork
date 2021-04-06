using System;
using System.Collections;

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

            HdlcFrame frame = new HdlcFrame(testData, new BitArray(Constants.AddressSize), new BitArray(Constants.ControlSize));
            var rawFrameBits = frame.Build();

            Console.WriteLine(frame.ToString());
            Console.WriteLine(rawFrameBits.ToBinString());
        }
        
        public static void Test3()
        {
            BitArray testData = new BitArray(64);
            Random random = new Random(DateTime.Now.Millisecond);
            
            for (var i = 0; i < testData.Count; i++)
            {
                testData[i] = random.Next(0, 100) >= 50;
            }

            HdlcFrame frame = new HdlcFrame(testData, new BitArray(Constants.AddressSize), new BitArray(Constants.ControlSize));
            var rawFrameBits = frame.Build();

            var parsedHdlcFrame = HdlcFrame.Parse(rawFrameBits);

            Console.WriteLine(frame.ToString());
            Console.WriteLine(parsedHdlcFrame.ToString());
        }
    }
}