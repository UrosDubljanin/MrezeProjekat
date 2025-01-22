using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

namespace Klijent
{
    internal class Klijent
    {
        static void Main(string[] args)
        {
            //UDP
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEPudp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50001);
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);


            //TCP
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEPtcp = new IPEndPoint(IPAddress.Loopback, 50002);
            tcpSocket.Connect(serverEPtcp);



            Console.WriteLine("Izvršite prijavu u formatu: PRIJAVA: [ime/nadimak], [igre odvojene zarezima (sl, sk, kzz)]");
            Console.Write("PRIJAVA: ");
            string prijava = "PRIJAVA:" + Console.ReadLine();
            byte[] binarnaPrijava = Encoding.UTF8.GetBytes(prijava);

            int brojBajta = udpSocket.SendTo(binarnaPrijava, 0, binarnaPrijava.Length, SocketFlags.None, serverEPudp);

            byte[] buffer = new byte[1024];
            string odgovor;

            int brBajta = tcpSocket.Receive(buffer);
            odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(odgovor);

            Console.WriteLine();

            string? spreman = Console.ReadLine();

            tcpSocket.Send(Encoding.UTF8.GetBytes(spreman));
            bool kraj = true;


            while (kraj)
            {
                byte[] bafer = new byte[1024];
                int bajti;
                try
                {
                    brBajta = tcpSocket.Receive(buffer);
                    string oznakaIgre = Encoding.UTF8.GetString(buffer, 0, brBajta);

                    if (oznakaIgre == "sl")
                    {
                        bajti = tcpSocket.Receive(bafer);
                        string pocetnaSlova = Encoding.UTF8.GetString(bafer, 0, bajti);
                        Console.WriteLine(pocetnaSlova);

                        Console.WriteLine("Unesite sto duzu rijec sastavljenu od ponudjenih slova: ");
                        string rijec = Console.ReadLine();
                        tcpSocket.Send(Encoding.UTF8.GetBytes(rijec));                                                      //SLAGALICA

                        brBajta = tcpSocket.Receive(buffer);
                        odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        Console.WriteLine(odgovor);
                    }
                    else if (oznakaIgre == "sk")
                    {
                        brBajta = tcpSocket.Receive(bafer);
                        string skocko = Encoding.UTF8.GetString(bafer, 0, brBajta);
                        Console.WriteLine(skocko);

                        while (true)
                        {
                            Console.WriteLine("Unesite kombinaciju");
                            string rijec = Console.ReadLine();
                            tcpSocket.Send(Encoding.UTF8.GetBytes(rijec));

                            bajti = tcpSocket.Receive(bafer);
                            string znakovi = Encoding.UTF8.GetString(bafer, 0, bajti);                     // SKOCKO
                            Console.WriteLine(znakovi);

                            if (znakovi.Contains("4 znaka su"))
                            {
                                Console.WriteLine("Zavrsili ste igru skocko.");
                                break;
                            }
                        }
                    }
                    else if (oznakaIgre == "kzz")
                    {
                        while (true)
                        {
                            bajti = tcpSocket.Receive(bafer);
                            string pitanje = Encoding.UTF8.GetString(bafer, 0, bajti);
                            Console.WriteLine(pitanje);

                            if (pitanje == "Odgovoreno je na sva pitanja. Kraj igre!")
                            {
                                break;
                            }

                            Console.WriteLine("Unesite tacan odgovor: ");
                            string izabranOdgovor = Console.ReadLine();
                            tcpSocket.Send(Encoding.UTF8.GetBytes(izabranOdgovor));                      //KO ZNA ZNA

                            int bajt = tcpSocket.Receive(bafer);
                            string provjeraOdgovora = Encoding.UTF8.GetString(bafer, 0, bajt);
                            Console.WriteLine(provjeraOdgovora);
                        }
                    }
                    else if (oznakaIgre == "kraj")
                    {
                        kraj = false;
                        Console.WriteLine("Zavrsili ste igru.");
                    }

                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }


            udpSocket.Close();
            tcpSocket.Close();
        }
    }
}



