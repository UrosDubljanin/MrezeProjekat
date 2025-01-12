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
            //Vezano za igru
            int brojacIgraca = 0;
            List<Igrac> igraci = new List<Igrac>();


            //UDP
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            udpSocket.Bind(serverEP);
            Console.WriteLine("Server ceka igrace da bi poceo igru:");
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            //TCP
            Socket tcpSocket=new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 50002);
            tcpSocket.Bind(localEP);
            tcpSocket.Listen();




            while (true) {
                byte[] prijavaBufer= new byte[1024];
                try
                {
                    int brBajta = udpSocket.ReceiveFrom(prijavaBufer, ref posiljaocEP);
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

                    Socket povezanSocket = tcpSocket.Accept();
                    poruka = "Uspesno ste ostvarili vezu sa serverom, igra moze da pocne. Kad budete spremni posaljite poruku [SPREMAN]";
                    byte[] bajt = Encoding.UTF8.GetBytes(poruka);
                    povezanSocket.Send(bajt);

                    brBajta=povezanSocket.Receive(bajt);
                    poruka= Encoding.UTF8.GetString(bajt, 0, brBajta);
                    poruka.ToLower();
                    if (poruka == "spreman")
                    {
                        Console.WriteLine("Igrac je spreman za pocetak igre.");
                    }





                    povezanSocket.Close();
                }
                catch (Exception e)
                {
                    break;
                }
            }




           
            udpSocket.Close();
            tcpSocket.Close();
 
        }
    }
}
