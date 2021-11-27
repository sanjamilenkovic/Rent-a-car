using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class RezervacijeViewModel
    {
        public List<RentViewModel> SveRezervacije{ get; set; }
        public string OdabranaRezervacija { get; set; }
        public List<Automobil> DostupniAutomobili{ get; set; }
        public IList<string> OdabraniAutomobili { get; set; }
        public IList<SelectListItem>  PonudaAutomobila{ get; set; }
        public RezervacijeViewModel()
        {
            SveRezervacije = new List<RentViewModel>();
            DostupniAutomobili = new List<Automobil>();
            OdabraniAutomobili = new List<string>();
            PonudaAutomobila = new List<SelectListItem>();
        }
    }
}