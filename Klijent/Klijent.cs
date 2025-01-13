using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Klijent
{
    internal class Klijent
    {
        static void Main(string[] args)
        {
            //UDP
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEPudp=new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50001);
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);


            //TCP
            Socket tcpSocket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPEndPoint serverEPtcp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50002);
            tcpSocket.Connect(serverEPtcp);



            Console.WriteLine("Izvršite prijavu u formatu: PRIJAVA: [ime/nadimak], [igre odvojene zarezima (sl, sk, kzz)]");
            Console.Write("PRIJAVA: ");
            string prijava="PRIJAVA:"+ Console.ReadLine();
            byte[] binarnaPrijava=Encoding.UTF8.GetBytes(prijava);

            int brojBajta = udpSocket.SendTo(binarnaPrijava, 0, binarnaPrijava.Length, SocketFlags.None, serverEPudp);

            byte[] buffer = new byte[1024];
            string odgovor;

            int brBajta = tcpSocket.Receive(buffer);
            odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(odgovor);

            Console.WriteLine();
            string? spreman = Console.ReadLine();
            tcpSocket.Send(Encoding.UTF8.GetBytes(spreman));

            udpSocket.Close();
            tcpSocket.Close();
        }
    }
}
