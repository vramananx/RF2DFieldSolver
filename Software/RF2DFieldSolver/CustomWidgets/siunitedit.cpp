#include "siunitedit.h"

#include <System.Windows.Forms.h>
#include <System.ComponentModel.h>
#include <cmath>

SIUnitEdit::SIUnitEdit(System::String^ unit, System::String^ prefixes, int precision, System::Windows::Forms::Form^ parent)
    : TextBox()
{
    this->unit = unit;
    this->prefixes = prefixes;
    this->precision = precision;
    this->TextAlign = System::Windows::Forms::HorizontalAlignment::Center;
    this->Parent = parent;
    this->LostFocus += gcnew System::EventHandler(this, &SIUnitEdit::OnLostFocus);
    this->KeyPress += gcnew System::Windows::Forms::KeyPressEventHandler(this, &SIUnitEdit::OnKeyPress);
    this->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &SIUnitEdit::OnKeyDown);
    this->MouseWheel += gcnew System::Windows::Forms::MouseEventHandler(this, &SIUnitEdit::OnMouseWheel);
    this->TextChanged += gcnew System::EventHandler(this, &SIUnitEdit::OnTextChanged);
    setValueQuiet(0);
}

SIUnitEdit::SIUnitEdit(System::Windows::Forms::Form^ parent)
    : SIUnitEdit("", " ", 4, parent)
{
}

void SIUnitEdit::setValue(double value)
{
    if(value != _value) {
        setValueQuiet(value);
        ValueChanged(this, System::EventArgs::Empty);
        ValueUpdated(this, System::EventArgs::Empty);
    }
}

static char swapUpperLower(char c) {
    if(isupper(c)) {
        return tolower(c);
    } else if(islower(c)) {
        return toupper(c);
    } else {
        return c;
    }
}

void SIUnitEdit::OnKeyPress(System::Object^ sender, System::Windows::Forms::KeyPressEventArgs^ e)
{
    if (e->KeyChar == (char)System::Windows::Forms::Keys::Escape) {
        // abort editing process and set old value
        this->Clear();
        setValueQuiet(_value);
        EditingAborted(this, System::EventArgs::Empty);
        this->Parent->Focus();
        e->Handled = true;
    }
    else if (e->KeyChar == (char)System::Windows::Forms::Keys::Return) {
        // use new value without prefix
        parseNewValue(1.0);
        continueEditing();
        e->Handled = true;
    }
    else if (prefixes->IndexOf(e->KeyChar) >= 0) {
        // a valid prefix key was pressed
        parseNewValue(Unit::SIPrefixToFactor(e->KeyChar));
        continueEditing();
        e->Handled = true;
    }
    else if (prefixes->IndexOf(swapUpperLower(e->KeyChar)) >= 0) {
        // no match on the pressed case but on the upper/lower case instead -> also accept this
        parseNewValue(Unit::SIPrefixToFactor(swapUpperLower(e->KeyChar)));
        continueEditing();
        e->Handled = true;
    }
}

void SIUnitEdit::OnKeyDown(System::Object^ sender, System::Windows::Forms::KeyEventArgs^ e)
{
    if (e->KeyCode == System::Windows::Forms::Keys::Escape) {
        // abort editing process and set old value
        this->Clear();
        setValueQuiet(_value);
        EditingAborted(this, System::EventArgs::Empty);
        this->Parent->Focus();
        e->Handled = true;
    }
    else if (e->KeyCode == System::Windows::Forms::Keys::Return) {
        // use new value without prefix
        parseNewValue(1.0);
        continueEditing();
        e->Handled = true;
    }
}

void SIUnitEdit::OnMouseWheel(System::Object^ sender, System::Windows::Forms::MouseEventArgs^ e)
{
    if(_value == 0.0) {
        // can't figure out step size with zero value
        return;
    }
    // most mousewheel have 15 degree increments, the reported delta is in 1/8th degree -> 120
    auto increment = e->Delta / 120.0;
    // round toward bigger step in case of special higher resolution mousewheel
    unsigned int steps = std::abs(increment > 0 ? ceil(increment) : floor(increment));
    int sign = increment > 0 ? 1 : -1;
    // figure out step increment
    auto newVal = _value;
    if(this->Focused) {
        auto cursor = this->SelectionStart;
        if(cursor == 0) {
            // cursor in front of first digit, do nothing (too big of a change, probably over/underflows)
            return;
        }
        // change the digit at the current cursor
        int nthDigit = cursor;
        // account for decimal point/leading zero/sign
        if(_value < 0) {
            nthDigit--;
        }
        auto dotPos = this->Text->IndexOf('.');
        if(dotPos >= 0 && dotPos < nthDigit) {
            nthDigit--;
        }
        if(this->Text->StartsWith("-0.") || this->Text->StartsWith("0.")) {
            nthDigit--;
        }
        auto step_size = pow(10, floor(log10(std::abs(newVal))) - nthDigit + 1);
        newVal += step_size * steps * sign;
        setValue(newVal);
        this->Text = this->PlaceholderText;
        this->SelectionStart = cursor;
    } else {
        // default to the third digit
        constexpr int nthDigit = 3;
        while(steps > 0) {
            // do update in multiple steps because the step size could change inbetween
            auto step_size = pow(10, floor(log10(std::abs(newVal))) - nthDigit + 1);
            newVal += step_size * sign;
            steps--;
        }
        setValue(newVal);
    }
}

void SIUnitEdit::OnLostFocus(System::Object^ sender, System::EventArgs^ e)
{
    parseNewValue(1.0);
    FocusLost(this, System::EventArgs::Empty);
}

void SIUnitEdit::OnTextChanged(System::Object^ sender, System::EventArgs^ e)
{
    parseNewValue(1.0);
}

void SIUnitEdit::setValueQuiet(double value)
{
    _value = value;
    this->PlaceholderText = Unit::ToString(value, unit, prefixes, precision);
    if(!this->Text->IsEmpty()) {
        // currently editing, update the text as well
        continueEditing();
    }
}

void SIUnitEdit::parseNewValue(double factor)
{
    System::String^ input = this->Text;
    if(input->IsEmpty()) {
        setValueQuiet(_value);
        EditingAborted(this, System::EventArgs::Empty);
    } else {
        // remove optional unit
        if(input->EndsWith(unit)) {
            input = input->Substring(0, input->Length - unit->Length);
        }
        auto lastChar = input[input->Length - 1];
        if(prefixes->IndexOf(lastChar) >= 0) {
            factor = Unit::SIPrefixToFactor(lastChar);
            input = input->Substring(0, input->Length - 1);
        }
        // remaining input should only contain numbers
        double v;
        if(Double::TryParse(input, v)) {
            this->Clear();
            setValue(v * factor);
        } else {
            System::Diagnostics::Debug::WriteLine("SIUnit conversion failure: " + input);
        }
    }
}

void SIUnitEdit::continueEditing()
{
    this->Text = this->PlaceholderText;
    this->SelectAll();
}
