#ifndef SCENARIO_H
#define SCENARIO_H

#include <Windows.h>
#include <System.Windows.Forms.h>

#include "elementlist.h"

namespace Ui {
class Scenario;
}

class Scenario : public System::Windows::Forms::Form
{
    Q_OBJECT

public:
    explicit Scenario(System::Windows::Forms::Form^ parent = nullptr);
    ~Scenario();

    static QList<Scenario*> createAll();

    void setupParameters();
    const System::String^ getName() const { return name; }

signals:
    void scenarioCreated(System::Drawing::PointF topLeft, System::Drawing::PointF bottomRight, ElementList *list);

protected:
    virtual ElementList *createScenario() = 0;
    virtual System::Drawing::Image^ getImage() = 0;

    using Parameter = struct {
        System::String^ name;
        System::String^ unit;
        System::String^ prefixes;
        int precision;
        double *value;
    };
    QList<Parameter> parameters;
    System::String^ name;
    Ui::Scenario *ui;
private:
    };
#endif // SCENARIO_H
