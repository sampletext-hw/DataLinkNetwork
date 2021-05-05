using DataLinkNetwork.Communication;

namespace DataLinkNetwork.Abstractions
{
    public interface IReceiver
    {
        byte[] Receive();

        MiddlewareBuffer AcceptConnect(ISender sender);

        void AcceptDisconnect(ISender sender);
    }
}