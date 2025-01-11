using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Klijent
{
    internal class Klijent
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP=new IPEndPoint(IPAddress.Any, 50001);
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Izvrsite prijavu: ");
            string prijava="PRIJAVA: "+Console.ReadLine();
            byte[] binarnaPrijava=Encoding.UTF8.GetBytes(prijava);

            int brojBajta = clientSocket.SendTo(binarnaPrijava, 0, binarnaPrijava.Length, SocketFlags.None, serverEP);

            clientSocket.Connect(serverEP);

            byte[] buffer = new byte[1024];
            string odgovor;

            int brBajta = clientSocket.Receive(buffer);
            string serverMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(serverMessage);

            Console.WriteLine();
            string spreman = Console.ReadLine();
            clientSocket.Send(Encoding.UTF8.GetBytes(spreman));
        }
    }
}
