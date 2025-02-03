#ifndef INFORMATIONBOX_H
#define INFORMATIONBOX_H

#include <Windows.h>
#include <System.Windows.Forms.h>

class InformationBox : public System::Windows::Forms::Form
{
    Q_OBJECT
public:
    static void ShowMessage(System::String^ title, System::String^ message, System::String^ messageID = nullptr, bool block = false, System::Windows::Forms::Form^ parent = nullptr);
    static void ShowMessageBlocking(System::String^ title, System::String^ message, System::String^ messageID = nullptr, System::Windows::Forms::Form^ parent = nullptr);
    static void ShowError(System::String^ title, System::String^ message, System::Windows::Forms::Form^ parent = nullptr);
    // Display a dialog with yes/no buttons. Returns true if yes is clicked, false otherwise. If the user has selected to never see this message again, defaultAnswer is returned instead
    static bool AskQuestion(System::String^ title, System::String^ question, bool defaultAnswer, System::String^ messageID = nullptr, System::Windows::Forms::Form^ parent = nullptr);

    static void setGUI(bool enable);
private:
    InformationBox(System::String^ title, System::String^ message, System::Windows::Forms::MessageBoxIcon icon, unsigned int hash, System::Windows::Forms::Form^ parent);
    ~InformationBox();
    static System::String^ hashToSettingsKey(unsigned int hash);
    unsigned int hash;
    static bool has_gui;
};

#endif // INFORMATIONBOX_H
