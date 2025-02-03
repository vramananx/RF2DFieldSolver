#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <System.Windows.Forms.h>
#include <System.Drawing.h>

#include "polygon.h"

#include "Scenarios/scenario.h"

static const System::String^ APP_VERSION = System::String::Format("{0}.{1}.{2}", FW_MAJOR, FW_MINOR, FW_PATCH);
static const System::String^ APP_GIT_HASH = GITHASH;

MainWindow::MainWindow(System::Windows::Forms::Form^ parent)
    : Form(parent)
    , ui(gcnew Ui::MainWindow)
{
    ui->setupUi(this);

    this->Text = this->Text + " " + APP_VERSION + "-" + APP_GIT_HASH->Substring(0, 9);

    auto updateViewArea = gcnew System::Action(this, &MainWindow::updateViewArea);

    this->WindowState = System::Windows::Forms::FormWindowState::Maximized;
    ui->splitter->SplitterDistance = 1000;
    ui->splitter_2->SplitterDistance = 5000;

    ui->resolution->setUnit("m");
    ui->resolution->setPrefixes("um ");
    ui->resolution->setPrecision(4);
    ui->resolution->setValue(10e-6);

    ui->gaussDistance->setUnit("m");
    ui->gaussDistance->setPrefixes("um ");
    ui->gaussDistance->setPrecision(4);
    ui->gaussDistance->setValue(20e-6);

    ui->tolerance->setUnit("V");
    ui->tolerance->setPrefixes("pnum ");
    ui->tolerance->setPrecision(4);
    ui->tolerance->setValue(100e-9);

    ui->threads->setValue(20);

    ui->borderIsGND->Checked = true;

    ui->xleft->setUnit("m");
    ui->xleft->setPrefixes("um ");
    ui->xleft->setPrecision(4);
    ui->xleft->ValueChanged += gcnew System::EventHandler(this, updateViewArea);
    ui->xleft->setValue(-3e-3);

    ui->xright->setUnit("m");
    ui->xright->setPrefixes("um ");
    ui->xright->setPrecision(4);
    ui->xright->ValueChanged += gcnew System::EventHandler(this, updateViewArea);
    ui->xright->setValue(3e-3);

    ui->ytop->setUnit("m");
    ui->ytop->setPrefixes("um ");
    ui->ytop->setPrecision(4);
    ui->ytop->ValueChanged += gcnew System::EventHandler(this, updateViewArea);
    ui->ytop->setValue(3e-3);

    ui->ybottom->setUnit("m");
    ui->ybottom->setPrefixes("um ");
    ui->ybottom->setPrecision(4);
    ui->ybottom->ValueChanged += gcnew System::EventHandler(this, updateViewArea);
    ui->ybottom->setValue(-1e-3);

    ui->gridsize->setUnit("m");
    ui->gridsize->setPrefixes("um ");
    ui->gridsize->setPrecision(4);
    ui->gridsize->ValueChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->view->setGrid(ui->gridsize->value());
    });
    ui->gridsize->setValue(1e-4);

    ui->showPotential->CheckedChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->view->setShowPotential(ui->showPotential->Checked);
    });
    ui->showPotential->Checked = true;

    ui->showGrid->CheckedChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->view->setShowGrid(ui->showGrid->Checked);
    });
    ui->showGrid->Checked = true;

    ui->snapGrid->CheckedChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->view->setSnapToGrid(ui->snapGrid->Checked);
    });
    ui->snapGrid->Checked = true;

    ui->viewMode->SelectedIndexChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->view->setKeepAspectRatio(ui->viewMode->SelectedIndex == 0);
    });

    ui->capacitanceP->setUnit("F/m");
    ui->capacitanceP->setPrefixes("fpnum ");
    ui->capacitanceP->setPrecision(4);

    ui->inductanceP->setUnit("H/m");
    ui->inductanceP->setPrefixes("fpnum ");
    ui->inductanceP->setPrecision(4);

    ui->impedanceP->setUnit("Ω");
    ui->impedanceP->setPrecision(4);

    ui->capacitanceN->setUnit("F/m");
    ui->capacitanceN->setPrefixes("fpnum ");
    ui->capacitanceN->setPrecision(4);

    ui->inductanceN->setUnit("H/m");
    ui->inductanceN->setPrefixes("fpnum ");
    ui->inductanceN->setPrecision(4);

    ui->impedanceN->setUnit("Ω");
    ui->impedanceN->setPrecision(4);

    ui->impedanceDiff->setUnit("Ω");
    ui->impedanceDiff->setPrecision(4);

    // save/load
    ui->actionOpen->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        openFromFileDialog("Load project", "RF 2D field solver files (*.RF2Dproj)");
        ui->view->Invalidate();
    });
    ui->actionSave->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        saveToFileDialog("Load project", "RF 2D field solver files (*.RF2Dproj)", ".RF2Dproj");
    });

    list = gcnew ElementList();
    ui->table->DataSource = list;
    ui->table->Columns[(int) ElementList::Column::Type]->CellTemplate = gcnew TypeDelegate();
    ui->view->setElementList(list);
    ui->view->setLaplace(&laplace);

    // connections for adding/removing elements
    auto addMenu = gcnew System::Windows::Forms::ContextMenuStrip();
    auto addRF = gcnew System::Windows::Forms::ToolStripMenuItem("Trace (+)");
    addRF->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        auto e = gcnew Element(Element::Type::TracePos);
        list->addElement(e);
        ui->view->startAppending(e);
    });
    addMenu->Items->Add(addRF);
    auto addRFNeg = gcnew System::Windows::Forms::ToolStripMenuItem("Trace (-)");
    addRFNeg->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        auto e = gcnew Element(Element::Type::TraceNeg);
        list->addElement(e);
        ui->view->startAppending(e);
    });
    addMenu->Items->Add(addRFNeg);
    auto addDielectric = gcnew System::Windows::Forms::ToolStripMenuItem("Dielectric");
    addDielectric->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        auto e = gcnew Element(Element::Type::Dielectric);
        list->addElement(e);
        ui->view->startAppending(e);
    });
    addMenu->Items->Add(addDielectric);
    auto addGND = gcnew System::Windows::Forms::ToolStripMenuItem("GND");
    addGND->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        auto e = gcnew Element(Element::Type::GND);
        list->addElement(e);
        ui->view->startAppending(e);
    });
    addMenu->Items->Add(addGND);
    ui->add->ContextMenuStrip = addMenu;

    ui->remove->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        auto row = ui->table->CurrentRow->Index;
        if(row >= 0 && row <= list->getElements()->Count) {
            list->removeElement(row);
            ui->view->Invalidate();
        }
    });

    // connections for the calculations
    ui->update->Click += gcnew System::EventHandler(this, &MainWindow::startCalculation);

    laplace->info += gcnew System::EventHandler(this, &MainWindow::info);
    laplace->warning += gcnew System::EventHandler(this, &MainWindow::warning);
    laplace->error += gcnew System::EventHandler(this, &MainWindow::error);
    laplace->calculationDone += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        // laplace is done
        laplace->percentage -= gcnew System::EventHandler(this, nullptr);
        ui->abort->Click -= gcnew System::EventHandler(this, nullptr);

        ui->view->Invalidate();
        // start gauss calculation
        info("Starting gauss integration for charge without dielectric");
        double chargeSumP = 0, chargeSumN = 0;
        for each (auto e in list->getElements()) {
            switch(e->getType()) {
            case Element::Type::TracePos:
                chargeSumP += Gauss::getCharge(&laplace, nullptr, e, ui->resolution->value(), ui->gaussDistance->value());
                break;
            case Element::Type::TraceNeg:
                chargeSumN -= Gauss::getCharge(&laplace, nullptr, e, ui->resolution->value(), ui->gaussDistance->value());
                break;
            case Element::Type::GND:
            case Element::Type::Dielectric:
            case Element::Type::Last:
                break;
            }
        }
        info("Air gauss calculation done");
        auto CairP = chargeSumP * e0;
        auto LP = 1.0 / (std::pow(2.998e8, 2.0) * CairP);
        ui->inductanceP->setValue(LP);

        auto CairN = chargeSumN * e0;
        auto LN = 1.0 / (std::pow(2.998e8, 2.0) * CairN);
        ui->inductanceN->setValue(LN);

        // start gauss calculation
        info("Starting gauss integration for charge with dielectric");
        chargeSumP = 0, chargeSumN = 0;
        for each (auto e in list->getElements()) {
            switch(e->getType()) {
            case Element::Type::TracePos:
                chargeSumP += Gauss::getCharge(&laplace, list, e, ui->resolution->value(), ui->gaussDistance->value());
                break;
            case Element::Type::TraceNeg:
                chargeSumN -= Gauss::getCharge(&laplace, list, e, ui->resolution->value(), ui->gaussDistance->value());
                break;
            case Element::Type::GND:
            case Element::Type::Dielectric:
            case Element::Type::Last:
                break;
            }
        }
        info("Dielectric gauss calculation done");
        auto CdielectricP = chargeSumP * e0;
        ui->capacitanceP->setValue(CdielectricP);

        auto CdielectricN = chargeSumN * e0;
        ui->capacitanceN->setValue(CdielectricN);

        auto impedanceP = sqrt(ui->inductanceP->value() / CdielectricP);
        ui->impedanceP->setValue(impedanceP);

        auto impedanceN = sqrt(ui->inductanceN->value() / CdielectricN);
        ui->impedanceN->setValue(impedanceN);

        ui->impedanceDiff->setValue(ui->impedanceP->value() + ui->impedanceN->value());

        // calculation complete
        ui->progress->Value = 100;
        ui->update->Enabled = true;
        ui->abort->Enabled = false;
        calculationStopped();
        ui->view->Invalidate();
    });

    auto calculationAborted = gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        ui->progress->Value = 0;
        calculationStopped();
        ui->view->Invalidate();
    });

    laplace->calculationAborted += calculationAborted;

    auto scenarios = Scenario::createAll();
    for each (auto s in scenarios) {
        auto action = gcnew System::Windows::Forms::ToolStripMenuItem(s->getName());
        ui->menuPredefined_Scenarios->Items->Add(action);
        action->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
            s->Show();
        });
        s->scenarioCreated += gcnew System::EventHandler(this, [=](System::Object^ sender, Scenario::ScenarioCreatedEventArgs^ e){
            // set up new area
            ui->xleft->setValue(e->TopLeft.X);
            ui->xright->setValue(e->BottomRight.X);
            ui->ytop->setValue(e->TopLeft.Y);
            ui->ybottom->setValue(e->BottomRight.Y);
            // switch to the new elements
            ui->view->setElementList(e->List);
            delete this->list;
            this->list = e->List;
            ui->table->DataSource = list;
        });
    }
}

