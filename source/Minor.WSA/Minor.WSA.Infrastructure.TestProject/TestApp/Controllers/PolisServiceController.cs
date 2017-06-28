using MVM.Polisbeheer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVM.Polisbeheer.Controllers
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
