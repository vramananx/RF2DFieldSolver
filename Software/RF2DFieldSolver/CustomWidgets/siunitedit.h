#ifndef SIUNITEDIT_H
#define SIUNITEDIT_H

#include <Windows.h>
#include <System.Windows.Forms.h>

class SIUnitEdit : public System::Windows::Forms::TextBox
{
    Q_OBJECT
public:
    SIUnitEdit(System::String^ unit = nullptr, System::String^ prefixes = " ", int precision = 0, System::Windows::Forms::Form^ parent = nullptr);
    SIUnitEdit(System::Windows::Forms::Form^ parent);

    void setUnit(System::String^ unit) { this->unit = unit; setValueQuiet(_value); }
    void setPrefixes(System::String^ prefixes) { this->prefixes = prefixes; setValueQuiet(_value); }
    void setPrecision(int precision) { this->precision = precision; setValueQuiet(_value); }
    double value() { return _value; }
public slots:
    void setValue(double value);
    void setValueQuiet(double value);
signals:
    void ValueChanged(double newvalue);
    void ValueUpdated(System::Windows::Forms::Form^ w);
    void EditingAborted();
    void FocusLost();
protected:
    bool eventFilter(System::Object^ obj, System::EventArgs^ event) override;
private slots:
    void continueEditing();
private:
    void parseNewValue(double factor);
    System::String^ unit, prefixes;
    int precision;
    double _value;
};

#endif // SIUNITEDIT_H
