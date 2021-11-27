    using System;
using System.Collections.Generic;
using System.Linq;
using RentACarWebApp.Models;
using System.Web;
using System.Web.Mvc;

namespace RentACarWebApp.ViewModels
{
    public class RentViewModel
    {
        public string Id{ get; set; }
        public string KorisnikId { get; set; }
        public string AutomobilId { get; set; }
        public Korisnik Korisnik{ get; set; }
        public ModelAutomobila ModelAutomobila{ get; set; }
        public string AdresaPreuzimanja { get; set; }
        public string AdresaVracanja { get; set; }
        public DateTime DatumVracanja { get; set; }
        public DateTime DatumPreuzimanja{ get; set; }
        public int BrojVozila { get; set; }
        public string UkupnoZaduzenje { get; set; }
        public List<Object> listaUsluga{ get; set; }


        public string DVracanja { get; set; }
        public string DPreuzimanja { get; set; }


        public IList<string> SelectedUsluge { get; set; }
        public IList<SelectListItem> DostupneUsluge { get; set; }
        public IList<DodatnaUsluga> SveDodatneUsluge { get; set; }

        public List<Automobil> DostupniAutomobili { get; set; }
        public IList<string> OdabraniAutomobili { get; set; }
        public IList<SelectListItem> PonudaAutomobila { get; set; }

        public RentViewModel()
        {
            DostupniAutomobili = new List<Automobil>();
            OdabraniAutomobili = new List<string>();
            PonudaAutomobila = new List<SelectListItem>();

            SelectedUsluge = new List<string>();
            SveDodatneUsluge = new List<DodatnaUsluga>();
            DostupneUsluge = new List<SelectListItem>();

            listaUsluga = new List<object>();

        }

    }
}