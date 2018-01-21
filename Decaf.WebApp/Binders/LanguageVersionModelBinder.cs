using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp;

namespace CoffeeMachine.WebApp.Binders
{
    public class LanguageVersionModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            string input = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(input))
            {
                return Task.CompletedTask;
            }

            LanguageVersion output = ParseLanguageVersion(input);
            bindingContext.Result = ModelBindingResult.Success(output);
            return Task.CompletedTask;
        }

        private static LanguageVersion ParseLanguageVersion(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = text.ToLowerInvariant();
            switch (text)
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
                    throw new ArgumentException($"Unrecognized language version: {text}", nameof(text));
            }
        }
    }
}
