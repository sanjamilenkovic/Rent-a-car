using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class IstorijaViewModel
    {
        //danasnje zatvaranje
        public List<Istorija> rentoviZaZatvaranje { get; set; }
        public Automobil automobil { get; set; }
        public Korisnik korisnik{ get; set; }

        //danasnji servisi
        public IList<SelectListItem> SviOgranci { get; set; }
        public string OdabranOgranak { get; set; }
        public List<Dictionary<string,object>> SpisakServisa{ get; set; }

        public IstorijaViewModel()
        {
            rentoviZaZatvaranje = new List<Istorija>();
            SviOgranci = new List<SelectListItem>();
            SpisakServisa = new List<Dictionary<string, object>>();

        }
    }
    
   
}