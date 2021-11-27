using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RentACarWebApp.Models
{
    public class Automobil
    {
        public String Kljuc { get; set; }
        public String Brend { get; set; }
        public String Model { get; set; }
        public String Registracija { get; set; }
        public String Menjac { get; set; }
        public String Cena { get; set; }
        public String Gorivo { get; set; }
        public String Pogon { get; set; }
        public String Snaga { get; set; }
        public String BrojVrata { get; set; }
        public String BrojSedista { get; set; }
        public String ZapreminaGepeka { get; set; }
        public String Slika { get; set; }
        public List<Object> DodatnaOprema { get; set; }
        public Dictionary<String, Object> sviAtributi { get; set; }

        public Automobil()
        {
            DodatnaOprema = new List<Object>();

            sviAtributi = new Dictionary<string, object>();
        }
    }
}