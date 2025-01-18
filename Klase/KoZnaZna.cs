using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    public class KoZnaZna
    {
        public string TekucePitanje { get; set; }
        public int TacanOdgovor { get; set; }
        public Dictionary<string, int> SvaPitanja { get; set; }

        public Dictionary<string, Tuple<string, string, string>> OpcijePitanja { get; set; }

        public KoZnaZna()
        {
            TekucePitanje = "";
            TacanOdgovor = 0;
            SvaPitanja = new Dictionary<string, int>();
            OpcijePitanja = new Dictionary<string, Tuple<string, string, string>>();
        }
        public void UcitavanjePitanja()
        {
            SvaPitanja.Add("Koje je najduza rijeka na svijetu?", 2);
            SvaPitanja.Add("Glavni grad Francuske?", 1); 
            SvaPitanja.Add("U kom Home Alone filmu djecakova porodica odlazi u Pariz zaboravljajuci njega?", 3);

            OpcijePitanja.Add("Koje je najduza rijeka na svijetu?", Tuple.Create("Amazon", "Nil", "Sena"));
            OpcijePitanja.Add("Glavni grad Francuske?", Tuple.Create("Pariz", "London", "Madrid"));
            OpcijePitanja.Add("U kom Home Alone filmu djecakova porodica odlazi u Pariz zaboravljajuci njega?", Tuple.Create("Prvi", "Drugi", "Treci"));
        }
        public string Provjera(string pitanje, int odgovor)
        {
            if (SvaPitanja.ContainsKey(pitanje))
            {
                TekucePitanje = pitanje;
                TacanOdgovor = SvaPitanja[pitanje];

                if (odgovor == TacanOdgovor)
                {
                    return "Tacan odgovor!";
                }
                else
                {
                    return "Netacan odgovor.";
                }
            }
            else
            {
                return "Pitanje nije pronađeno.";
            }
        }
    }
}
