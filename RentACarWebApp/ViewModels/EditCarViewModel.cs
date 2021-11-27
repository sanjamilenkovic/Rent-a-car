using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACarWebApp.ViewModels
{
    public class EditCarViewModel
    {
        public Dictionary<string, object> dodatnaOprema{ get; set; }
        public Dictionary<string, string> atributiAutomobila { get; set; }

        public string IdAutomobila { get; set; }
        public EditCarViewModel()
        {
            atributiAutomobila = new Dictionary<string, string>();
            dodatnaOprema = new Dictionary<string, object>();

        }
    }
}