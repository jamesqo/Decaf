using System.Collections.Generic;
using Xunit;

namespace CoffeeMachine.Tests
{
    public class UnitTests
    {
        public static IEnumerable<object[]> Brew_Data()
        {
            // TODO: Everything at https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            yield return new object[] { "public abstract class C { }", "public abstract class C { }" };
            yield return new object[] { "assert foo != null;", "Debug.Assert(foo != null);" };
            yield return new object[] { "boolean", "bool" };
            yield return new object[] { "break;", "break;" };
            yield return new object[] { "byte", "byte" };
            yield return new object[] { "class C extends B { }", "class C : B { }" };
            yield return new object[] { "class C implements I { }", "class C : I { }" };
            yield return new object[] { "int", "int" };
            yield return new object[] { "long", "long" };
            yield return new object[] { "short", "short" };
            // TODO: Move to E2E.
            yield return new object[]
            {
                @"
switch (foo) {
    case 1:
        bar();
    case 2:
        baz();
    default:
        bag();
}",

                @"
switch (foo)
{
    case 1:
        Bar();
        goto case 2;
    case 2:
        Baz();
        goto default;
    default:
        bag();
        break;
}"
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