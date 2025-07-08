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
            bool krajIgre = true;

            List<Socket> sviKlijenti = new List<Socket>();
            Dictionary<Socket, double> KonacniRezultat = new Dictionary<Socket, double>();

            //UDP
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            udpSocket.Bind(serverEP);
            Console.WriteLine("Server ceka igrace da bi poceo igru");
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(new IPEndPoint(IPAddress.Any, 50002));
            tcpSocket.Listen(10);


            while (krajIgre)
            {
                byte[] prijavaBufer = new byte[1024];
                try
                {
                    int brBajta = udpSocket.ReceiveFrom(prijavaBufer, ref posiljaocEP);
                    string poruka = Encoding.UTF8.GetString(prijavaBufer, 0, brBajta);
                    string[] delovi = poruka.Split(":");
                    string[] delici = delovi[1].Split(",");

                    bool greskaPrijava = false;
                    int brojIgri = 0;
                    for (int i = 1; i < delici.Length; i++)
                    {
                        if (delici[i] != "sl" && delici[i] != "sk" && delici[i] != "kzz")
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

                    Console.WriteLine("Povežite sve klijente, pa pritisnite ENTER...");

                    while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Enter)
                    {
                        if (tcpSocket.Poll(1500 * 1000, SelectMode.SelectRead))
                        {
                            Socket povezanSocket = tcpSocket.Accept();
                            povezanSocket.Blocking = false;
                            sviKlijenti.Add(povezanSocket);
                            Console.WriteLine("Novi klijent je povezan");
                        }
                        System.Threading.Thread.Sleep(100);
                    }

                    List<Socket> readSockets = new List<Socket>(sviKlijenti);

                    Socket.Select(readSockets, null, null, 1000000);

                    foreach (Socket s in readSockets)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = 0;

                        try
                        {
                            bytesRead = s.Receive(buffer);
                        }
                        catch (SocketException)
                        {
                            Console.WriteLine("Klijent je prekinuo vezu");
                            s.Close();
                            sviKlijenti.Remove(s);
                            continue;
                        }

                        if (bytesRead == 0)
                        {
                            Console.WriteLine("Klijent je zatvorio vezu");
                            s.Close();
                            sviKlijenti.Remove(s);
                            continue;
                        }


                    }

                    Dictionary<Socket, bool> spremniKlijenti = new Dictionary<Socket, bool>();

                    foreach (var sc in sviKlijenti)
                    {
                        spremniKlijenti[sc] = false;
                        string p = "Uspesno ste ostvarili vezu sa serverom. Kad budete spremni posaljite poruku [SPREMAN]";
                        byte[] bajtovi = Encoding.UTF8.GetBytes(p);
                        sc.Send(bajtovi);
                    }

                    while (spremniKlijenti.Values.Any(spreman => !spreman))
                    {
                        List<Socket> readList = new List<Socket>(sviKlijenti);
                        Socket.Select(readList, null, null, 1000000);

                        foreach (Socket sct in readList)
                        {
                            byte[] buf = new byte[1024];
                            int bytesR = sct.Receive(buf);
                            string odgovor = Encoding.UTF8.GetString(buf, 0, bytesR).Trim().ToLower();

                            if (odgovor == "spreman")
                            {
                                spremniKlijenti[sct] = true;
                                Console.WriteLine($"Klijent je spreman.");
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                    }

                    Console.WriteLine("Svi klijenti su spremni, možemo nastaviti.");

                    foreach (var klijent in sviKlijenti)
                    {
                        KonacniRezultat[klijent] = 0; 
                    }




                    //IGRE
                    for (int i = 1; i < delici.Length; i++)
                    {
                        string igra = delici[i];

                        foreach (var soket in sviKlijenti)
                        {
                            soket.Send(Encoding.UTF8.GetBytes(igra));
                        }

                        if (igra == "sl")
                        {
                            Slagalica slagalica = new Slagalica();
                            slagalica.GenerisiSlova();

                            Console.WriteLine($"Ponudjena slova za igru su: {slagalica.PonudjenaSlova}");

                            string pocetnaSlova = $"{slagalica.PonudjenaSlova}";
                            byte[] bajtovi = Encoding.UTF8.GetBytes(pocetnaSlova);



                            foreach (var soket in sviKlijenti)
                            {
                                soket.Send(bajtovi);
                            }

                            //Pravimo recnik ciji kljuc predstavlja trenutni klijent, a vrijednost sta je klijent odgovorio i kada 
                            Dictionary<Socket, (string rijec, DateTime vrijeme)> odgovori = new Dictionary<Socket, (string, DateTime)>();

                            DateTime startVreme = DateTime.Now;          

                            while (odgovori.Count < sviKlijenti.Count)  // Sve dok svi klijenti ne posalju rijec
                            {
                                List<Socket> spremniZaCitanje = new List<Socket>(sviKlijenti);
                                Socket.Select(spremniZaCitanje, null, null, 1000000);

                                foreach (Socket klijent in spremniZaCitanje)
                                {
                                    if (odgovori.ContainsKey(klijent))
                                        continue;                       // Vec smo dobili odgovor od ovog klijenta

                                    byte[] buffer = new byte[1024];
                                    int primljeno = klijent.Receive(buffer);
                                    string odgovor = Encoding.UTF8.GetString(buffer, 0, primljeno).Trim();

                                    DateTime vremeOdgovora = DateTime.Now;
                                    odgovori[klijent] = (odgovor, vremeOdgovora);
                                }

                                Thread.Sleep(50);
                            }

                            var sortiraniOdgovori = odgovori
                                .Where(o => slagalica.ProveriRec(o.Value.rijec) > 0)
                                .OrderBy(o => o.Value.vrijeme)
                                .ToList();

                            Socket? najbrzi = sortiraniOdgovori.FirstOrDefault().Key;

                            foreach (var klijent in sviKlijenti)
                            {
                                string rezultat;

                                if (!odgovori.ContainsKey(klijent))
                                {
                                    rezultat = "Niste uneli nijednu reč.";
                                }
                                else
                                {
                                    string rec = odgovori[klijent].rijec;
                                    DateTime vreme = odgovori[klijent].vrijeme;

                                    int poeniZaRec = slagalica.ProveriRec(rec);

                                    if (poeniZaRec == 0)
                                    {
                                        rezultat = $"Reč \"{rec}\" nije validna. Osvojeno: 0 poena.";
                                    }
                                    else
                                    {
                                        double osvojeni = poeniZaRec;

                                        if (klijent != najbrzi && najbrzi != null)
                                        {
                                            double umanjenje = 0.1 * poeniZaRec;


                                            osvojeni = poeniZaRec - umanjenje;
                                        }

                                        KonacniRezultat[klijent] += osvojeni;
                                        rezultat = $"Vaša reč: \"{rec}\" ({rec.Length} slova). Osvojeno: {osvojeni} poena. Ukupno: {KonacniRezultat[klijent]}";
                                    }
                                }

                                klijent.Send(Encoding.UTF8.GetBytes(rezultat));
                            }


                        }
                        else if (igra == "sk")
                        {
                            Skocko skocko = new Skocko();
                            skocko.GenerisiKombinaciju();

                            string pocetnaSlova = "Unesite kombinaciju sledecih znakova HTPKSZ";
                            byte[] bajtovi = Encoding.UTF8.GetBytes(pocetnaSlova);
                            foreach (var socket in sviKlijenti)
                            {
                                socket.Send(bajtovi);
                            }

                            foreach (var klijent in sviKlijenti.ToList())  // Koristim ovdje .ToList da bih mogla da micem klijente iz liste
                            {
                                int brojac = 1;
                                int brojOsvojenihPoena = 0;

                                while (brojac <= 6)
                                {
                                    List<Socket> spremniZaCitanje = new List<Socket> { klijent };   // Pravimo listu klijenata spremnih za citanje i stavljamo tu prvog klijenta
                                    Socket.Select(spremniZaCitanje, null, null, 100*20000000);      // Provjeravamo da li taj klijent ima nesto za citanje

                                    if (spremniZaCitanje.Count > 0)
                                    {
                                        byte[] buffer = new byte[1024];
                                        int primljenoBajtova = 0;

                                        try
                                        {
                                            primljenoBajtova = klijent.Receive(buffer);             // Ako ima, server prima tu poruku
                                        }
                                        catch (SocketException se)
                                        {
                                            Console.WriteLine($"Greška prilikom primanja podataka: {se.Message}");
                                            klijent.Close();
                                            sviKlijenti.Remove(klijent);                                            
                                            break;
                                        }

                                        if (primljenoBajtova == 0)
                                        {
                                            Console.WriteLine("Klijent je zatvorio vezu.");
                                            klijent.Close();
                                            sviKlijenti.Remove(klijent);
                                            break;
                                        }

                                        string kombinacijaIgraca = Encoding.UTF8.GetString(buffer, 0, primljenoBajtova).Trim();
                                        Console.WriteLine($"Klijent poslao: {kombinacijaIgraca}");

                                        string rezultat = skocko.ProveriKombinaciju(kombinacijaIgraca);

                                        if (rezultat.StartsWith("4 znaka su na pravom mestu"))
                                        {
                                            switch (brojac)
                                            {
                                                case 1: brojOsvojenihPoena = 30; break;
                                                case 2: brojOsvojenihPoena = 25; break;
                                                case 3: brojOsvojenihPoena = 20; break;
                                                case 4: brojOsvojenihPoena = 15; break;
                                                default: brojOsvojenihPoena = 10; break;
                                            }

                                            KonacniRezultat[klijent] += brojOsvojenihPoena;
                                            string obav = rezultat + $" Osvojili ste {KonacniRezultat[klijent]} poena!";
                                            klijent.Send(Encoding.UTF8.GetBytes(obav));
                                            break;
                                        }
                                        else
                                        {
                                            klijent.Send(Encoding.UTF8.GetBytes(rezultat));
                                        }

                                        brojac++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Klijent nije poslao ništa u zadatom vremenu.");
                                        continue;
                                    }
                                }
                            }

                        }
                        else
                        {
                            KoZnaZna kzz = new KoZnaZna();
                            kzz.UcitavanjePitanja();

                            // Za svako pitanje
                            foreach (var pitanje in kzz.SvaPitanja)
                            {
                                if (!kzz.OpcijePitanja.TryGetValue(pitanje.Key, out var opcije)) //pvdje provjeravamo da li pitanje ima opcije za odgovor
                                    continue;

                                string tekstPitanja = $"{pitanje.Key}\n1 - {opcije.Item1}\n2 - {opcije.Item2}\n3 - {opcije.Item3}";
                                byte[] pitanjeBytes = Encoding.UTF8.GetBytes(tekstPitanja);

                                foreach (var klijent in sviKlijenti)
                                {
                                    klijent.Send(pitanjeBytes);        //svakom od igraca se proslijedjuje pitanje
                                }

                                Dictionary<Socket, DateTime> vremeOdgovora = new();  // Prati ko je odgovorio prvi i kad
                                Dictionary<Socket, int> odgovori = new();            // Prati odgovore igraca
                                DateTime pocetak = DateTime.Now;                     // Vrijeme kada je pitanje poslato

                                TimeSpan timeout = TimeSpan.FromSeconds(30); // max vreme za pitanje
                                while ((DateTime.Now - pocetak) < timeout && odgovori.Count < sviKlijenti.Count) //ovdje provjeravamo da li su svi igraci odgovorili prije 30s
                                {
                                    List<Socket> spremni = new List<Socket>(sviKlijenti);       // U listu spremnih klijenata stavljamo sve klijente iz liste klijenata
                                    Socket.Select(spremni, null, null, 5000000);      // Provjeravamo da li je klijent poslao odgovor

                                    foreach (var klijent in spremni)
                                    {
                                        if (odgovori.ContainsKey(klijent)) continue;  // Provjeravamo da li je klijent odgovorio, da ne bi imali vise odgovora od jednog klijenta

                                        byte[] buffer = new byte[1024];
                                        int primljeno = 0;

                                        try
                                        {
                                            primljeno = klijent.Receive(buffer);
                                        }
                                        catch
                                        {
                                            continue; // greška, ignoriši
                                        }

                                        if (primljeno > 0)
                                        {
                                            string odgovorStr = Encoding.UTF8.GetString(buffer, 0, primljeno).Trim();
                                            if (int.TryParse(odgovorStr, out int odgovor))
                                            {
                                                odgovori[klijent] = odgovor;              // U ove dvije linije koda biljezimo odgovor i vrijeme odgovora klijenta u recniku odgovori
                                                vremeOdgovora[klijent] = DateTime.Now;
                                            }
                                            else
                                            {
                                                string mess = "Nepravilan unos. Očekuje se broj 1, 2 ili 3.";
                                                klijent.Send(Encoding.UTF8.GetBytes(mess));
                                            }
                                        }
                                    }
                                }

                                // Obradi odgovore
                                var tacniOdgovori = odgovori // Lista svih klijenata koji su tačno odgovorili, sortirana po brzini (najbrži prvi)
                                    .Where( parIzOdgovora=> kzz.Provjera(pitanje.Key, parIzOdgovora.Value).Contains("Tacan")) // Ovdje prolazimo kroz svaki odgovor da provjerimo je li tacan i zadrzavamo samo one klijente sa tacnim odg
                                    .OrderBy(parIzOdgovora => vremeOdgovora[parIzOdgovora.Key])  // Sortiramo odgovore po vremenu kada su pristizali (od najbrzeg do najsporijeg) 
                                    .ToList(); // Rezultate svih ovih upita pretvaramo u listu

                                Socket? prviTacan = tacniOdgovori.FirstOrDefault().Key; // Uzimamo klijenta prvog iz liste tacnih odgovora koju smo gore napravili

                                foreach (var klijent in sviKlijenti)
                                {
                                    string rezultat;

                                    if (!odgovori.ContainsKey(klijent))
                                    {
                                        rezultat = "Niste odgovorili na vreme.";
                                    }
                                    else
                                    {
                                        int odgovor = odgovori[klijent];
                                        string provjera = kzz.Provjera(pitanje.Key, odgovor);

                                        if (provjera.Contains("Tacan"))
                                        {
                                            int poeniZaPitanje = 10;    //Ako je pitanje tacno 10 poena

                                            if (klijent == prviTacan)
                                            {
                                                KonacniRezultat[klijent] += poeniZaPitanje;  //Ako je klijent prvi odgovorio njemu ide svih 10 poena
                                            }
                                            else
                                            {
                                                TimeSpan kasnjenje = vremeOdgovora[klijent] - vremeOdgovora[prviTacan]; // Provjeravamo koliko je ovaj igrač kasnio u odnosu na onog koji je prvi tacno odgovorio

                                                if ((int)kasnjenje.TotalSeconds < 30)
                                                {
                                                    double umanjenje=0.1 * poeniZaPitanje;          //Ako klijent kasni sa odgovorom do 30s dobija 10% manje poena
                                                    KonacniRezultat[klijent] += poeniZaPitanje - umanjenje;

                                                }                                

                                                provjera += $" (kasnio/la {kasnjenje.Seconds}s)";
                                            }

                                            rezultat = provjera + $" Trenutni poeni: {KonacniRezultat[klijent]}";
                                        }
                                        else
                                        {
                                            KonacniRezultat[klijent] -= 5;
                                            rezultat = provjera + $" Trenutni poeni: {KonacniRezultat[klijent]}";
                                        }
                                    }

                                    klijent.Send(Encoding.UTF8.GetBytes(rezultat));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            Console.WriteLine("\n--- KRAJ IGARA ---");
            Console.WriteLine("Konačni rezultati:");

            foreach (var klijent in sviKlijenti)
            {
                double ukupno = KonacniRezultat.ContainsKey(klijent) ? KonacniRezultat[klijent] : 0;
                string finalnaPoruka = $"KRAJ IGARA.\nVaš ukupan broj poena: {ukupno}";

                try
                {
                    klijent.Send(Encoding.UTF8.GetBytes(finalnaPoruka));
                    Console.WriteLine($"{klijent.RemoteEndPoint} - {ukupno} poena");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri slanju rezultata klijentu {klijent.RemoteEndPoint}: {ex.Message}");
                }

                klijent.Close();
            }

            foreach (var klijent in sviKlijenti)
            {
                klijent.Send(Encoding.UTF8.GetBytes("kraj"));
                klijent.Close();
            }

            udpSocket.Close();
            tcpSocket.Close();

            Environment.Exit(0); 

        }
    }
}