using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            bodovi = [0, 0, 0];
        }
    }
}
