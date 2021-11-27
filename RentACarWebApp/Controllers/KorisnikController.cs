using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Arango.Client;
using RentACarWebApp.Models;
using RentACarWebApp.ViewModels;

namespace RentACarWebApp.Controllers
{
    public class KorisnikController : Controller
    {

        private ADatabase db;
        private HomeController homeController;
        private CarController carController;


        public KorisnikController()
        {
            this.homeController = new HomeController();
            carController = new CarController();
            ArangoDBContext dbContext = new ArangoDBContext();
            db = dbContext.ArangoDatabase;
        }

        // GET: Korisnik
        public ActionResult Index()
        {
            return View();
        }

        [Route("ViewSingle/{id}")]
        public ActionResult ViewSingle(string id)
        {

            //pribavljanje istorije rentiranja za korisnika
            MojaRentiranjaViewModel vm = new MojaRentiranjaViewModel();

            var queryResult = db.Query
                .BindVar("id", id)
                .Aql(@"
                        FOR item IN Korisnik 
                            FILTER item._key == @id
                            RETURN item
                        ")
                .ToDocument();

            Korisnik k = new Korisnik();
            k.Id = queryResult.Value["_key"].ToString();
            k.Ime = queryResult.Value["Ime"].ToString();
            k.Prezime = queryResult.Value["Prezime"].ToString();
            k.Email = queryResult.Value["Email"].ToString();
            k.Grad = queryResult.Value["Grad"].ToString();
            k.BrojTelefona = queryResult.Value["BrojTelefona"].ToString();

            vm.Korisnik = k;


            //pribavljanje istorije rentiranja
            var query = db.Query
                .BindVar("user", "Korisnik/"+ id)
                .Aql(@"
                    FOR item in korisnikRentirao
                    FILTER item._to == @user
                    FILTER item.Zatvoren == true
                    RETURN item
                    ")
                .ToDocuments();

            foreach (var podatak in query.Value)
            {
                var z = Boolean.Parse(podatak["Zatvoren"].ToString());
                if(z != null)
                {
                    Istorija r = new Istorija();
                    r.IdAutomobila = podatak["_from"].ToString();
                    r.automobil = carController.PribaviAuto(r.IdAutomobila);

                    r.AdresaPreuzimanja = podatak["AdresaPreuzimanja"].ToString();
                    r.AdresaVracanja = podatak["AdresaVracanja"].ToString();
                    r.DatumPreuzimanja = podatak["DatumPreuzimanja"].ToString();
                    r.DatumVracanja = podatak["DatumVracanja"].ToString();

                    r.UkupnoZaduzenje = Int32.Parse(podatak["Cena"].ToString());
                    r.Beleske = podatak["Beleske"].ToString();
                    r.listaUsluga = (List<object>)podatak["DodatneUsluge"];
                    r.Zatvoren = Boolean.Parse(podatak["Zatvoren"].ToString());
                    r.AutomobilIspravan = Boolean.Parse(podatak["AutomobilIspravan"].ToString());


                    vm.ListaSvihRentovaKorisnika.Add(r);
                }

                
            }


            //pribavljanje trenutno aktivnih rentiranja
            var queryAktivni = db.Query
                .BindVar("user", "Korisnik/" + id)
                .Aql(@"
                    FOR item in korisnikRentirao
                    FILTER item._to == @user
                    FILTER item.Zatvoren == false
                    RETURN item
                    ")
                .ToDocuments();

            foreach (var podatak in queryAktivni.Value)
            {
                Rentiranje r = new Rentiranje();
                r.IdAutomobila = podatak["_from"].ToString();
                r.Automobil = carController.PribaviAuto(r.IdAutomobila);
                r.AdresaPreuzimanja = podatak["AdresaPreuzimanja"].ToString();
                r.AdresaVracanja = podatak["AdresaVracanja"].ToString();
                r.DatumPreuzimanja = podatak["DatumPreuzimanja"].ToString();
                r.DatumVracanja = podatak["DatumVracanja"].ToString();
                r.listaUsluga = (List<object>)podatak["DodatneUsluge"];
                r.Cena = podatak["Cena"].ToString();

                vm.ListaAktivnihRentiranja.Add(r);
            }


            return View(vm);
        }

        public ActionResult ViewAll()
        {
            var queryResult = db.Query
                .Aql(@"
                            FOR item IN Korisnik 
                                RETURN item
                            ")
                .ToDocuments();

            List<Korisnik> listaSvihKorisnika = new List<Korisnik>();

            if(queryResult.Success)
            {
                foreach(var korisnik in queryResult.Value)
                {
                    Korisnik k = new Korisnik();
                    k.Id = korisnik["_key"].ToString();
                    k.Email = korisnik["Email"].ToString();
                    k.BrojTelefona = korisnik["BrojTelefona"].ToString();
                    k.Ime = korisnik["Ime"].ToString();
                    k.Prezime = korisnik["Prezime"].ToString();
                    k.Grad = korisnik["Grad"].ToString();
                    k.UserName = korisnik["UserName"].ToString();

                    listaSvihKorisnika.Add(k);
                }
            }

            return View(listaSvihKorisnika);
        }
    }
}