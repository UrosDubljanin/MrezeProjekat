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
            Console.WriteLine("Server ceka igrace da bi poceo igru");
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


                    for (int i = 1; i < delici.Length; i++)
                    {
                        if (delici[i] == "sl")
                        {
                            Slagalica slagalica = new Slagalica();
                            slagalica.GenerisiSlova();

                            Console.WriteLine($"Ponudjena slova za igru su: {slagalica.PonudjenaSlova}");

                            string pocetnaSlova = $"{slagalica.PonudjenaSlova}";
                            byte[] bajtovi = Encoding.UTF8.GetBytes(pocetnaSlova);
                            povezanSocket.Send(bajtovi);

                            byte[] buffer = new byte[1024];
                            int primljenoBajtova = povezanSocket.Receive(buffer);
                            string recIgraca = Encoding.UTF8.GetString(buffer, 0, primljenoBajtova);

                            int poeni = slagalica.ProveriRec(recIgraca);
                            string rezultat;

                            if (poeni > 0)
                            {
                                rezultat = $"Rijec koju ste unijeli je validna! Osvojili ste {poeni} poena.";
                            }
                            else
                            {
                                rezultat = "Rijec koju ste unijeli nije validna!";
                            }

                            byte[] rezultatBajtovi = Encoding.UTF8.GetBytes(rezultat);
                            povezanSocket.Send(rezultatBajtovi);

                        }else if (delici[i] == "sk")
                        {
                            Skocko skocko = new Skocko();
                            skocko.GenerisiKombinaciju();

                            Console.WriteLine($"Trazena kombinacija je {skocko.TrazenaKomb}");

                            string pocetnaSlova = $"Unesite kombinaciju sledecih znakova HTPKSZ";
                            byte[] bajtovi = Encoding.UTF8.GetBytes(pocetnaSlova);
                            povezanSocket.Send(bajtovi);

                            int brojac = 1;
                            int brojOsvojenihPoena = 0;
                            while (brojac<6)
                            {
                                byte[] buffer = new byte[1024];
                                int primljenoBajtova = povezanSocket.Receive(buffer);
                                string kombinacijaIgraca = Encoding.UTF8.GetString(buffer, 0, primljenoBajtova);

                                string rezultat = skocko.ProveriKombinaciju(kombinacijaIgraca);

                                if (rezultat == "4 znaka su na pravom mestu, 0 nisu na mestu.") {
                                    if (brojac == 1)
                                    {
                                        brojOsvojenihPoena = 30;
                                    }else if (brojac == 2)
                                    {
                                        brojOsvojenihPoena = 25;
                                    }else if(brojac == 3)
                                    {
                                        brojOsvojenihPoena = 20;
                                    }else if (brojac == 4)
                                    {
                                        brojOsvojenihPoena = 15;
                                    }
                                    else
                                    {
                                        brojOsvojenihPoena = 10;
                                    }
                                    byte[] nizz = Encoding.UTF8.GetBytes(rezultat+$"Osvojili ste {brojOsvojenihPoena} poena!");
                                    povezanSocket.Send(nizz);
                                    break;
                                }

                                byte[] niz = Encoding.UTF8.GetBytes(rezultat);
                                povezanSocket.Send(niz);

                                brojac++;
                            }
                        }
                        else
                        {
                            KoZnaZna koznazna = new KoZnaZna();
                            koznazna.UcitavanjePitanja();

                            int brojPoena = 0;
                            foreach (var pitanje in koznazna.SvaPitanja)
                            {
                                if (koznazna.OpcijePitanja.TryGetValue(pitanje.Key, out var opcije))
                                {
                                    string pitanjeZaKlijenta = $"{pitanje.Key}\n1 - {opcije.Item1}\n2 - {opcije.Item2}\n3 - {opcije.Item3}";
                                    byte[] pitanjeBajtovi = Encoding.UTF8.GetBytes(pitanjeZaKlijenta);
                                    povezanSocket.Send(pitanjeBajtovi);

                                    byte[] buffer = new byte[1024];
                                    int primljenoBajtova = povezanSocket.Receive(buffer);
                                    string odgovorKlijentaStr = Encoding.UTF8.GetString(buffer, 0, primljenoBajtova);

                                    if (int.TryParse(odgovorKlijentaStr, out int odgovorKlijenta))
                                    {
                                        string rezultat = koznazna.Provjera(pitanje.Key, odgovorKlijenta);
                                        if (rezultat.Contains("Tacan"))
                                        {
                                            brojPoena += 10;
                                        }
                                        else
                                        {
                                            brojPoena -= 5;
                                        }
                                        byte[] rezultatBajtovi = Encoding.UTF8.GetBytes(rezultat+$"Osvojili ste {brojPoena} poena");
                                        povezanSocket.Send(rezultatBajtovi); 

                                    }
                                    else
                                    {
                                        string greska = "Netacan unos. Očekuje se broj 1, 2 ili 3.";
                                        byte[] greskaBajtovi = Encoding.UTF8.GetBytes(greska);
                                        povezanSocket.Send(greskaBajtovi);  
                                    }
                                }
                                else
                                {
                                    string greska = "Pitanje nema definisane opcije.";
                                    byte[] greskaBajtovi = Encoding.UTF8.GetBytes(greska);
                                    povezanSocket.Send(greskaBajtovi);
                                }
                            }
                            string krajPoruka = "Odgovoreno je na sva pitanja. Kraj igre!";
                            byte[] krajBajtovi = Encoding.UTF8.GetBytes(krajPoruka);
                            povezanSocket.Send(krajBajtovi);

                        }
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
