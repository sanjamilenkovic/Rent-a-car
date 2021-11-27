using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Arango.Client;
using RentACarWebApp.Models;
using System.Threading.Tasks;
using System.Web.Routing;
using RentACarWebApp.ViewModels;
using Microsoft.AspNet.Identity;

namespace RentACarWebApp.Controllers
{
    public class CarController : Controller
    {

        private ADatabase db;
        private List<DodatnaUsluga> sveDodatne;
        private HomeController homeController;
        public CarController()
        {
            ArangoDBContext dbContext = new ArangoDBContext();
            db = dbContext.ArangoDatabase;
            sveDodatne = PribaviSveDodatneUsluge();
        }

        //pocetna strana
        [HttpGet]
        public ActionResult ViewAll()
        {

            ViewAllViewModel vm = new ViewAllViewModel();
            
            vm.SviModeli = PribaviSveModele();
            vm.SveKlaseAutomobila = GetKlaseAutomobila();

            return View(vm);

        }



        //pocetna strana za filtriranje prikaza
        [HttpPost]
        public ActionResult ViewAll(ViewAllViewModel vm)
        {
            List<ModelAutomobila> listaRezultata = new List<ModelAutomobila>();

            if (vm.OdabraneKlaseAutomobila.Count > 0)
            {
                foreach (var klasa in vm.OdabraneKlaseAutomobila)
                {
                    //FOR part IN 1 OUTBOUND "klaseAutomobila/B"
                    //GRAPH "modelPripadaSegmentu"
                    //RETURN part

                    List<ModelAutomobila> probnaLista = new List<ModelAutomobila>();

                    var queryResult = db.Query
                        .BindVar("klasa", klasa)
                        .BindVar("graf", "organizacijaVoznogParka")
                        .Aql(@"
                        FOR part IN 1 OUTBOUND @klasa GRAPH @graf
                        RETURN part
                        ")
                        .ToDocuments();

                    foreach (var res in queryResult.Value)
                    {
                        ModelAutomobila m = new ModelAutomobila();
                        m.Id = res["_id"].ToString();
                        m.Key = res["_key"].ToString();
                        m.Brend = res["Brend"].ToString();
                        m.Model = res["Model"].ToString();
                        m.Slika = res["Slika"].ToString();
                        m.Cena = res["Cena"].ToString();
                        PribaviBrojDostupnih(m);
                        probnaLista.Add(m);
                    }

                    foreach(var res in probnaLista)
                    {
                        listaRezultata.Add(res);
                    }
                }
            }
            else
            {
                listaRezultata = PribaviSveModele();
            }

            vm.SviModeli = listaRezultata;
            vm.SveKlaseAutomobila = GetKlaseAutomobila();
            return View(vm);
        }


        public ActionResult SviModeli(string id)
        {
            var model = "modeli/" + id;
            var queryResult = db.Query
                .BindVar("model", model)
                .BindVar("graf", "organizacijaVoznogParka")

                .Aql(@"
                        FOR car IN 1 OUTBOUND @model GRAPH @graf
                        RETURN car
                        ")
                .ToDocuments();

            ViewAllViewModel vm = new ViewAllViewModel();

            foreach(var auto in queryResult.Value)
            {
                Automobil a = PribaviAuto(auto["_id"].ToString());
                vm.listaSvihAutomobila.Add(a);
            }

            return View(vm);
        }



        //pripremanje forme gde korisnik kreira rezervaciju
        [HttpGet]
        [Route("Car/Rent/{id}")]
        public ActionResult Rent(string id)
        {
            List<DodatnaUsluga> listaSvihUsluga = PribaviSveDodatneUsluge();
            var queryResult = db.Query
                .BindVar("id", id)
                .Aql(@"
                            FOR item IN modeli
                            FILTER item._key == @id 
                            RETURN item
                            ")
                .ToDocuments();

            ModelAutomobila m = new ModelAutomobila();

            foreach (var res in queryResult.Value)
            {
                m.Id = res["_id"].ToString();
                m.Brend = res["Brend"].ToString();
                m.Model = res["Model"].ToString();
                m.Key = res["_key"].ToString();
                m.Slika = res["Slika"].ToString();
                m.Cena = res["Cena"].ToString();
                PribaviBrojDostupnih(m);

            }

            RentViewModel rvm = new RentViewModel
            {
                ModelAutomobila = m,
                DostupneUsluge = GetUsluge(),
                SveDodatneUsluge = sveDodatne

            };



            return View(rvm);
        }


        //ovde korisnik kreira rezervaciju i ona se upisuje u bazu
        [HttpPost]
        public ActionResult Rent(RentViewModel rvm)
        {
            if (ModelState.IsValid)
            {
                //var cena = Int32.Parse(rvm.automobil.Cena);
                double ukupnaCena = 0;
                if(rvm.SelectedUsluge.Count >0)
                {
                    foreach(var usluga in rvm.SelectedUsluge)
                    {
                        foreach(var defusluga in sveDodatne)
                        {
                            if (defusluga.Id==usluga)
                            {
                                ukupnaCena += defusluga.Cena;
                            }
                        }
                    }
                }

                var razlika = rvm.DatumVracanja.Subtract(rvm.DatumPreuzimanja).TotalDays;
                ukupnaCena += razlika * Int32.Parse(rvm.ModelAutomobila.Cena);

                ukupnaCena *= rvm.BrojVozila;

                var dv = rvm.DatumVracanja.ToShortDateString();
                var dp = rvm.DatumPreuzimanja.ToShortDateString();
                var edgeData = new Dictionary<string, object>()
                    .String("AdresaPreuzimanja", rvm.AdresaPreuzimanja)
                    .String("DatumPreuzimanja", dp)
                    .String("AdresaVracanja", rvm.AdresaVracanja)
                    .String("DatumVracanja", dv)
                    .Bool("Obradjen", false)
                    .Int("BrojVozila", rvm.BrojVozila)
                    .String("UkupnoZaduzenje", ukupnaCena.ToString())
                    .Object("DodatneUsluge", rvm.SelectedUsluge);

                var car = rvm.ModelAutomobila.Key;

                var queryResult = db.Query
                         .BindVar("user", User.Identity.Name)
                         .Aql(@"
                            FOR item IN Korisnik
                                FILTER item.UserName == @user
                                RETURN item
                        ")
                         .ToDocument();

                var user = queryResult.Value.String("_id");

                var createEdgeResult = db.Document
                    .WaitForSync(true)
                    .CreateEdge("korisnikRezervisaoModel", user, "modeli/" + car, edgeData);

            }

            return RedirectToAction("ViewAll", "Car");


        }


        
        [HttpGet]
        public ActionResult New()
        {
            Automobil novi = new Automobil();

            NewCarViewModel novii = new NewCarViewModel();
            novii.SviOgranci = GetKlaseAutomobila();

            return View(novii);
        }

        [HttpPost]
        public ActionResult New(NewCarViewModel a, FormCollection c)
        {
            a.Automobil.Slika = c.GetValue("imageInput").AttemptedValue;

            var document = new Dictionary<string, object>()
            .String("_key", a.Automobil.Kljuc)
            .String("Brend", a.Automobil.Brend)
            .String("Model", a.Automobil.Model)
            .String("Menjac", a.Automobil.Menjac)
            .String("BrojSedista", a.Automobil.BrojSedista)
            .Int("Cena", 50)
            .String("Gorivo", a.Automobil.Gorivo)
            .String("Slika", a.Automobil.Slika);


            if (a.Automobil.BrojVrata != null)
                document.Add("BrojVrata", a.Automobil.BrojVrata);

            if (a.Automobil.Pogon != null)
                document.Add("Pogon", a.Automobil.Pogon);

            if (a.Automobil.Snaga != null)
                document.Add("Snaga", a.Automobil.Snaga);

            if (a.Automobil.ZapreminaGepeka != null)
                document.Add("ZapreminaGepeka", a.Automobil.ZapreminaGepeka);


            var createDocumentResult = db.Document
                .WaitForSync(true)
                .Create("automobili/2", document);

            var id = a.OdabranOgranak;


            return RedirectToAction("ViewAll", "Car");
        }



#region Pribavljanje svasta
public List<DodatnaUsluga> PribaviSveDodatneUsluge()
{
    List<DodatnaUsluga> sveUsluge = new List<DodatnaUsluga>();

    var queryResult = db.Query
        .Aql(@"
                    FOR item IN dodatneUsluge 
                        RETURN item
                    ")
        .ToDocuments();

    foreach (var usluga in queryResult.Value)
    {
        DodatnaUsluga d = new DodatnaUsluga
        {
            Cena = Convert.ToInt32(usluga["Cena"]),
            Naziv = usluga["Vrsta"].ToString(),
            Id = usluga["_id"].ToString(),
            IsSelected = false
        };

        sveUsluge.Add(d);
    }

    return sveUsluge;
}

public void PribaviBrojDostupnih(ModelAutomobila m)
{

            //RETURN MINUS(
            //(FOR car IN 1 OUTBOUND "modeli/renault-clio" GRAPH "rentiranje"
            //return car._id),
            //(FOR car IN korisnikRentirao
            //return car._from)
            //)

            var queryResult2 = db.Query
                .BindVar("model", m.Id)
                .BindVar("graf", "rentiranje")
                .Aql(@"
                            FOR x IN MINUS(
                            (FOR car IN 1 OUTBOUND @model GRAPH @graf
                            return car._id),
                            (FOR car IN korisnikRentirao
                            FILTER car.Zatvoren == false
                            return car._from)
                            ) RETURN x")
                .ToList<String>();

    var brojDostupnih = queryResult2.Value.Count;

            //FOR car IN korisnikRezervisaoModel
            //FILTER car._to == "modeli/renault-clio"
            //return car
            var brojRezervisanih = 0;
            var queryResult3 = db.Query
                .BindVar("model", m.Id)
                .Aql(@"
                    FOR car IN korisnikRezervisaoModel
                    FILTER car._to == @model
                    RETURN car
                            ")
                .ToDocuments();
            foreach (var item in queryResult3.Value)
            {
                brojRezervisanih = Int32.Parse(item["BrojVozila"].ToString());
            }
            m.BrojDostupnih = brojDostupnih-  brojRezervisanih;

            var q = db.Query
                .BindVar("model", m.Id)
                .BindVar("graf", "rentiranje")
                .Aql(@"
                    FOR car IN 1 OUTBOUND @model GRAPH @graf
                    return car._id
                            ")
                .ToList<string>();
            m.UkupanBrojModela = q.Value.Count;
        }

public List<ModelAutomobila> PribaviSveModele()
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

        PribaviBrojDostupnih(m);

        if (m.BrojDostupnih > 0)
            sviModeli.Add(m);

    }
    return sviModeli;

}

public Automobil PribaviAuto(string id)
{
    var queryResult = db.Document.Get(id);

    Automobil a = new Automobil();

    foreach (var p in queryResult.Value)
    {
        a.Kljuc = queryResult.Value.String("_key");
        a.Brend = queryResult.Value.String("Brend");
        a.Model = queryResult.Value.String("Model");
        a.Cena = queryResult.Value.String("Cena");
        a.Slika = queryResult.Value.String("Slika");
        a.Registracija = queryResult.Value.String("Registracija");

        if (p.Key != "_id" && p.Key != "_key" && p.Key != "_rev" && p.Key != "Slika" && p.Key != "sviAtributi")
        {
            a.sviAtributi.Add(p.Key, p.Value);

        }
    }

    return a;
}

public List<Automobil> PribaviSveAutomobile(AResult<List<Dictionary<string, object>>> queryResult)
{
    List<Automobil> lista = new List<Automobil>();
    foreach (var automobil in queryResult.Value) //automobil = Dictionary<string, object>
    {
        Automobil a = new Automobil();
        a.Kljuc = automobil["_key"].ToString();
        a.Model = automobil["Model"].ToString();
        a.Brend = automobil["Brend"].ToString();
        a.Gorivo = automobil["Gorivo"].ToString();
        a.Slika = automobil["Slika"].ToString();
        a.Cena = automobil["Cena"].ToString();
        a.Menjac = automobil["Menjac"].ToString();
        a.BrojSedista = automobil["BrojSedista"].ToString();

        lista.Add(a);
    }
    return lista;
}

[Route("/Car/ViewSingleCar/{id}")]
public ActionResult ViewSingleCar(string id)
{
    string id2 = "automobili2/" + id;

    Automobil a = PribaviAuto(id2);
    return View(a);
}


#endregion


#region SelectListItem
private IList<SelectListItem> GetUsluge()
{
    IList<SelectListItem> lista = new List<SelectListItem>();


    foreach (var usluga in sveDodatne)
    {
        lista.Add(new SelectListItem { Text = usluga.Naziv, Value = usluga.Id });
    }

    return lista;
}

private IList<int> GetListuCena()
{
    List<DodatnaUsluga> ponudaUsluga = PribaviSveDodatneUsluge();
    IList<int> listaCena = new List<int>();


    foreach (var usluga in ponudaUsluga)
    {
        listaCena.Add(usluga.Cena);
    }

    return listaCena;
}

private IList<SelectListItem> GetKlaseAutomobila()
{  
    var sviOgranci = db.Query
        .Aql(@"
            FOR item IN klaseAutomobila
                RETURN item
            ")
        .ToDocuments();

    IList<SelectListItem> lista = new List<SelectListItem>();

    //lista.Add(new SelectListItem
    //{
    //    Text = "Svi segmenti",
    //    Value = "0"
    //});

    foreach (var ogranak in sviOgranci.Value)
    {
        lista.Add(new SelectListItem
        {
            Text = ogranak["tip"].ToString(),
            Value = ogranak["_id"].ToString()
        });
    }

    return lista;
}
#endregion



/*




[HttpGet]
public ActionResult Edit(String id)
{
var automobil = db.Document.Get("automobili/" + id);
CarViewModel cvm = new CarViewModel();

foreach (var x in automobil.Value)
{
if (x.Key != "_rev" && x.Key != "_id" && x.Key != "Dodatna oprema")
    cvm.sviAtributiAutomobila.Add(x.Key, x.Value.ToString());
}

return View(cvm);
}

[HttpPost]
public ActionResult Edit(CarViewModel a)
{
Dictionary<string, object> novi = new Dictionary<String, object>();
var id = a.sviAtributiAutomobila["_key"];

foreach (var x in a.sviAtributiAutomobila)
{
if (x.Key != "_id")
    novi.Add(x.Key, x.Value);
}

var createDocumentResult = db.Document
.Update("automobili/" + id, novi);

return RedirectToAction("ViewAll", "Home");
}
*/
    }
}