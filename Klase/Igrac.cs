using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Klase
{
    public class Igrac
    {
        public int ID { get; set; } = 0;
        public string KorisnickoIme { get; set; } = "";
        public int[] bodovi { get; set; }= new int[3];

        public Igrac(int iD, string korisnickoIme)
        {
            ID = iD;
            KorisnickoIme = korisnickoIme;
            bodovi = new int[3] {0,0,0};
        }
        public int izracunajUkupno()
        {
            int rezultat = bodovi[0] + bodovi[1] + bodovi[2];
            return rezultat;
        }
    }
}
