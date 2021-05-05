using System;
using System.Text;
using System.Threading.Tasks;
using DataLinkNetwork.Abstractions;
using DataLinkNetwork.Communication;

namespace DataLinkNetwork
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            ISender sender = new Sender();
            IReceiver receiver = new Receiver();

            sender.Connect(receiver);

            await Task.Run(() =>
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
            await Task.Run(() =>
            {
                var receivedBytes = receiver.Receive();

                var receive = Encoding.UTF8.GetString(receivedBytes);

                Console.WriteLine(receive);
            });

            Console.ReadKey();
        }
    }
}