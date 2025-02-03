using System;
using System.Windows.Forms;
using System.Globalization;

public class SIUnitEdit : TextBox
{
    private string unit;
    private string prefixes;
    private int precision;
    private double _value;

    public SIUnitEdit(string unit = "", string prefixes = " ", int precision = 0)
    {
        this.unit = unit;
        this.prefixes = prefixes;
        this.precision = precision;
        this.TextAlign = HorizontalAlignment.Center;
        this.LostFocus += (sender, e) => ParseNewValue(1.0);
        SetValueQuiet(0);
    }

    public void SetUnit(string unit)
    {
        this.unit = unit;
        SetValueQuiet(_value);
    }

    public void SetPrefixes(string prefixes)
    {
        this.prefixes = prefixes;
        SetValueQuiet(_value);
    }

    public void SetPrecision(int precision)
    {
        this.precision = precision;
        SetValueQuiet(_value);
    }

    public double Value
    {
        get { return _value; }
    }

    public void SetValue(double value)
    {
        if (value != _value)
        {
            SetValueQuiet(value);
            OnValueChanged(value);
            OnValueUpdated(this);
        }
    }

    public event EventHandler<double> ValueChanged;
    public event EventHandler ValueUpdated;
    public event EventHandler EditingAborted;
    public event EventHandler FocusLost;

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (e.KeyChar == (char)Keys.Escape)
        {
            Clear();
            SetValueQuiet(_value);
            OnEditingAborted();
            this.Parent.Focus();
            e.Handled = true;
        }
        else if (e.KeyChar == (char)Keys.Return)
        {
            ParseNewValue(1.0);
            ContinueEditing();
            e.Handled = true;
        }
        else if (prefixes.Contains(e.KeyChar.ToString()))
        {
            ParseNewValue(Unit.SIPrefixToFactor(e.KeyChar));
            ContinueEditing();
            e.Handled = true;
        }
        else if (prefixes.Contains(SwapUpperLower(e.KeyChar).ToString()))
        {
            ParseNewValue(Unit.SIPrefixToFactor(SwapUpperLower(e.KeyChar)));
            ContinueEditing();
            e.Handled = true;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_value == 0.0)
        {
            return;
        }

        int increment = e.Delta / 120;
        int steps = Math.Abs(increment);
        int sign = increment > 0 ? 1 : -1;
        double newVal = _value;

        if (this.Focused)
        {
            int cursor = this.SelectionStart;
            if (cursor == 0)
            {
                return;
            }

            int nthDigit = cursor;
            if (_value < 0)
            {
                nthDigit--;
            }

            int dotPos = this.Text.IndexOf('.');
            if (dotPos >= 0 && dotPos < nthDigit)
            {
                nthDigit--;
            }

            if (this.Text.StartsWith("-0.") || this.Text.StartsWith("0."))
            {
                nthDigit--;
            }

            double step_size = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
            newVal += step_size * steps * sign;
            SetValue(newVal);
            this.Text = this.PlaceholderText;
            this.SelectionStart = cursor;
        }
        else
        {
            const int nthDigit = 3;
            while (steps > 0)
            {
                double step_size = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
                newVal += step_size * sign;
                steps--;
            }
            SetValue(newVal);
        }
    }

    private void SetValueQuiet(double value)
    {
        _value = value;
        this.PlaceholderText = Unit.ToString(value, unit, prefixes, precision);
        if (!string.IsNullOrEmpty(this.Text))
        {
            ContinueEditing();
        }
    }

    private void ParseNewValue(double factor)
    {
        string input = this.Text;
        if (string.IsNullOrEmpty(input))
        {
            SetValueQuiet(_value);
            OnEditingAborted();
        }
        else
        {
            if (input.EndsWith(unit))
            {
                input = input.Substring(0, input.Length - unit.Length);
            }

            char lastChar = input[input.Length - 1];
            if (prefixes.Contains(lastChar.ToString()))
            {
                factor = Unit.SIPrefixToFactor(lastChar);
                input = input.Substring(0, input.Length - 1);
            }

            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
                Clear();
                SetValue(v * factor);
            }
            else
            {
                Console.WriteLine("SIUnit conversion failure: " + input);
            }
        }
    }

    private void ContinueEditing()
    {
        this.Text = this.PlaceholderText;
        this.SelectAll();
    }

    private static char SwapUpperLower(char c)
    {
        if (char.IsUpper(c))
        {
            return char.ToLower(c);
        }
        else if (char.IsLower(c))
        {
            return char.ToUpper(c);
        }
        else
        {
            return c;
        }
    }

    protected virtual void OnValueChanged(double newValue)
    {
        ValueChanged?.Invoke(this, newValue);
    }

    protected virtual void OnValueUpdated(EventArgs e)
    {
        ValueUpdated?.Invoke(this, e);
    }

    protected virtual void OnEditingAborted()
    {
        EditingAborted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnFocusLost()
    {
        FocusLost?.Invoke(this, EventArgs.Empty);
    }
}

public static class Unit
{
    public static double FromString(string str, string unit, string prefixes)
    {
        if (string.IsNullOrEmpty(str))
        {
            return double.NaN;
        }

        if (str.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
        {
            str = str.Substring(0, str.Length - unit.Length);
        }

        double factor = 1.0;
        if (prefixes.Contains(str[str.Length - 1].ToString()))
        {
            char prefix = str[str.Length - 1];
            factor = SIPrefixToFactor(prefix);
            str = str.Substring(0, str.Length - 1);
        }

        if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value * factor;
        }
        else
        {
            return double.NaN;
        }
    }

    public static string ToString(double value, string unit, string prefixes, int precision)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return "NaN";
        }
        else if (Math.Abs(value) <= double.Epsilon)
        {
            return "0 " + unit;
        }
        else
        {
            string sValue = value < 0 ? "-" : "";
            value = Math.Abs(value);

            int prefixIndex = 0;
            int preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            while (preDotDigits > 3 && prefixIndex < prefixes.Length - 1)
            {
                prefixIndex++;
                preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            }

            value /= SIPrefixToFactor(prefixes[prefixIndex]);
            sValue += value.ToString("F" + Math.Max(0, precision - preDotDigits), CultureInfo.InvariantCulture);
            sValue += prefixes[prefixIndex] + unit;

            return sValue;
        }
    }

    public static double SIPrefixToFactor(char prefix)
    {
        switch (prefix)
        {
            case 'f': return 1e-15;
            case 'p': return 1e-12;
            case 'n': return 1e-9;
            case 'u': return 1e-6;
            case 'm': return 1e-3;
            case ' ': return 1e0;
            case 'k': return 1e3;
            case 'M': return 1e6;
            case 'G': return 1e9;
            case 'T': return 1e12;
            case 'P': return 1e15;
            default: return 1e0;
        }
    }
}
