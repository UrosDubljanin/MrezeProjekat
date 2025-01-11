using System.Net;
using System.Net.Sockets;
using Klase;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            List<Igrac> igraci = new List<Igrac>();
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            serverSocket.Bind(serverEP);
            Console.WriteLine("Server ceka igrace da bi poceo igru:");

            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            serverSocket.Close();
 
        }
    }
}
