#include "scenario.h"
#include "ui_scenario.h"

#include "CustomWidgets/siunitedit.h"

#include "microstrip.h"
#include "stripline.h"
#include "coplanarmicrostrip.h"
#include "coplanarstripline.h"
#include "differentialmicrostrip.h"
#include "coplanardifferentialmicrostrip.h"
#include "differentialstripline.h"
#include "coplanardifferentialstripline.h"

Scenario::Scenario(System::Windows::Forms::Form^ parent) :
    Form(parent),
    ui(gcnew Ui::Scenario)
{
    ui->setupUi(this);

    ui->parameters->setLayout(gcnew System::Windows::Forms::TableLayoutPanel);
    ui->buttonBox->CancelButton = gcnew System::Windows::Forms::Button();
    ui->buttonBox->CancelButton->Click += gcnew System::EventHandler(this, &Scenario::reject);
    ui->buttonBox->AcceptButton = gcnew System::Windows::Forms::Button();
    ui->buttonBox->AcceptButton->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        // update parameters
        for(unsigned int i=0;i<parameters.size();i++) {
            auto layout = static_cast<System::Windows::Forms::TableLayoutPanel^>(ui->parameters->Layout);
            auto entry = static_cast<SIUnitEdit^>(layout->GetControlFromPosition(1, i));
            *parameters[i].value = entry->value();
        }

        auto list = createScenario();
        scenarioCreated(System::Drawing::PointF(ui->xleft->value(), ui->ytop->value()), System::Drawing::PointF(ui->xright->value(), ui->ybottom->value()), list);
        this->DialogResult = System::Windows::Forms::DialogResult::OK;
        this->Close();
    });
    ui->autoArea->Checked = true;
    ui->xleft->setUnit("m");
    ui->xleft->setPrefixes("um ");
    ui->xleft->setPrecision(4);
    ui->xleft->setValue(-3e-3);

    ui->xright->setUnit("m");
    ui->xright->setPrefixes("um ");
    ui->xright->setPrecision(4);
    ui->xright->setValue(3e-3);

    ui->ytop->setUnit("m");
    ui->ytop->setPrefixes("um ");
    ui->ytop->setPrecision(4);
    ui->ytop->setValue(3e-3);

    ui->ybottom->setUnit("m");
    ui->ybottom->setPrefixes("um ");
    ui->ybottom->setPrecision(4);
    ui->ybottom->setValue(-1e-3);
}

Scenario::~Scenario()
{
    delete ui;
}

QList<Scenario *> Scenario::createAll()
{
    QList<Scenario*> ret;
    ret.push_back(gcnew Microstrip());
    ret.push_back(gcnew CoplanarMicrostrip());
    ret.push_back(gcnew DifferentialMicrostrip());
    ret.push_back(gcnew CoplanarDifferentialMicrostrip());
    ret.push_back(gcnew Stripline());
    ret.push_back(gcnew CoplanarStripline());
    ret.push_back(gcnew DifferentialStripline());
    ret.push_back(gcnew CoplanarDifferentialStripline());

    for each (auto s in ret) {
        s->setupParameters();
    }
    return ret;
}

void Scenario::setupParameters()
{
    auto layout = static_cast<System::Windows::Forms::TableLayoutPanel^>(ui->parameters->Layout);
    for each (auto p in parameters) {
        auto label = gcnew System::Windows::Forms::Label();
        label->Text = p.name + ":";
        auto entry = gcnew SIUnitEdit(p.unit, p.prefixes, p.precision);
        entry->setValue(*p.value);
        layout->Controls->Add(label);
        layout->Controls->Add(entry);
    }
    this->Text = name + " Setup Dialog";

    // show the image
    ui->image->Image = getImage();
}
