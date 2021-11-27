using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RentACarWebApp.Models;

namespace RentACarWebApp.ViewModels
{
    public class NewCarViewModel
    {
        public Automobil Automobil { get; set; }
        public IList<SelectListItem> SviOgranci { get; set; }
        public string OdabranOgranak { get; set; }

        public NewCarViewModel()
        {
            SviOgranci = new List<SelectListItem>();
        }
    }
}