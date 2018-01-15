using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeMachine.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        [HttpGet]
        public string Get(string javaCode)
        {
            return Decaf.Brew(javaCode);
        }
    }
}
