using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataLinkNetwork.Abstractions;
using DataLinkNetwork.BitArrayRoutine;
using DataLinkNetwork.Checksum;

namespace DataLinkNetwork.Communication
{
    public class Receiver : IReceiver
    {
        private MiddlewareBuffer _middlewareBuffer;

        private bool _connected;

        private Mutex _connectedMutex = new Mutex();

        private BitArray InternalReceive()
        {
            if (C.RLog) Console.WriteLine("Receiver: Acquiring Middleware");
            _middlewareBuffer.Acquire();
            var bitArray = _middlewareBuffer.Get();
            _middlewareBuffer.Release();
            if (C.RLog) Console.WriteLine("Receiver: Released Middleware");

            return bitArray;
        }

        public byte[] Receive()
        {
            _connectedMutex.WaitOne();
            Dictionary<int, byte[]> framedBytes = new();

            int lastReceived = -1;

            while (_middlewareBuffer.HasAvailable())
            {
                var bitArray = InternalReceive();

                var hdlcFrame = Frame.Parse(bitArray);
                BitArrayReader controlReader = new BitArrayReader(hdlcFrame.Control);
                byte frameId = controlReader.Read(8).ToByteArray()[0];

                if (frameId <= lastReceived)
                {
                    if (C.RLog) Console.WriteLine("Receiver: received already processed frame");
                    _middlewareBuffer.SetStatusCode(1);
                }
                else
                {
                    lastReceived = frameId;

                    var checksum = new VerticalOddityChecksumBuilder().Build(hdlcFrame.Data);
                    Console.WriteLine($"Receiv {frameId}: {bitArray.ToBinString()}");
                    if (hdlcFrame.Checksum.IsSameNoCopy(checksum, 0, 0, C.ChecksumSize))
                    {
                        _middlewareBuffer.SetStatusCode(1);
                        framedBytes.Add(frameId, hdlcFrame.Data.DeBitStaff().ToByteArray());
                        if (C.RLog) Console.WriteLine($"Receiver: received {bitArray.Length} bits");
                        if (C.RLog) Console.WriteLine($"Receiv {frameId}: {bitArray.ToBinString()}");
                    }
                    else
                    {
                        _middlewareBuffer.SetStatusCode(-1);
                        if (C.RLog) Console.WriteLine("Receiver: Failed Checksum Check");

                        if (C.RLog) Console.WriteLine($"Was: {hdlcFrame.Checksum.ToBinString()}\n" + $"Rec: {checksum.ToBinString()}");
                    }
                }
            }

            if (C.RLog) Console.WriteLine("Receiver: Waiting");
            Thread.Sleep(100);

            var total = framedBytes.OrderBy(f => f.Key).SelectMany(b => b.Value).ToList();
            if (_middlewareBuffer.HasAvailable())
            {
                if (C.RLog) Console.WriteLine("Receiver: There is some data to receive");
                total.AddRange(Receive());
            }

            _connectedMutex.ReleaseMutex();
            return total.ToArray();
        }

        public MiddlewareBuffer AcceptConnect(ISender sender)
        {
            if (C.RLog) Console.WriteLine("Receiver Accepting");

            if (!_connected)
            {
                _connectedMutex.WaitOne();
                _middlewareBuffer = new MiddlewareBuffer(sender, this);
                _connected = true;
                _connectedMutex.ReleaseMutex();
                if (C.RLog) Console.WriteLine("Receiver Accepted");
                return _middlewareBuffer;
            }
            else
            {
                if (C.RLog) Console.WriteLine("Receiver already connected");
                return null;
            }
        }

        public void AcceptDisconnect(ISender sender)
        {
            if (C.RLog) Console.WriteLine("Receiver Disconnecting");
            if (_connected)
            {
                _connectedMutex.WaitOne();
                _middlewareBuffer = null;
                _connected = false;
                _connectedMutex.ReleaseMutex();

                if (C.RLog) Console.WriteLine("Receiver Disconnected");
            }
            else
            {
                if (C.RLog) Console.WriteLine("Receiver already Disconnected");
            }
        }
    }
}