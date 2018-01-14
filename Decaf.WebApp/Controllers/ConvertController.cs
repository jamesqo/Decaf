using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Decaf.WebApp.Controllers
{
    public class ConvertController : Controller
    {
        [HttpGet]
        public string Get(
            string javaCode)
        {
            // TODO: Unqualified name
            return CoffeeMachine.Decaf.Brew(javaCode);
        }
    }
}
