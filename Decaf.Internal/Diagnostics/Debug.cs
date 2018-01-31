using System;
using System.Diagnostics;

namespace CoffeeMachine.Internal.Diagnostics
{
    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message = "[No message provided]")
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            throw new DebugAssertException(GetFullMessage(message));
        }

        private static string GetFullMessage(string message)
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Assertion failed: {message}",
                Environment.StackTrace
            });
        }
    }

    internal class DebugAssertException : Exception
    {
        public DebugAssertException(string fullMessage)
            : base(fullMessage)
        {
        }
    }
}
