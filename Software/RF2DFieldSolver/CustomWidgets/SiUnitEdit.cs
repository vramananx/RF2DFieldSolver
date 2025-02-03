using System;
using System.Windows.Forms;

namespace RF2DFieldSolver.CustomWidgets
{
    public class SiUnitEdit : TextBox
    {
        private string unit;
        private string prefixes;
        private int precision;
        private double value;

        public SiUnitEdit(string unit = "", string prefixes = " ", int precision = 0)
        {
            this.unit = unit;
            this.prefixes = prefixes;
            this.precision = precision;
            this.TextAlign = HorizontalAlignment.Center;
            this.Leave += (sender, e) => ParseNewValue(1.0);
            SetValueQuiet(0);
        }

        public void SetUnit(string unit)
        {
            this.unit = unit;
            SetValueQuiet(value);
        }

        public void SetPrefixes(string prefixes)
        {
            this.prefixes = prefixes;
            SetValueQuiet(value);
        }

        public void SetPrecision(int precision)
        {
            this.precision = precision;
            SetValueQuiet(value);
        }

        public double GetValue()
        {
            return value;
        }

        public void SetValue(double value)
        {
            if (value != this.value)
            {
                SetValueQuiet(value);
                OnValueChanged(EventArgs.Empty);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == (char)Keys.Escape)
            {
                Clear();
                SetValueQuiet(value);
                OnEditingAborted(EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Return)
            {
                ParseNewValue(1.0);
                ContinueEditing();
                e.Handled = true;
            }
            else if (prefixes.IndexOf(e.KeyChar) >= 0)
            {
                ParseNewValue(SiPrefixToFactor(e.KeyChar));
                ContinueEditing();
                e.Handled = true;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (value == 0.0)
            {
                return;
            }

            int increment = e.Delta / 120;
            int steps = Math.Abs(increment);
            int sign = increment > 0 ? 1 : -1;
            double newVal = value;

            if (Focused)
            {
                int cursor = SelectionStart;
                if (cursor == 0)
                {
                    return;
                }

                int nthDigit = cursor;
                if (value < 0)
                {
                    nthDigit--;
                }

                int dotPos = Text.IndexOf('.');
                if (dotPos >= 0 && dotPos < nthDigit)
                {
                    nthDigit--;
                }

                if (Text.StartsWith("-0.") || Text.StartsWith("0."))
                {
                    nthDigit--;
                }

                double stepSize = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
                newVal += stepSize * steps * sign;
                SetValue(newVal);
                Text = PlaceholderText;
                SelectionStart = cursor;
            }
            else
            {
                const int nthDigit = 3;
                while (steps > 0)
                {
                    double stepSize = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
                    newVal += stepSize * sign;
                    steps--;
                }
                SetValue(newVal);
            }
        }

        private void SetValueQuiet(double value)
        {
            this.value = value;
            PlaceholderText = ToString(value, unit, prefixes, precision);
            if (!string.IsNullOrEmpty(Text))
            {
                ContinueEditing();
            }
        }

        private void ParseNewValue(double factor)
        {
            string input = Text;
            if (string.IsNullOrEmpty(input))
            {
                SetValueQuiet(value);
                OnEditingAborted(EventArgs.Empty);
            }
            else
            {
                if (input.EndsWith(unit))
                {
                    input = input.Substring(0, input.Length - unit.Length);
                }

                char lastChar = input[input.Length - 1];
                if (prefixes.IndexOf(lastChar) >= 0)
                {
                    factor = SiPrefixToFactor(lastChar);
                    input = input.Substring(0, input.Length - 1);
                }

                if (double.TryParse(input, out double parsedValue))
                {
                    Clear();
                    SetValue(parsedValue * factor);
                }
                else
                {
                    Console.WriteLine("SIUnit conversion failure: " + input);
                }
            }
        }

        private void ContinueEditing()
        {
            Text = PlaceholderText;
            SelectAll();
        }

        private double SiPrefixToFactor(char prefix)
        {
            switch (prefix)
            {
                case 'p': return 1e-12;
                case 'n': return 1e-9;
                case 'u': return 1e-6;
                case 'm': return 1e-3;
                case 'k': return 1e3;
                case 'M': return 1e6;
                case 'G': return 1e9;
                default: return 1.0;
            }
        }

        private string ToString(double value, string unit, string prefixes, int precision)
        {
            string format = "F" + precision;
            return value.ToString(format) + unit;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnEditingAborted(EventArgs e)
        {
            EditingAborted?.Invoke(this, e);
        }

        public event EventHandler ValueChanged;
        public event EventHandler EditingAborted;
    }
}
