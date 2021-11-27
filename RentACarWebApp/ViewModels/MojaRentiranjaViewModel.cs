using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class MojaRentiranjaViewModel
    {
        public Korisnik Korisnik { get; set; }
        public List<Istorija> ListaSvihRentovaKorisnika{ get; set; }
        public List<Rentiranje> ListaAktivnihRentiranja { get; set; }

        public MojaRentiranjaViewModel()
        {
            ListaSvihRentovaKorisnika = new List<Istorija>();
            ListaAktivnihRentiranja = new List<Rentiranje>();

        }
    }

    public class Rentiranje
    {
        public string Kljuc { get; set; }
        public string IdKorisnika { get; set; } 
        public string IdAutomobila { get; set; }
        public Automobil Automobil { get; set; }
        public Korisnik Korisnik { get; set; }

        public string AdresaPreuzimanja { get; set; }
        public string AdresaVracanja { get; set; }
        public string DatumPreuzimanja { get; set; }
        public string DatumVracanja { get; set; }
        public string DodatneUsluge { get; set; }
        public String Cena{ get; set; }
        public List<object> listaUsluga { get; set; }

    }
}