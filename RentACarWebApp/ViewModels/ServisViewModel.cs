using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RentACarWebApp.ViewModels
{
    public class ServisViewModel
    {
        public IList<SelectListItem> SviModeli { get; set; }
        public IList<SelectListItem> SviServisi { get; set; }
        public IList<SelectListItem> SviOgranci { get; set; }
        public IList<SelectListItem> SviAutomobili { get; set; }
        public List<Dictionary<string,object>> DetaljiServisa{ get; set; }
        public string OdabranServis { get; set; }
        public string OdabranAutomobil { get; set; }
        public string OdabranOgranak { get; set; }
        public string OdabranModel { get; set; }




        public ServisViewModel()
        {
            SviModeli = new List<SelectListItem>();
            SviServisi = new List<SelectListItem>();
            SviOgranci = new List<SelectListItem>();
            SviAutomobili = new List<SelectListItem>();
            DetaljiServisa = new List<Dictionary<string, object>>();
        }
    }
}