namespace DataLinkNetwork.Abstractions
{
    public interface ISender
    {
        void Send(byte[] array);

        void Connect(IReceiver receiver);

        void Disconnect(IReceiver receiver);
    }
}