using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACarWebApp.Models
{
    public class DodatnaUsluga
    {
        public string Id { get; set; }
        public string Naziv { get; set; }
        public int Cena { get; set; }
        public bool IsSelected { get; set; }
    }
}