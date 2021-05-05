namespace DataLinkNetwork
{
    public interface IReceiver
    {
        byte[] Receive();

        MiddlewareBuffer AcceptConnect(ISender sender);

        void AcceptDisconnect(ISender sender);
    }
}