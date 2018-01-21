using System;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeMachine.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        [HttpGet]
        public string Get(string javaCode)
        {
            try
            {
                return Decaf.Brew(javaCode);
            }
            catch (Exception e)
            {
                return string.Join(Environment.NewLine, new[]
                {
                    "Unable to convert code because an exception occurred.",
                    $"Exception type: {e.GetType()}",
                    $"Exception message: {e.Message}",
                    "Stack trace:",
                    e.StackTrace
                });
            }
        }
    }
}
