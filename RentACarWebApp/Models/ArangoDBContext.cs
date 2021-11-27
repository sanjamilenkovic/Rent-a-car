using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Arango.Client;

namespace RentACarWebApp.Models
{
    public class ArangoDBContext
    {
        public ADatabase ArangoDatabase { get; set; }

        public ArangoDBContext()
        {
            if (!ASettings.HasConnection("rent-a-carDB"))
            {                 // adds new connection data to database manager
                ASettings.AddConnection(
                    "rent-a-carDB",
                    "127.0.0.1",
                    8529,
                    false,
                    "rent-a-car",
                    "root",
                    "root"
                );
            }
            this.ArangoDatabase = new ADatabase("rent-a-carDB");

        }

    }
}