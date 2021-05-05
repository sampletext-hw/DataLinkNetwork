using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using DataLinkNetwork.Abstractions;
using DataLinkNetwork.BitArrayRoutine;

namespace DataLinkNetwork.Communication
{
    public class Sender : ISender
    {
        private Stopwatch _stopwatch;

        public const long SendTimeoutMilliseconds = 5000;

        private bool _connected;

        private MiddlewareBuffer _middlewareBuffer;

        private bool InternalSend(BitArray bitArray, int tried, int index)
        {
            if (C.SLog) Console.WriteLine("Sender: Acquiring Middleware");
            _middlewareBuffer.Acquire();
            if (C.SLog) Console.WriteLine("Sender: Acquired Middleware");
            _middlewareBuffer.Push(bitArray);
            if (C.SLog) Console.WriteLine($"Sender: Sent {bitArray.Length} bits");
            if (C.SLog) Console.WriteLine($"Sender {index}: {bitArray.ToBinString()}");
            _middlewareBuffer.Release();

            _stopwatch = Stopwatch.StartNew();

            int lastReceiveStatus = 0;

            while (lastReceiveStatus == 0)
            {
                lastReceiveStatus = _middlewareBuffer.GetStatusCode();
                if (lastReceiveStatus == 0)
                {
                    if (_stopwatch.ElapsedMilliseconds > SendTimeoutMilliseconds)
                    {
                        _stopwatch.Stop();
                        if (C.SLog) Console.WriteLine("Sender: Timeout expired, while confirming the send");
                        lastReceiveStatus = -1;
                        break;
                    }
                    Thread.Sleep(10);
                }
                else
                {
                    if (C.SLog) Console.WriteLine("Sender: Received status");
                }
            }

            if (lastReceiveStatus == 1)
            {
                // Everything is OK
                if (C.SLog) Console.WriteLine("Sender: Receive Confirmed");
                _middlewareBuffer.ResetStatus();
                return true;
            }
            else if (lastReceiveStatus == -1)
            {
                _middlewareBuffer.ResetStatus();
                if (tried == 3)
                {
                    if (C.SLog) Console.WriteLine($"Sender: Send failed for {tried} times. Abort!");
                    return false;
                }

                if (C.SLog) Console.WriteLine("Sender: Receive Denied, Sending Again");
                bool result = InternalSend(bitArray, tried + 1, index);
                return result;
            }

            return false;
        }

        public void Send(byte[] array)
        {
            BitArray data = new BitArray(array).BitStaff();

            var arrays = data.Split(C.MaxFrameDataSize);

            for (var index = 0; index < arrays.Count; index++)
            {
                var dataBits = arrays[index];
                BitArray addressBits = new BitArray(C.AddressSize);
                BitArray controlBits = new BitArray(C.ControlSize);
                BitArrayWriter writer = new BitArrayWriter(controlBits);
                writer.Write(new BitArray(new[] {(byte)(index & 0xFF)}));

                Frame frame = new Frame(dataBits, addressBits, controlBits);

                var bitArray = frame.Build();

                if (C.SLog) Console.WriteLine($"Sender: sending {index} frame");
                
                bool result = InternalSend(bitArray, 0, index);
                if (!result)
                {
                    if (C.SLog) Console.WriteLine("Sender: Sending was aborted!");
                    return;
                }
            }
        }

        public void Connect(IReceiver receiver)
        {
            if (C.SLog) Console.WriteLine("Sender Connecting");

            if (!_connected)
            {
                _middlewareBuffer = receiver.AcceptConnect(this);
                _connected = true;
                if (C.SLog) Console.WriteLine("Sender Connected");
            }
            else
            {
                if (C.SLog) Console.WriteLine("Sender already connected");
            }
        }

        public void Disconnect(IReceiver receiver)
        {
            if (C.SLog) Console.WriteLine("Sender Disconnecting");

            if (_connected)
            {
                receiver.AcceptDisconnect(this);
                _connected = false;
                if (C.SLog) Console.WriteLine("Sender Disconnected");
            }
            else
            {
                if (C.SLog) Console.WriteLine("Sender already disconnected");
            }
        }
    }
}