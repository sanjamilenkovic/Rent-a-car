using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using RentACarWebApp.Models;
using Arango.Client;
using System.Threading.Tasks;
using RentACarWebApp.ViewModels;

namespace RentACarWebApp.Controllers
{
    public class HomeController : Controller
    {
        private ADatabase db;
        private CarController carController;
        private List<object> listaDodatnihUsluga;
        public HomeController()
        {
            ArangoDBContext dbContext = new ArangoDBContext();
            db = dbContext.ArangoDatabase;
            carController = new CarController();
            listaDodatnihUsluga = new List<object>();
        }

        public ActionResult Index()
        {
            List<ModelAutomobila> sviModeli = new List<ModelAutomobila>();

            //pribavljanje svih modela automobila
            var queryResult = db.Query
                .Aql(@"
                    FOR item IN modeli 
                        RETURN item
                    ")
                .ToDocuments();

            foreach (var res in queryResult.Value)
            {
                ModelAutomobila m = new ModelAutomobila();
                m.Id = res["_id"].ToString();
                m.Brend = res["Brend"].ToString();
                m.Model = res["Model"].ToString();
                m.Key = res["_key"].ToString();
                m.Slika = res["Slika"].ToString();
                m.Cena = res["Cena"].ToString();

                sviModeli.Add(m);
            }

            return View(sviModeli);
        }

        //pribavljanje svih rezervacija koje treba da se zatvore, odavde se poziva FinishRezervacije
        [HttpGet]
        public ActionResult Rezervacije()
        {
            RezervacijeViewModel rezvm = new RezervacijeViewModel();

            var queryResult = db.Query
                    .Aql(@"
                        FOR item IN korisnikRezervisaoModel
                        FILTER item.Obradjen == false
                        RETURN item
                        ")
                    .ToDocuments();

            foreach (var rez in queryResult.Value)
            {
                RentViewModel r = new RentViewModel();
                r.Id = rez["_key"].ToString();

                string model = rez["_to"].ToString();
                string user = rez["_from"].ToString();
                r.Korisnik = PribaviKorisnika(user);
                r.ModelAutomobila = PribaviModelAutomobila(model);
                r.BrojVozila = Int32.Parse(rez["BrojVozila"].ToString());


                r.AdresaPreuzimanja = rez["AdresaPreuzimanja"].ToString();
                r.DVracanja = rez["DatumVracanja"].ToString();
                r.DPreuzimanja = rez["DatumPreuzimanja"].ToString();
                r.AdresaVracanja = rez["AdresaVracanja"].ToString();
                r.UkupnoZaduzenje = rez["UkupnoZaduzenje"].ToString();

                rezvm.SveRezervacije.Add(r);
            }

            return View(rezvm);
        }


        //podesavanja za formu u kojoj biram konkretne automobile na osnovu modela koji korisnik zeli da rentira
        [HttpGet]
        public ActionResult FinishRezervacije(string id)
        {

            var queryResult = db.Query
                .BindVar("id", id)
                    .Aql(@"
                        FOR item IN korisnikRezervisaoModel
                        FILTER item._key == @id
                        RETURN item
                        ")
                    .ToDocuments();
            RentViewModel r = new RentViewModel();

            foreach (var rez in queryResult.Value)
            {
                r.Id = rez["_key"].ToString();

                r.AutomobilId = rez["_to"].ToString();
                r.KorisnikId = rez["_from"].ToString();
                r.Korisnik = PribaviKorisnika(r.KorisnikId);
                r.ModelAutomobila = PribaviModelAutomobila(r.AutomobilId);
                r.BrojVozila = Int32.Parse(rez["BrojVozila"].ToString());
                r.listaUsluga = (List<object>)rez["DodatneUsluge"];

                r.AdresaPreuzimanja = rez["AdresaPreuzimanja"].ToString();
                r.DVracanja = rez["DatumVracanja"].ToString();
                r.DPreuzimanja = rez["DatumPreuzimanja"].ToString();
                r.AdresaVracanja = rez["AdresaVracanja"].ToString();
                r.UkupnoZaduzenje = rez["UkupnoZaduzenje"].ToString();

            }

            this.listaDodatnihUsluga = r.listaUsluga;

            //PRIBAVLJANJE AUTOMOBILA KOJI NISU RENTIRANI

            //            FOR x IN OUTERSECTION(
            //            (
            //                FOR y IN korisnikRentirao
            //                  RETURN { id: y._from, to: y._to}
            //               ),

            //                (FOR y IN ANY 'modeli/renault-clio' automobilPripadaModelu
            //                RETURN  y._id)
            //                )
            //                RETURN x
            //            ").ToObject();

            List<String> raz1 = new List<String>();
            List<String> raz2 = new List<String>();

            List<String> listaRezultata = new List<String>();


            var razlika1 = db.Query
                .Aql(@"
                        FOR y IN korisnikRentirao
                        FILTER y.Zatvoren == false
                        RETURN {id: y._from, to: y._to}").ToDocuments();

            if (razlika1.Value.Count > 0)
            {
                foreach (var a in razlika1.Value)
                {
                    raz1.Add(a["id"].ToString());
                }
            }


            var razlika2 = db.Query
                .BindVar("model", r.ModelAutomobila.Id)
                .Aql(@"
                    FOR y IN ANY @model automobilPripadaModelu
                            RETURN y").ToDocuments();

            foreach (var a in razlika2.Value)
            {
                raz2.Add(a["_id"].ToString());
            }


            if (razlika1.Value.Count > 0)
                listaRezultata = raz2.Except(raz1).ToList();
            else
                listaRezultata = raz2;

            foreach (var car in listaRezultata)
            {
                var x = carController.PribaviAuto(car);
                r.DostupniAutomobili.Add(x);
            }

            foreach (var s in r.DostupniAutomobili)
            {
                SelectListItem ss = new SelectListItem()
                {
                    Text = s.Kljuc,
                    Value = s.Kljuc
                };
                r.PonudaAutomobila.Add(ss);
            }

            return View(r);
        }


        //pamcenje u bazi konketnih potega za rentiranje + brisem rezervaciju
        [HttpPost]
        public ActionResult FinishRezervacije(RentViewModel rvm)
        {
            var x = this.listaDodatnihUsluga;
            foreach (var odabranModel in rvm.OdabraniAutomobili)
            {
                var cena = Int32.Parse(rvm.UkupnoZaduzenje) / rvm.BrojVozila;

                var edgeData = new Dictionary<string, object>()
                    .String("AdresaPreuzimanja", rvm.AdresaPreuzimanja)
                    .String("DatumPreuzimanja", rvm.DPreuzimanja)
                    .String("AdresaVracanja", rvm.AdresaVracanja)
                    .String("DatumVracanja", rvm.DVracanja)
                    .Bool("Obradjen", true)
                    .Bool("Zatvoren", false)
                    .String("Cena", cena.ToString())
                    .Object("DodatneUsluge", rvm.listaUsluga);

                var createEdgeResult = db.Document
                    .WaitForSync(true)
                    .CreateEdge("korisnikRentirao", "automobili2/" + odabranModel, rvm.KorisnikId, edgeData);


            }
            var deleteRezervaciju = db.Document
                    .Delete("korisnikRezervisaoModel/" + rvm.Id);

            return RedirectToAction("ViewAll", "Car");
        }



        public ActionResult MojiRentovi()
        {
            //pribavljanje istorije rentiranja za korisnika
            MojaRentiranjaViewModel vm = new MojaRentiranjaViewModel();

            //pribavljanje id-ja korisnika
            var queryResult = db.Query
                     .BindVar("user", User.Identity.Name)
                     .Aql(@"
                            FOR item IN Korisnik
                                FILTER item.UserName == @user
                                RETURN item
                        ")
                     .ToDocument();

            var user = queryResult.Value.String("_id");

            //pribavljanje istorije rentiranja
            var query = db.Query
                .BindVar("user", user)
                .Aql(@"
                    FOR item in korisnikRentirao
                    FILTER item._to == @user
                    FILTER item.Zatvoren == true
                    RETURN item
                    ")
                .ToDocuments();

            foreach (var podatak in query.Value)
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


            //pribavljanje trenutno aktivnih rentiranja
            var queryAktivni = db.Query
                .BindVar("user", user)
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

        [HttpGet]
        public ActionResult AktivnaRentiranja()
        {
            MojaRentiranjaViewModel vm = new MojaRentiranjaViewModel();

            //pribavljanje trenutno aktivnih rentiranja
            var queryAktivni = db.Query
                .Aql(@"
                    FOR item in korisnikRentirao
                    FILTER item.Zatvoren == false
                    RETURN item
                    ")
                .ToDocuments();

            foreach (var podatak in queryAktivni.Value)
            {
                Rentiranje r = new Rentiranje();
                r.IdAutomobila = podatak["_from"].ToString();
                r.IdKorisnika = podatak["_to"].ToString();
                r.Automobil = carController.PribaviAuto(r.IdAutomobila);
                r.Korisnik = PribaviKorisnika(r.IdKorisnika);

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

        [HttpGet]
        public ActionResult DanasnjaDesavanja()
        {
            IstorijaViewModel ivm = PribaviRentKojiDanasTrebaZatvoriti();

            //pribavljanje ogranaka
            ivm.SviOgranci = GetOgranke();
            return View(ivm);
        }


        [HttpPost]
        public ActionResult DanasnjaDesavanja(IstorijaViewModel vm)
        {
            //pribavljamo sve zakazane servise za danas, po ograncima
            var ogranak = vm.OdabranOgranak; //_id tog ogranka
            String danas = DateTime.Now.ToShortDateString();


            //FOR part IN 1..4 OUTBOUND "ogranak/ogranak-nis" GRAPH "servis"
            //FILTER part.Naredni == "11/21/2021"
            //RETURN part
            var queryResult = db.Query
                .BindVar("ogranak", ogranak)
                .BindVar("graf", "servis")
                .BindVar("datum", danas)
                .Aql(@"
                        FOR part IN 1..4 OUTBOUND @ogranak GRAPH @graf
                        FILTER part.Naredni == @datum
                        RETURN part
                            ")
                .ToDocuments();



            IstorijaViewModel ivm = new IstorijaViewModel();
            ivm = PribaviRentKojiDanasTrebaZatvoriti();


            foreach (var servis in queryResult.Value)
            {
                ivm.SpisakServisa.Add(servis);
            }

            ivm.SviOgranci = GetOgranke();

            return View(ivm);
        }


        public IstorijaViewModel PribaviRentKojiDanasTrebaZatvoriti()
        {
            //FOR item IN korisnikRentirao
            //FILTER item.DatumVracanja == "11/18/2021"
            //RETURN item

            String danas = DateTime.Now.ToShortDateString();

            var queryResult = db.Query
                .BindVar("datum", danas)
                .Aql(@"
             FOR item IN korisnikRentirao
                 FILTER item.DatumVracanja == @datum
                 RETURN item
             ")
                .ToDocuments();

            IstorijaViewModel ivm = new IstorijaViewModel();

            foreach (var rent in queryResult.Value)
            {
                Istorija i = new Istorija();
                i.Zatvoren = (bool)rent["Zatvoren"];

                if (i.Zatvoren == false)
                {
                    i.IdAutomobila = rent["_from"].ToString();
                    i.IdKorisnika = rent["_to"].ToString();

                    i.korisnik = PribaviKorisnika(i.IdKorisnika);
                    i.automobil = carController.PribaviAuto(i.IdAutomobila);

                    i.AdresaPreuzimanja = rent["AdresaPreuzimanja"].ToString();
                    i.DatumPreuzimanja = rent["DatumPreuzimanja"].ToString();
                    i.DatumVracanja = rent["DatumVracanja"].ToString();
                    i.AdresaVracanja = rent["AdresaVracanja"].ToString();
                    i.Kljuc = rent["_key"].ToString();
                    //i.DodatneUsluge = rent["DodatneUsluge"];

                    ivm.rentoviZaZatvaranje.Add(i);
                }
            }
            return ivm;
        }

        [HttpGet]
        public ActionResult FinishRent(string id)
        {

            var queryResult = db.Query
                .BindVar("id", id)
                .Aql(@"
                FOR item IN korisnikRentirao
                    FILTER item._key == @id
                    RETURN item
                ")
                .ToDocuments();

            Istorija i = new Istorija();


            foreach (var rent in queryResult.Value)
            {
                i.IdAutomobila = rent["_from"].ToString();
                i.IdKorisnika = rent["_to"].ToString();

                i.korisnik = PribaviKorisnika(i.IdKorisnika);
                i.automobil = carController.PribaviAuto(i.IdAutomobila);
                i.AdresaPreuzimanja = rent["AdresaPreuzimanja"].ToString();
                i.DatumPreuzimanja = rent["DatumPreuzimanja"].ToString();
                i.DatumVracanja = rent["DatumVracanja"].ToString();
                i.AdresaVracanja = rent["AdresaVracanja"].ToString();
                i.Kljuc = rent["_key"].ToString();
                i.DodatneUsluge = rent["DodatneUsluge"];
            }

            return View(i);
        }

        [HttpPost]
        public ActionResult FinishRent(Istorija i)
        {
            var idDokumenta = i.Kljuc;

            var document = new Dictionary<string, object>()
            .Bool("Zatvoren", true)
            .String("NaplaceneKazne", i.NaplaceneKazne)
            .Int("PredjenaKilometraza",i.PredjenaKilometraza)
            .Bool("AutomobilIspravan", i.AutomobilIspravan)
            .String("Beleske", i.Beleske);

            var automobil = db.Document.Get("automobili2/" + i.automobil.Kljuc);

            var km = Int32.Parse(automobil.Value["Kilometraza"].ToString());

            int novakm = km + i.PredjenaKilometraza;

            var updateAuto = new Dictionary<string, object>()
                .Int("Kilometraza", novakm);

            var updateAutoResult = db.Document
                .Update("automobili2/" + i.automobil.Kljuc, updateAuto);

            var updateDocumentResult = db.Document
                .Update("korisnikRentirao/" + i.Kljuc, document);

            return RedirectToAction("ViewAll", "Car");
        }


        #region Pribavljanje Korisnik/Model
        public Korisnik PribaviKorisnika(string id)
        {
            var queryResult = db.Document.Get(id);

            Korisnik a = new Korisnik();

            foreach (var p in queryResult.Value)
            {
                a.Ime = queryResult.Value.String("Ime");
                a.Prezime = queryResult.Value.String("Prezime");
                a.Id = queryResult.Value.String("_key");
            }

            return a;
        }

        public ModelAutomobila PribaviModelAutomobila(string id)
        {
            var queryResult = db.Document.Get(id);

            ModelAutomobila a = new ModelAutomobila();

            foreach (var p in queryResult.Value)
            {
                a.Model = queryResult.Value.String("Model");
                a.Brend = queryResult.Value.String("Brend");
                a.Cena = queryResult.Value.String("Cena");
                a.Slika = queryResult.Value.String("Slika");
                a.Id = queryResult.Value.String("_id");
                //pribaviti broj dostupnih
            }

            return a;
        }

        //public Automobil PribaviAuto(string id)
        //{
        //    var queryResult = db.Document.Get(id);

        //    Automobil a = new Automobil();

        //    foreach (var p in queryResult.Value)
        //    {
        //        a.Kljuc = queryResult.Value.String("_key");
        //        a.Cena = queryResult.Value.String("Cena");
        //        a.Brend = queryResult.Value.String("Brend");
        //        a.Model = queryResult.Value.String("Model");
        //        a.Slika = queryResult.Value.String("Slika");
        //    }

        //    return a;
        //}

        #endregion

        #region SelectListItem
        private IList<SelectListItem> GetServise()
        {
            var sviServisi = db.Query
                .Aql(@"
                        FOR item IN servis
                            RETURN item
                        ")
                .ToDocuments();

            IList<SelectListItem> lista = new List<SelectListItem>();
            lista.Add(new SelectListItem
            {
                Text = "----Odaberi servis----",
                Value = "0"
            });

            foreach (var servis in sviServisi.Value)
            {
                lista.Add(new SelectListItem
                {
                    Text = servis["Vrsta"].ToString(),
                    Value = servis["_id"].ToString()
                });
            }

            return lista;
        }


        private IList<SelectListItem> GetAutomobile()
        {
            var sviAutomobili = db.Query
                .Aql(@"
                        FOR item IN automobili2
                            RETURN item
                        ")
                .ToDocuments();

            IList<SelectListItem> lista = new List<SelectListItem>();
            lista.Add(new SelectListItem
            {
                Text = "----Odaberi automobil----",
                Value = "0"
            });

            foreach (var servis in sviAutomobili.Value)
            {
                lista.Add(new SelectListItem
                {
                    Text = servis["Registracija"].ToString(),
                    Value = servis["_id"].ToString()
                });
            }

            return lista;
        }


        private IList<SelectListItem> GetModele()
        {
            List<ModelAutomobila> sviModeli = carController.PribaviSveModele();

            IList<SelectListItem> lista = new List<SelectListItem>();
            lista.Add(new SelectListItem
            {
                Text = "----Odaberi model----",
                Value = "0"
            });

            foreach (var servis in sviModeli)
            {
                lista.Add(new SelectListItem
                {
                    Text = servis.Brend + " " + servis.Model,
                    Value = servis.Key
                });
            }

            return lista;
        }

        public IList<SelectListItem> GetOgranke()
        {
            var sviOgranci = db.Query
                .Aql(@"
                        FOR item IN ogranak
                            RETURN item
                        ")
                .ToDocuments();

            IList<SelectListItem> lista = new List<SelectListItem>();
            lista.Add(new SelectListItem
            {
                Text = "----Odaberi ogranak----",
                Value = "0"
            });

            foreach (var ogranak in sviOgranci.Value)
            {
                lista.Add(new SelectListItem
                {
                    Text = ogranak["Naziv"].ToString(),
                    Value = ogranak["_id"].ToString()
                });
            }

            return lista;
        }

        #endregion


        [HttpGet]
        public ActionResult Servis()
        {
            ServisViewModel vm = new ServisViewModel();
            vm.SviAutomobili = GetAutomobile();
            vm.SviOgranci = GetOgranke();
            vm.SviModeli = GetModele();

            return View(vm);
        }



        [HttpPost]
        public ActionResult Servis(ServisViewModel vm)
        {
            var model = vm.OdabranModel;

            if (vm.OdabranModel != "0" && vm.OdabranModel != null)
            {
                var automobil = PribaviModelAutomobila("modeli/" + vm.OdabranModel);
                var auto = vm.OdabranModel;

                var queryResult = db.Query
                .BindVar("auto", "modeli/" + auto)
                .BindVar("graf", "organizacijaVoznogParka")
                .Aql(@"
                FOR part IN 1 OUTBOUND @auto GRAPH @graf
                RETURN part
                ")
                .ToDocuments();

                vm.SviAutomobili.Add(new SelectListItem
                {
                    Text = "----Odaberi automobil----",
                    Value = "0"
                });

                foreach (var item in queryResult.Value)
                {
                    vm.SviAutomobili.Add(new SelectListItem
                    {
                        Text = item["Registracija"].ToString(),
                        Value = item["_id"].ToString()
                    });
                }

                vm.SviModeli.Add(new SelectListItem
                {
                    Text = automobil.Brend + " " + automobil.Model,
                    Value = "1"
                });
            }

            else if (vm.OdabranAutomobil != "0" && vm.OdabranAutomobil != null)
            {
                var auto = vm.OdabranAutomobil;

                var queryResult = db.Query
                .BindVar("auto", auto)
                .BindVar("graf", "servis")
                .Aql(@"
                FOR part IN 1..4 OUTBOUND @auto GRAPH @graf
                RETURN part
                ")
                .ToDocuments();

                foreach (var item in queryResult.Value)
                {
                    vm.DetaljiServisa.Add(item);
                }
                vm.SviAutomobili = GetAutomobile();
                vm.SviModeli = GetModele();

            }
            else if (vm.OdabranOgranak != "0" && vm.OdabranOgranak != null)
            {
                var ogranak = vm.OdabranOgranak;

                var queryResult = db.Query
                .BindVar("ogranak", ogranak)
                .BindVar("graf", "servis")
                .Aql(@"
                FOR part IN 1..4 OUTBOUND @ogranak GRAPH @graf
                FILTER part.Ispravan == false
                RETURN part
                ")
                .ToDocuments();

                foreach (var item in queryResult.Value)
                {
                    vm.DetaljiServisa.Add(item);
                }

                vm.SviAutomobili = GetAutomobile();
                vm.SviModeli = GetModele();
            }
            else
            {
                var x = System.DateTime.Now;

                //pribavljanje automobila koji su neispravni nakon sto se zakljuci rent

                //FOR part IN korisnikRentiraoAutomobil
                //FILTER part.AutomobilIspravan == false
                //FOR automobil IN automobili
                //FILTER automobil._id == part._to
                //RETURN automobil

                var queryResult = db.Query
                    .Aql(@"
                FOR part IN korisnikRentirao
                FILTER part.AutomobilIspravan == false
                FOR automobil IN automobili2
                FILTER automobil._id == part._from
                RETURN part
                ")
                    .ToDocuments();

                foreach (var item in queryResult.Value)
                {
                    vm.DetaljiServisa.Add(item);
                }
                vm.SviAutomobili = GetAutomobile();
                vm.SviModeli = GetModele();
            }

            vm.SviOgranci = GetOgranke();

            return View(vm);
        }


        [Route("deleteCar/{id}")]
        public async Task<ActionResult> Delete(String id)
        {
            var deleteDocumentResult = db.Document
                    .Delete("automobili/" + id);

            return RedirectToAction("Index", "Home");
        }
        

        /*public ActionResult MojiRentoviOld()
        {
            //pribavljanje istorije rentiranja za korisnika
            IstorijaViewModel vm = new IstorijaViewModel();

            //FOR rent IN korisnikRezervisaoModel
            //FILTER rent._from == "Korisnik/606465"
            //RETURN rent
            var queryResult = db.Query
                     .BindVar("user", User.Identity.Name)
                     .Aql(@"
                            FOR item IN Korisnik
                                FILTER item.UserName == @user
                                RETURN item
                        ")
                     .ToDocument();

            var user = queryResult.Value.String("_id");

            var query = db.Query
                .BindVar("user", user)
                .Aql(@"
                    FOR rent IN korisnikRezervisaoModel
                        FILTER rent._from == @user
                        RETURN rent
                    ")
                .ToDocuments();

            foreach (var rent in query.Value)
            {
                Istorija i = new Istorija();
                i.IdAutomobila = rent["_to"].ToString();
                i.automobil = PribaviAuto(i.IdAutomobila);
                i.AdresaPreuzimanja = rent["AdresaPreuzimanja"].ToString();
                i.DatumPreuzimanja = rent["DatumPreuzimanja"].ToString();
                i.DatumVracanja = rent["DatumVracanja"].ToString();
                i.AdresaVracanja = rent["AdresaVracanja"].ToString();
                i.Kljuc = rent["_key"].ToString();
                i.listaUsluga = (List<object>)rent["DodatneUsluge"];
                i.Zatvoren = (bool)rent["Zatvoren"];
                i.UkupnoZaduzenje = Convert.ToInt32(rent["UkupnoZaduzenje"]);

                if (i.Zatvoren == true)
                {
                    i.AutomobilIspravan = (bool)rent["AutomobilIspravan"];
                    i.Beleske = rent["Beleske"].ToString();

                }

                vm.rentoviZaZatvaranje.Add(i);

            }
            return View(vm);
        }*/
    }
}