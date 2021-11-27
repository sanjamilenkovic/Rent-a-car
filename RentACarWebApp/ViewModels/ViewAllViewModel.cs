using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class ViewAllViewModel
    {
        public List<Automobil> listaSvihAutomobila { get; set; }
        public List<ModelAutomobila> SviModeli { get; set; }

        public IList<SelectListItem> SveKlaseAutomobila { get; set; }
        public List<string> OdabraneKlaseAutomobila { get; set; }


        public ViewAllViewModel()
        {
            listaSvihAutomobila = new List<Automobil>();
            SveKlaseAutomobila = new List<SelectListItem>();
            OdabraneKlaseAutomobila = new List<string>();
            SviModeli = new List<ModelAutomobila>();
        }
    }

    public class ModelAutomobila
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Model { get; set; }
        public string Brend { get; set; }
        public string Cena { get; set; }
        public string Slika { get; set; }
        public int UkupanBrojModela { get; set; }
        public int BrojDostupnih { get; set; }



    }
}