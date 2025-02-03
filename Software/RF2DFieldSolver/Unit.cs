using System;
using System.Globalization;

namespace RF2DFieldSolver
{
    public static class Unit
    {
        public static double FromString(string input, string unit = "", string prefixes = " ")
        {
            if (string.IsNullOrEmpty(input))
            {
                return double.NaN;
            }

            // Remove unit if present
            if (input.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(0, input.Length - unit.Length);
            }

            // Check if last char is a valid prefix
            double factor = 1.0;
            if (prefixes.Contains(input[input.Length - 1]))
            {
                char prefix = input[input.Length - 1];
                factor = SIPrefixToFactor(prefix);
                input = input.Substring(0, input.Length - 1);
            }

            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return value * factor;
            }

            return double.NaN;
        }

        public static string ToString(double value, string unit = "", string prefixes = " ", int precision = 6)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "NaN";
            }
            else if (Math.Abs(value) <= double.Epsilon)
            {
                return "0 " + unit;
            }

            string result = value < 0 ? "-" : "";
            value = Math.Abs(value);

            int prefixIndex = 0;
            int preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            while (preDotDigits > 3 && prefixIndex < prefixes.Length - 1)
            {
                prefixIndex++;
                preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            }

            value /= SIPrefixToFactor(prefixes[prefixIndex]);
            result += value.ToString("F" + Math.Max(0, precision - preDotDigits), CultureInfo.InvariantCulture);
            result += prefixes[prefixIndex] + unit;

            return result;
        }

        private static double SIPrefixToFactor(char prefix)
        {
            return prefix switch
            {
                'f' => 1e-15,
                'p' => 1e-12,
                'n' => 1e-9,
                'u' => 1e-6,
                'm' => 1e-3,
                ' ' => 1e0,
                'k' => 1e3,
                'M' => 1e6,
                'G' => 1e9,
                'T' => 1e12,
                'P' => 1e15,
                _ => 1e0,
            };
        }
    }
}
