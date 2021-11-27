using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using ArangoDB.AspNet.Identity;

namespace RentACarWebApp.Models
{
    // You can add profile data for the user by adding more properties to your Korisnik class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class Korisnik : IdentityUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Grad { get; set; }
        public string BrojTelefona { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }



        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Korisnik> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

}