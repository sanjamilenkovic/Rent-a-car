using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACarWebApp.ViewModels
{
    public class CarViewModel
    {
        public string Slika { get; set; }
        public Dictionary<String, String> sviAtributiAutomobila { get; set; }

        public CarViewModel()
        {
            sviAtributiAutomobila = new Dictionary<string, string>();
        }
    }
}