using Xunit;

namespace CoffeeMachine.Tests
{
    public class DecafUnitTests
    {
        public static TheoryData<string, string> Brew_Data()
        {
            return new TheoryData<string, string>
            {
                // [Keywords] Should convert
                // TODO: Everything at https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
                { "abstract", "abstract" },
                { "assert expression()", "Debug.Assert(Expression())" },
                { "boolean", "bool" },
                { "break", "break" },
                { "byte", "byte" },
                { "continue", "continue" },
                { "extends", ":" },
                { "final", "readonly" },
                { "implements", ":" },
                { "instanceof", "is" },
                { "int", "int" },
                { "long", "long" },
                { "native", "extern" },
                { "short", "short" },
                { "super", "base" },
                { "synchronized", "lock" },

                // [Method invocations] Generic: Place type arguments after method name
                { "Foo.<String, Object>bar()", "Foo.Bar<string, object>()" },

                // [Getters/setters] Should convert
                { "this.getCurrentItem()", "this.CurrentItem" },
                { "this.setCurrentItem(value)", "this.CurrentItem = value" },

                // [Getters/setters] Method name is just 'get'/'set': should not convert
                { "this.get()", "this.Get()" },
                { "this.set(value)", "this.Set(value)" },

                // [Getters/setters] Generic: should not convert
                { "Foo.<String>getBar()", "Foo.GetBar<string>()" },
                { "Foo.<String>setBar(value)", "Foo.SetBar<string>(value)" },
            };
        }

        [Theory]
        [MemberData(nameof(Brew_Data))]
        public void Brew(string javaCode, string expectedCSharpCode)
        {
            var actualCSharpCode = Decaf.Brew(javaCode);
            Assert.Equal(expectedCSharpCode, actualCSharpCode);
        }
    }
}