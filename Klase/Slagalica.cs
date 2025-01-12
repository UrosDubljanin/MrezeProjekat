using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    public class Slagalica
    {
        public string PonudjenaSlova { get; private set; }
        public string SastavljenaRec { get; private set; }

        private static readonly Random random = new Random();
        public Slagalica()
        {
            PonudjenaSlova = "";
            SastavljenaRec = "";
        }

        public void GenerisiSlova()
        {
            PonudjenaSlova = "";
            string slova = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; 
            char[] generisanaSlova = new char[12];
            int brojRazlicitihSlova = 0;

            for (int i = 0; i < 12; i++)
            {
                char novoSlovo = slova[random.Next(slova.Length)];

                generisanaSlova[i] = novoSlovo;

                bool razlicito = true;
                for (int j = 0; j < i; j++)
                {
                    if (generisanaSlova[j] == novoSlovo)
                    {
                        razlicito = false;
                        break;
                    }
                }

                if (razlicito)
                {
                    brojRazlicitihSlova++;
                }

                if (i == 11 && brojRazlicitihSlova < 5)
                {
                    i = -1;
                    brojRazlicitihSlova = 0;
                }
            }

            PonudjenaSlova = new string(generisanaSlova);
        }

    }
}
