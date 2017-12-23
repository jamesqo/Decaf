using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoffeeMachine.Internal.Diagnostics
{
    internal static class D
    {
        [Conditional("DEBUG")]
        public static void AssertEqual<T>(T actual, T expected)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                string message = "Actual: " + actual + Environment.NewLine
                    + "Expected: " + expected;
                Fail(message);
            }
        }

        [Conditional("DEBUG")]
        public static void AssertIsOneOf<T>(T actual, params T[] expected)
        {
            if (Array.IndexOf(expected, actual) == -1)
            {
                string message = "Actual: " + actual + Environment.NewLine
                    + "Expected: " + string.Join(" or ", expected);
                Fail(message);
            }
        }

        [Conditional("DEBUG")]
        public static void AssertNotNull<T>(T actual)
            where T : class
        {
            if (actual == null)
            {
                Fail("Value was null.");
            }
        }

        [Conditional("DEBUG")]
        public static void AssertTrue(bool condition, string message = "[No message provided]")
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            throw new DAssertException(GetFullMessage(message));
        }

        private static string GetFullMessage(string message)
        {
            return "D.Assert failed!" + Environment.NewLine
                + message + Environment.NewLine
                + Environment.StackTrace;
        }
    }

    internal class DAssertException : Exception
    {
        public DAssertException(string fullMessage)
            : base(fullMessage)
        {
        }
    }
}