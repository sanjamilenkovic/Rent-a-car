using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class KorisnikViewModel
    {
        public Korisnik Korisnik { get; set; }
        public List<Dictionary<string,object>> IstorijaRentiranjaKorisnika{ get; set; }

        public KorisnikViewModel()
        {
            IstorijaRentiranjaKorisnika = new List<Dictionary<string, object>>();
        }
    }
}