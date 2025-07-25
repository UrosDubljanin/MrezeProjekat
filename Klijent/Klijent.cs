﻿using System.Net.Sockets;
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
            
            Console.WriteLine("Izvršite prijavu u formatu: PRIJAVA: [ime/nadimak], ([trening],[igre odvojene zarezima (sl, sk, kzz)])");
            Console.Write("PRIJAVA: ");
            string prijava = "PRIJAVA:" + Console.ReadLine();
            byte[] binarnaPrijava = Encoding.UTF8.GetBytes(prijava);

            int brojBajta = udpSocket.SendTo(binarnaPrijava, 0, binarnaPrijava.Length, SocketFlags.None, serverEPudp);

            //TCP
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEPtcp = new IPEndPoint(IPAddress.Loopback, 50002);
            tcpSocket.Connect(serverEPtcp);

            byte[] buffer = new byte[1024];
            string odgovor;

            int brBajta = tcpSocket.Receive(buffer);
            odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(odgovor);

            Console.WriteLine();

            string? spreman = Console.ReadLine();

            tcpSocket.Send(Encoding.UTF8.GetBytes(spreman));
            bool kraj = true;

            brBajta=tcpSocket.Receive(buffer);
            odgovor= Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(odgovor);

            bool krajIgre = true;

            while (kraj)
            {
                byte[] bafer = new byte[1024];
                int bajti;
                try
                {
                    brBajta = tcpSocket.Receive(buffer);
                    string oznakaIgre = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine(oznakaIgre);

                    if (oznakaIgre == "sl")
                    {
                        byte[] baferSl = new byte[1024];
                        bajti = tcpSocket.Receive(baferSl);
                        string pocetnaSlova = Encoding.UTF8.GetString(baferSl, 0, bajti);
                        Console.WriteLine(pocetnaSlova);

                        Console.WriteLine("Unesite sto duzu rijec sastavljenu od ponudjenih slova: ");
                        string rijec = Console.ReadLine();
                        tcpSocket.Send(Encoding.UTF8.GetBytes(rijec));                                                      //SLAGALICA

                        brBajta = tcpSocket.Receive(baferSl);
                        odgovor = Encoding.UTF8.GetString(baferSl, 0, brBajta);
                        Console.WriteLine(odgovor);
                    }
                    else if (oznakaIgre == "sk")
                    {
                        byte[] baferSk = new byte[1024];
                        brBajta = tcpSocket.Receive(baferSk);
                        string skocko = Encoding.UTF8.GetString(baferSk, 0, brBajta);
                        Console.WriteLine(skocko);
                        bool skKraj = true;

                        while (skKraj)                                                                        // SKOCKO
                        {
                            string rijec = Console.ReadLine();
                            tcpSocket.Send(Encoding.UTF8.GetBytes(rijec));

                            bajti = tcpSocket.Receive(baferSk);
                            string znakovi = Encoding.UTF8.GetString(baferSk, 0, bajti);
                            if (znakovi.Contains("4 znaka su"))
                            {
                                string[] poruka = znakovi.Split('|');
                                string odgovorNakonPokusaja = poruka[0] + poruka[3];
                                Console.WriteLine(odgovorNakonPokusaja);
                                Console.WriteLine("Zavrsili ste igru skocko.");
                                skKraj = false;
                            }else if (znakovi.Contains("Niste pogodili."))
                            {
                                Console.WriteLine(znakovi);
                                Console.WriteLine("Zavrsili ste igru skocko.");
                                skKraj = false;
                            }
                            else
                            {
                                string[] poruka = znakovi.Split('|');

                                if (poruka.Length != 1)
                                {
                                    Console.WriteLine(poruka[0]);
                                    int tMesto = int.Parse(poruka[1]);
                                    int pMesto = int.Parse(poruka[2]);
                                    if (tMesto == 0 && pMesto == 0)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("****");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        int brojZnakova = 0;
                                        for (int i = 0; i < tMesto; i++)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.Write('*');
                                            brojZnakova++;
                                            Console.ResetColor();
                                        }
                                        for (int i = 0; i < pMesto; i++)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.Write('*');
                                            brojZnakova++;
                                            Console.ResetColor();
                                        }
                                        while (brojZnakova != 4)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write('*');
                                            brojZnakova++;
                                            Console.ResetColor();
                                        }
                                        Console.WriteLine();
                                    }
                                }
                            }
                        }
                        brBajta = tcpSocket.Receive(baferSk);
                        string krajSkocka = Encoding.UTF8.GetString(baferSk, 0, brBajta);
                        Console.WriteLine(krajSkocka);

                    }
                    else if (oznakaIgre == "kzz")
                    {
                        byte[] baferKzz = new byte[1024];
                        while (krajIgre)
                        {
                            bajti = tcpSocket.Receive(baferKzz);
                            string poruka = Encoding.UTF8.GetString(baferKzz, 0, bajti);

                            if (poruka.Contains("Odgovoreno je na sva pitanja. Kraj igre!"))
                            {
                                krajIgre = false;
                                break;
                            }

                            Console.WriteLine(poruka);
                            Console.WriteLine("Unesite tacan odgovor: ");
                            string izabranOdgovor = Console.ReadLine();
                            tcpSocket.Send(Encoding.UTF8.GetBytes(izabranOdgovor));

                            int bajt = tcpSocket.Receive(baferKzz);
                            string provjeraOdgovora = Encoding.UTF8.GetString(baferKzz, 0, bajt);
                            Console.WriteLine(provjeraOdgovora);
                        }

                        
                    }else
                    {
                        Console.WriteLine("Zavrsili ste igru.");
                        kraj = false;
                    }

                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            byte[] tabelaBufer = new byte[1024];
            int bajtovi;

            try
            {
                bajtovi = tcpSocket.Receive(tabelaBufer);
                string tabela = Encoding.UTF8.GetString(tabelaBufer, 0, bajtovi);
                Console.WriteLine(tabela);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            try
            {
                bajtovi = tcpSocket.Receive(tabelaBufer);
                string porukaZaKraj = Encoding.UTF8.GetString(tabelaBufer, 0, bajtovi);
                Console.Write(porukaZaKraj);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }




            udpSocket.Close();
            tcpSocket.Close();
        }
    }
}



