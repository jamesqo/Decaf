using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;

namespace CoffeeMachine.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        [HttpGet]
        public string Get(
            string javaCode,
            LanguageVersion csharpLanguageVersion,
            IndentationStyle indentationStyle,
            bool translateCollectionTypes,
            bool unqualifyTypeNames,
            bool useVarInDeclarations)
        {
            try
            {
                var options = new BrewOptions
                {
                    CSharpLanguageVersion = csharpLanguageVersion,
                    IndentationStyle = indentationStyle,
                    TranslateCollectionTypes = translateCollectionTypes,
                    UnqualifyTypeNames = unqualifyTypeNames,
                    UseVarInDeclarations = useVarInDeclarations
                };
                return Decaf.Brew(javaCode, options);
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
