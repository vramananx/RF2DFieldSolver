#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <Windows.h>
#include <System.Windows.Forms.h>

#include "elementlist.h"
#include "laplace/laplace.h"
#include "gauss/gauss.h"
#include "savable.h"

class MainWindow : public System::Windows::Forms::Form, public Savable
{
public:
    MainWindow(System::Windows::Forms::Form^ parent = nullptr);
    ~MainWindow();

    virtual nlohmann::json toJSON() override;
    virtual void fromJSON(nlohmann::json j) override;

private:
    static constexpr double e0 = 8.8541878188e-12;
    void startCalculation();
    void calculationStopped();
    void info(System::String^ info);
    void warning(System::String^ warning);
    void error(System::String^ error);

    Ui::MainWindow^ ui;
    ElementList^ list;
    Laplace laplace;
    Gauss gauss;
};

#endif // MAINWINDOW_H
