using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACarWebApp.Models
{
    public class Istorija
    {
        public string IdKorisnika { get; set; }
        public string IdAutomobila { get; set; }
        public string AdresaPreuzimanja { get; set; }
        public string AdresaVracanja { get; set; }
        public string DatumPreuzimanja { get; set; }
        public string DatumVracanja { get; set; }
        public string Kljuc{ get; set; }
        public Boolean Zatvoren{ get; set; }
        public string DodatneBeleske { get; set; }
        public Korisnik korisnik{ get; set; }
        public Automobil automobil{ get; set; }
        public Boolean proba { get; set; }
        public string Beleske{ get; set; }
        public int UkupnoZaduzenje{ get; set; }
        public string NaplaceneKazne { get; set; }
        public string OstecenjaVozila { get; set; }
        public int PredjenaKilometraza{ get; set; }


        public Boolean AutomobilPreuzet{ get; set; }
        public Boolean AutomobilIspravan{ get; set; }
        public Object DodatneUsluge{ get; set; }
        public List<object> listaUsluga { get; set; }
        public Istorija()
        {
            this.listaUsluga = new List<object>();
        }
    }
}