MainWindow::~MainWindow()
{
    delete ui;
}

nlohmann::json MainWindow::toJSON()
{
    nlohmann::json j;
    // store simulation box imformation
    j["xleft"] = ui->xleft->value();
    j["xright"] = ui->xright->value();
    j["ytop"] = ui->ytop->value();
    j["ybottom"] = ui->ybottom->value();
    j["viewGrid"] = ui->gridsize->value();
    // store view settings
    j["showPotential"] = ui->showPotential->Checked;
    j["showGrid"] = ui->showGrid->Checked;
    j["snapToGrid"] = ui->snapGrid->Checked;
    j["viewMode"] = ui->viewMode->Text;
    // store simulation parameters
    j["simulationGrid"] = ui->resolution->value();
    j["gaussDistance"] = ui->gaussDistance->value();
    j["tolerance"] = ui->tolerance->value();
    j["threads"] = ui->threads->value();
    j["borderIsGND"] = ui->borderIsGND->Checked;
    // store elements
    j["list"] = list->toJSON();
    return j;
}

void MainWindow::fromJSON(nlohmann::json j)
{
    // load simulation box information
    ui->xleft->setValue(j.value("xleft", ui->xleft->value()));
    ui->xright->setValue(j.value("xright", ui->xright->value()));
    ui->ytop->setValue(j.value("ytop", ui->ytop->value()));
    ui->ybottom->setValue(j.value("ybottom", ui->ybottom->value()));
    ui->gridsize->setValue(j.value("viewGrid", ui->gridsize->value()));
    // load view settings
    ui->showPotential->Checked = j.value("showPotential", ui->showPotential->Checked);
    ui->showGrid->Checked = j.value("showGrid", ui->showGrid->Checked);
    ui->snapGrid->Checked = j.value("snapToGrid", ui->snapGrid->Checked);
    ui->viewMode->Text = j.value("viewMode", ui->viewMode->Text);
    // load simulation parameters
    ui->resolution->setValue(j.value("simulationGrid", ui->resolution->value()));
    ui->gaussDistance->setValue(j.value("gaussDistance", ui->gaussDistance->value()));
    ui->tolerance->setValue(j.value("tolerance", ui->tolerance->value()));
    ui->threads->setValue(j.value("threads", ui->threads->value()));
    ui->borderIsGND->Checked = j.value("borderIsGND", ui->borderIsGND->Checked);
    // load elements
    if(j.contains("list")) {
        list->fromJSON(j["list"]);
    }
}

