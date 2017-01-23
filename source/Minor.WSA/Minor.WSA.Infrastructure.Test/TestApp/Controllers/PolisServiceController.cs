using Minor.WSA.Infrastructure.Test.TestApp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.TestApp.Controllers
{
    //[Microservice("Endpointnaam")]
    /// Endpointnaam defaults to "PolisService", from <<endpointnaam>>Controller
    public class PolisServiceController : IPolisService
    {
        //[Handles("MVM.PolisService.RegisteerPolisCommand")]
        public void RegistreerPolis(RegisteerPolisCommand command)
        {

        }

        public void NoCommandHandler(string name)
        {

        }
    }
}
