using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp;

namespace CoffeeMachine.WebApp.Binders
{
    public class LanguageVersionModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType != typeof(LanguageVersion))
            {
                return null;
            }

            return new LanguageVersionModelBinder();
        }
    }
}