void MainWindow::info(System::String^ info)
{
    auto tf = ui->status->SelectionFont;
    tf->Color = System::Drawing::Color::Black;
    ui->status->SelectionFont = tf;
    ui->status->AppendText(info);
    auto sb = ui->status->VerticalScrollBar;
    sb->Value = sb->Maximum;
}

void MainWindow::warning(System::String^ warning)
{
    auto tf = ui->status->SelectionFont;
    tf->Color = System::Drawing::Color::FromArgb(255, 174, 26);
    ui->status->SelectionFont = tf;
    ui->status->AppendText(warning);
    auto sb = ui->status->VerticalScrollBar;
    sb->Value = sb->Maximum;
}

void MainWindow::error(System::String^ error)
{
    auto tf = ui->status->SelectionFont;
    tf->Color = System::Drawing::Color::FromArgb(255, 94, 0);
    ui->status->SelectionFont = tf;
    ui->status->AppendText(error);
    auto sb = ui->status->VerticalScrollBar;
    sb->Value = sb->Maximum;
}

void MainWindow::startCalculation()
{
    ui->progress->Value = 0;
    ui->update->Enabled = false;
    ui->abort->Enabled = true;
    ui->view->Enabled = false;
    ui->table->Enabled = false;
    ui->gridsize->Enabled = false;
    ui->xleft->Enabled = false;
    ui->xright->Enabled = false;
    ui->ytop->Enabled = false;
    ui->ybottom->Enabled = false;
    ui->resolution->Enabled = false;
    ui->gaussDistance->Enabled = false;
    ui->threads->Enabled = false;
    ui->tolerance->Enabled = false;
    ui->borderIsGND->Enabled = false;
    ui->add->Enabled = false;
    ui->remove->Enabled = false;

    // start the calculations
    ui->status->Clear();
    ui->capacitanceP->setValue(System::Double::NaN);
    ui->inductanceP->setValue(System::Double::NaN);
    ui->impedanceP->setValue(System::Double::NaN);
    ui->capacitanceN->setValue(System::Double::NaN);
    ui->inductanceN->setValue(System::Double::NaN);
    ui->impedanceN->setValue(System::Double::NaN);
    ui->impedanceDiff->setValue(System::Double::NaN);

    laplace->invalidateResult();
    ui->view->Invalidate();
    // TODO sanity check elements

    // check for self-intersecting polygons
    for each (auto e in list->getElements()) {
        if(Polygon::selfIntersects(e->getVertices())) {
            error("Element \""+e->getName()+"\" self intersects, this is not supported");
            calculationStopped();
            return;
        }
    }
    // check for short circuits between RF and GND
    for each (auto e1 in list->getElements()) {
        if(e1->getType() != Element::Type::GND) {
            continue;
        }
        for each (auto e2 in list->getElements()) {
            if(e2->getType() != Element::Type::TracePos && e2->getType() != Element::Type::TraceNeg) {
                continue;
            }
            // check for overlap
            if(System::Drawing::Drawing2D::GraphicsPath::IsOutlineVisible(e1->getVertices(), e2->getVertices())) {
                error("Short circuit between RF \""+e2->getName()+"\" and GND \""+e1->getName()+"\"");
                calculationStopped();
                return;
            }
        }
    }
    // check for overlapping/touching RF elements
    for(int i=0; i<list->getElements()->Count; i++) {
        auto e1 = list->getElements()[i];
        if(e1->getType() != Element::Type::TracePos && e1->getType() != Element::Type::TraceNeg) {
            continue;
        }
        for(int j=i+1; j<list->getElements()->Count; j++) {
            auto e2 = list->getElements()[j];
            if(e2->getType() != Element::Type::TracePos && e2->getType() != Element::Type::TraceNeg) {
                continue;
            }
            // check for overlap
            if(System::Drawing::Drawing2D::GraphicsPath::IsOutlineVisible(e1->getVertices(), e2->getVertices())) {
                error("Traces \""+e2->getName()+"\" and \""+e1->getName()+"\" touch/overlap, this is not supported");
                calculationStopped();
                return;
            }
        }
    }
    // check and warn about overlapping dielectrics
    for(int i=0; i<list->getElements()->Count; i++) {
        auto e1 = list->getElements()[i];
        if(e1->getType() != Element::Type::Dielectric) {
            continue;
        }
        for(int j=i+1; j<list->getElements()->Count; j++) {
            auto e2 = list->getElements()[j];
            if(e2->getType() != Element::Type::Dielectric) {
                continue;
            }
            // check for overlap
            auto P1 = System::Drawing::Drawing2D::GraphicsPath::IsOutlineVisible(e1->getVertices());
            auto P2 = System::Drawing::Drawing2D::GraphicsPath::IsOutlineVisible(e2->getVertices());
            if(P1->IsOutlineVisible(P2)) {
                // check if this is actually an overlap or just touching
                auto intersect = P1->Intersect(P2);
                // calculate area of intersection
                double area = 0;
                for(int k=0; k<intersect->Count-1; k++) {
                    auto s1 = intersect[k];
                    auto s2 = intersect[(k+1) % intersect->Count];
                    area += s1.X * s2.Y - s2.X * s1.Y;
                }
                area = Math::Abs(area / 2);
                if(area > 0) {
                    warning("Dielectric \""+e1->getName()+"\" and \""+e2->getName()+"\" overlap, \""+e1->getName()+"\" will be used for overlapping area");
                }
            }
        }
    }

    laplace->percentage += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
        constexpr int minPercent = 0;
        constexpr int maxPercent = 99;
        ui->progress->Value = percent * (maxPercent-minPercent) / 100 + minPercent;
    });
    ui->abort->Click += gcnew System::EventHandler(this, &Laplace::abortCalculation);

    // Start the dielectric laplace calculation
    laplace->setArea(ui->view->getTopLeft(), ui->view->getBottomRight());
    laplace->setGrid(ui->resolution->value());
    laplace->setThreads(ui->threads->value());
    laplace->setThreshold(ui->tolerance->value());
    laplace->setGroundedBorders(ui->borderIsGND->Checked);
    laplace->startCalculation(list);
    ui->view->Invalidate();
}

void MainWindow::calculationStopped()
{
    ui->update->Enabled = true;
    ui->abort->Enabled = false;
    ui->view->Enabled = true;
    ui->table->Enabled = true;
    ui->gridsize->Enabled = true;
    ui->xleft->Enabled = true;
    ui->xright->Enabled = true;
    ui->ytop->Enabled = true;
    ui->ybottom->Enabled = true;
    ui->resolution->Enabled = true;
    ui->gaussDistance->Enabled = true;
    ui->threads->Enabled = true;
    ui->tolerance->Enabled = true;
    ui->borderIsGND->Enabled = true;
    ui->add->Enabled = true;
    ui->remove->Enabled = true;
}
