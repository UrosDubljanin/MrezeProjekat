using System.Net;
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


                }catch(Exception e)
                {
                    break;
                }
            }





            serverSocket.Close();
 
        }
    }
}
