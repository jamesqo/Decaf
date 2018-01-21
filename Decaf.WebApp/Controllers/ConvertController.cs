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
            string csharpLanguageVersion,
            bool translateCollectionTypes,
            bool unqualifyTypeNames,
            bool useVarInDeclarations)
        {
            try
            {
                var options = new BrewOptions
                {
                    CSharpLanguageVersion = ParseLanguageVersion(csharpLanguageVersion),
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

        private static LanguageVersion ParseLanguageVersion(string versionText)
        {
            switch (versionText)
            {
                case "1":
                    return LanguageVersion.CSharp1;
                case "2":
                    return LanguageVersion.CSharp2;
                case "3":
                    return LanguageVersion.CSharp3;
                case "4":
                    return LanguageVersion.CSharp4;
                case "5":
                    return LanguageVersion.CSharp5;
                case "6":
                    return LanguageVersion.CSharp6;
                case "7":
                    return LanguageVersion.CSharp7;
                case "7.1":
                    return LanguageVersion.CSharp7_1;
                case "7.2":
                    return LanguageVersion.CSharp7_2;
                case "latest":
                    return LanguageVersion.Latest;
                default:
                    throw new ArgumentException($"Unrecognized language version: {versionText}", nameof(versionText));
            }
        }
    }
}
