using System;
using System.Collections;

namespace DataLinkNetwork
{
    public class HdlcFrame
    {
        public static readonly BitArray Flag = new(new[] {false, true, true, true, true, true, true, false});

        public BitArray Data { get; set; }

        public BitArray Address { get; set; }

        public BitArray Control { get; set; }

        public BitArray Checksum { get; set; }

        public HdlcFrame(BitArray data, BitArray address, BitArray control)
        {
            if (data.Length > Constants.MaxFrameDataSize)
            {
                throw new ArgumentException(
                    $"Data length can't exceed {Constants.MaxFrameDataSize}, actual {data.Length}");
            }

            if (address.Length > Constants.AddressSize)
            {
                throw new ArgumentException(
                    $"Address length can't exceed {Constants.AddressSize}, actual {address.Length}");
            }

            if (control.Length > Constants.ControlSize)
            {
                throw new ArgumentException(
                    $"Control length can't exceed {Constants.ControlSize}, actual {control.Length}");
            }

            Data = data;
            Address = address;
            Control = control;
        }

        public BitArray Build()
        {
            Checksum = new VerticalOddityChecksumBuilder().Build(Data);

            int frameSize =
                Constants.FlagSize +
                Constants.AddressSize +
                Constants.ControlSize +
                Data.Length +
                Constants.ChecksumSize +
                Constants.FlagSize;

            BitArray frameArray = new BitArray(frameSize);

            var writer = new BitArrayWriter(frameArray);

            writer.Write(Flag);
            writer.Write(Address);
            writer.Write(Control);
            writer.Write(Data);
            writer.Write(Checksum);
            writer.Write(Flag);

            return frameArray;
        }

        public static HdlcFrame Parse(BitArray rawBits)
        {
            var startFlagPosition = rawBits.FindFlag();
            if (startFlagPosition == -1)
            {
                throw new ArgumentException($"{nameof(rawBits)} doesn't contain start Flag");
            }

            int minimumNextFlag = startFlagPosition + Constants.FlagSize;

            var nextFlagPosition = rawBits.FindFlag(minimumNextFlag);

            if (nextFlagPosition == -1)
            {
                throw new ArgumentException($"{nameof(rawBits)} doesn't contain second Flag");
            }

            BitArrayReader reader = new BitArrayReader(rawBits, startFlagPosition);

            var flagBits = reader.Read(Constants.FlagSize);
            var addressBits = reader.Read(Constants.AddressSize);
            var controlBits = reader.Read(Constants.ControlSize);
            var currentReaderPosition = reader.Position;

            reader.Adjust(nextFlagPosition - currentReaderPosition - Constants.ChecksumSize);
            var checksumStartPosition = reader.Position;
            var checksumBits = reader.Read(Constants.ChecksumSize);
            reader.Adjust(-Constants.ChecksumSize - checksumStartPosition + currentReaderPosition);
            var dataBits = reader.Read(checksumStartPosition - currentReaderPosition);

            var checksum = new VerticalOddityChecksumBuilder().Build(dataBits);
            if (checksum.IsSameNoCopy(checksumBits, 0, 0, Constants.ChecksumSize))
            {
                Console.WriteLine("HdlcFrame checksum matched");
            }
            else
            {
                Console.WriteLine("HdlcFrame checksum mismatched");
            }

            return new HdlcFrame(dataBits, addressBits, controlBits) {Checksum = checksum};
        }

        public override string ToString()
        {
            return
                $"HdlcFrame {{\n  {nameof(Data)}: {Data.ToBinString()},\n  {nameof(Address)}: {Address.ToBinString()},\n  {nameof(Control)}: {Control.ToBinString()},\n  {nameof(Checksum)}: {Checksum.ToBinString()}\n}}";
        }
    }
}