using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    public class Skocko
    {
        public string TrazenaKomb {  get; set; }
        public string TekucaKomb {  get; set; }

        private static readonly Random random = new Random();
        public Skocko()
        {
            TrazenaKomb = "";
            TekucaKomb = "";
        }

        public void GenerisiKombinaciju()
        {
            TrazenaKomb = "";
            string slova = "HTPKSZ";
            char[] generisanaSlova = new char[4];

            for(int i=0; i<4; i++)
            {
                char novoSlovo = slova[random.Next(slova.Length)];
                generisanaSlova[i] = novoSlovo;
            }

            TrazenaKomb = new string(generisanaSlova);
        }


        public string ProveriKombinaciju(string TekucaKomb)
        {
            int naPravomMestu = 0;
            int naPogresnomMestu = 0;

            List<char> preostaliZnakovi = TrazenaKomb.ToList();

            for (int i = 0; i < TekucaKomb.Length; i++)
            {
                if (TekucaKomb[i] == TrazenaKomb[i])
                {
                    naPravomMestu++;
                    preostaliZnakovi.Remove(TekucaKomb[i]);
                }
            }

            for (int i = 0; i < TekucaKomb.Length; i++)
            {
                if (TekucaKomb[i] != TrazenaKomb[i] && preostaliZnakovi.Contains(TekucaKomb[i]))
                {
                    naPogresnomMestu++;
                    preostaliZnakovi.Remove(TekucaKomb[i]); 
                }
            }

            return $"{naPravomMestu} znaka su na pravom mestu, {naPogresnomMestu} nisu na mestu.|{naPravomMestu}|{naPogresnomMestu}|";
        }

    }
}
