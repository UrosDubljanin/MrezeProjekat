using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Klase;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            int brojacIgraca = 0;
            List<Igrac> igraci = new List<Igrac>();
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            serverSocket.Bind(serverEP);
            Console.WriteLine("Server ceka igrace da bi poceo igru:");

            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                byte[] prijavaBufer= new byte[1024];
                try
                {
                    int brBajta = serverSocket.ReceiveFrom(prijavaBufer, ref posiljaocEP);
                    string poruka = Encoding.UTF8.GetString(prijavaBufer, 0, brBajta);
                    string[] delovi = poruka.Split(":");
                    string[] delici = delovi[1].Split(",");

                    bool greskaPrijava = false;
                    int brojIgri = 0;
                    for(int i = 1; i < delici.Length; i++)
                    {
                        if (delici[i]!="sl" && delici[i]!="sk" && delici[i] != "kzz")
                        {
                            Console.WriteLine("Greska prilikom prijave igraca");
                            greskaPrijava = true;
                            break;
                        }
                        brojIgri++;
                    }
                    if (greskaPrijava)
                    {
                        break;
                    }

                    brojacIgraca++;
                    Igrac igrac = new Igrac(brojacIgraca, delici[0],brojIgri);
                    igraci.Add(igrac);

                    Console.WriteLine($"Igrac {igrac.KorisnickoIme} se uspesno prijavio i hoce da igra {brojIgri} igri.");

                    ///////////////////////////////////////////////////////////////////////////

                    Socket acceptedSocket = serverSocket.Accept();
                    // Dobijanje udaljene (remote) IP adrese i porta
                    IPEndPoint remoteEndPoint = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    if (remoteEndPoint != null)
                    {
                        Console.WriteLine("Peer's IP address is: {0}", remoteEndPoint.Address.ToString());
                        Console.WriteLine("Peer's port is: {0}", remoteEndPoint.Port);
                    }
                    else
                    {
                        Console.WriteLine("Could not retrieve remote end point information.");
                    }
                    
                    //Primer za dobijanje lokalne adrese:
                    // Dobijanje lokalne (local) IP adrese i porta
                    IPEndPoint localEndPoint = acceptedSocket.LocalEndPoint as IPEndPoint;
                    if (localEndPoint != null)
                    {
                        Console.WriteLine("Local IP address is: {0}", localEndPoint.Address.ToString());
                        Console.WriteLine("Local port is: {0}", localEndPoint.Port);
                    }
                    else
                    {
                        Console.WriteLine("Could not retrieve local end point information.");
                    }
                   
                    /////////////////////////////////////////////////////////////////////////////////////
                }
                catch (Exception e)
                {
                    break;
                }
            }





            serverSocket.Close();
 
        }
    }
}
