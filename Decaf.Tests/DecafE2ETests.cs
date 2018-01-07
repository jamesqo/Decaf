using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace CoffeeMachine.Tests
{
    public class DecafE2ETests : DecafTestsBase
    {
        private static string GetThisFilePath([CallerFilePath] string path = null)
        {
            return path;
        }

        public static IEnumerable<object[]> Brew_Data()
        {
            var thisFile = GetThisFilePath();
            var testDataDirectory = Path.Combine(Path.GetDirectoryName(thisFile), "TestData");
            var testFiles = Directory.EnumerateFiles(testDataDirectory).ToHashSet();

            for (int i = 1; ; i++)
            {
                string inFile = Path.Combine(testDataDirectory, $"Test{i}-in.java");
                string outFile = Path.Combine(testDataDirectory, $"Test{i}-out.cs");

                if (!testFiles.Contains(inFile) || !testFiles.Contains(outFile))
                {
                    break;
                }

                yield return new object[] { File.ReadAllText(inFile), File.ReadAllText(outFile) };
            }
        }

        [Theory]
        [MemberData(nameof(Brew_Data))]
        public void Brew(string javaCode, string expectedCSharpCode)
        {
            CommonBrew(javaCode, expectedCSharpCode);
        }
    }
}