using System.Diagnostics;
using Xunit;

namespace CoffeeMachine.Tests
{
    public abstract class DecafTestsBase
    {
        protected void CommonBrew(string javaCode, string expectedCSharpCode)
        {
            try
            {
                var actualCSharpCode = Decaf.Brew(javaCode);
                Assert.Equal(expectedCSharpCode, actualCSharpCode);
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Debugger.Break();
                        var actualCSharpCode = Decaf.Brew(javaCode);
                        Debugger.Break();
                    }
                }

                throw;
            }
        }
    }
}
