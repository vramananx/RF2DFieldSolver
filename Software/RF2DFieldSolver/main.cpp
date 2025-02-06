#include "mainwindow.h"

#include <QApplication>

#include "informationbox.h"
#include "element.h"
#include "elementlist.h"
#include "gauss/gauss.h"
#include "laplace/laplace.h"
#include "laplace/lattice.h"
#include "laplace/worker.h"
#include "Scenarios/coplanardifferentialmicrostrip.h"
#include "Scenarios/coplanardifferentialstripline.h"
#include "Scenarios/coplanarmicrostrip.h"
#include "Scenarios/coplanarstripline.h"
#include "Scenarios/differentialmicrostrip.h"
#include "Scenarios/differentialstripline.h"
#include "Scenarios/microstrip.h"
#include "Scenarios/scenario.h"
#include "Scenarios/stripline.h"
#include "unit.h"
#include "util.h"

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    MainWindow w;
    w.show();
    return a.exec();
}

#include "informationbox.h"

#include <QCheckBox>
#include <QSettings>
#include <QDebug>

bool InformationBox::has_gui = true;

void InformationBox::ShowMessage(QString title, QString message, QString messageID, bool block, QWidget *parent)
{
    if(!has_gui) {
        // no gui option active, do not show any messages
        return;
    }

    // check if the user still wants to see this message
    unsigned int hash;
    if(messageID.isEmpty()) {
        hash = qHash(message);
    } else {
        hash = qHash(messageID);
    }

    QSettings s;
    if(!s.contains(hashToSettingsKey(hash))) {
        auto box = new InformationBox(title, message, QMessageBox::Information, hash, parent);
        if(block) {
            box->exec();
        } else {
            box->show();
        }
    }
}

void InformationBox::ShowMessageBlocking(QString title, QString message, QString messageID, QWidget *parent)
{
    ShowMessage(title, message, messageID, true, parent);
}

void InformationBox::ShowError(QString title, QString message, QWidget *parent)
{
    if(!has_gui) {
        // no gui option active, do not show any messages
        return;
    }
    auto box = new InformationBox(title, message, QMessageBox::Information, 0, parent);
    box->show();
}

bool InformationBox::AskQuestion(QString title, QString question, bool defaultAnswer, QString messageID, QWidget *parent)
{
    if(!has_gui) {
        // no gui option active, do not show any messages
        return defaultAnswer;
    }

    // check if the user still wants to see this message
    unsigned int hash;
    if(messageID.isEmpty()) {
        hash = qHash(question);
    } else {
        hash = qHash(messageID);
    }

    QSettings s;
    if(!s.contains(hashToSettingsKey(hash))) {
        auto box = new InformationBox(title, question, QMessageBox::Question, hash, parent);
        box->setStandardButtons(QMessageBox::Yes | QMessageBox::No);
        int ret = box->exec();
        if(ret == QMessageBox::Yes) {
            return true;
        } else {
            return false;
        }
    } else {
        // don't show this question anymore
        return s.value(hashToSettingsKey(hash)).toBool();
    }
}

void InformationBox::setGUI(bool enable)
{
    has_gui = enable;
}

InformationBox::InformationBox(QString title, QString message, Icon icon, unsigned int hash, QWidget *parent)
    : QMessageBox(parent),
      hash(hash)
{
    setWindowTitle(title);
    setText(message);
    setAttribute(Qt::WA_DeleteOnClose, true);
    setModal(true);
    setIcon(icon);

    auto cb = new QCheckBox("Do not show this message again");
    setCheckBox(cb);

    if(hash == 0) {
        cb->setVisible(false);
    }
}

InformationBox::~InformationBox()
{
    auto cb = checkBox();
    if(cb->isChecked()) {
        auto value = true;
        auto clicked = clickedButton();
        if(clicked && QMessageBox::standardButton(clicked) == StandardButton::No) {
            value = false;
        }
        QSettings s;
        s.setValue(hashToSettingsKey(hash), value);
    }
}

QString InformationBox::hashToSettingsKey(unsigned int hash)
{
    return QString("DoNotShowDialog/") + QString::number(hash);
}

#include "element.h"

Element::Element(Type type)
    : QObject{nullptr},
      type(type)
{
    epsilon_r = 4.3;
    switch(type) {
    case Type::TracePos: name = "RF+"; break;
    case Type::TraceNeg: name = "RF-"; break;
    case Type::Dielectric: name = "Substrate"; break;
    case Type::GND: name = "GND"; break;
    case Type::Last: break;
    }
}

nlohmann::json Element::toJSON()
{
    nlohmann::json j;
    j["name"] = name.toStdString();
    j["type"] = TypeToString(type).toStdString();
    j["e_r"] = epsilon_r;
    nlohmann::json jvertices;
    for(auto &v : vertices) {
        nlohmann::json jvertex;
        jvertex["x"] = v.x();
        jvertex["y"] = v.y();
        jvertices.push_back(jvertex);
    }
    j["vertices"] = jvertices;
    return j;
}

void Element::fromJSON(nlohmann::json j)
{
    name = QString::fromStdString(j.value("name", name.toStdString()));
    type = TypeFromString(QString::fromStdString(j.value("type", "")));
    epsilon_r = j.value("e_r", epsilon_r);
    vertices.clear();
    if(j.contains("vertices")) {
        for(auto jvertex : j["vertices"]) {
            QPointF p;
            p.rx() = jvertex.value("x", 0.0);
            p.ry() = jvertex.value("y", 0.0);
            vertices.push_back(p);
        }
    }
}

QString Element::TypeToString(Type type)
{
    switch(type) {
    case Type::Dielectric: return "Dielectric";
    case Type::GND: return "GND";
    case Type::TracePos: return "Trace+";
    case Type::TraceNeg: return "Trace-";
    case Type::Last: return "";
    }
    return "";
}

Element::Type Element::TypeFromString(QString s)
{
    for(unsigned int i=0;i<(int) Type::Last;i++) {
        if(s == TypeToString((Type) i)) {
            return (Type) i;
        }
    }
    return Type::Last;
}

QList<Element::Type> Element::getTypes()
{
    QList<Type> ret;
    for(unsigned int i=0;i<(int) Type::Last;i++) {
        ret.append((Type) i);
    }
    return ret;
}

void Element::addVertex(int index, QPointF vertex)
{
    vertices.insert(index, vertex);
}

void Element::appendVertex(QPointF vertex)
{
    vertices.append(vertex);
}

void Element::removeVertex(int index)
{
    if(index >= 0 && index < vertices.size()) {
        vertices.removeAt(index);
    }
}

void Element::changeVertex(int index, QPointF newCoords)
{
    if(index >= 0 && index < vertices.size()) {
        vertices[index] = newCoords;
    }
}

void Element::setType(Type t)
{
    type = t;
    emit typeChanged();
}

QPolygonF Element::toPolygon()
{
    auto ret = QPolygonF(vertices);
//    if(vertices.size() > 2) {
//        // QPolygon expects the last point to be the same as the first
//        ret << vertices[0];
//    }
    return ret;
}

#include "elementlist.h"

#include <QComboBox>

ElementList::ElementList(QObject *parent)
    : QAbstractTableModel{parent}
{

}

nlohmann::json ElementList::toJSON()
{
    nlohmann::json j;
    nlohmann::json jelements;
    for(auto e : elements) {
        jelements.push_back(e->toJSON());
    }
    j["elements"] = jelements;
    return j;
}

void ElementList::fromJSON(nlohmann::json j)
{
    while(elements.size()) {
        removeElement(0);
    }
    if(j.contains("elements")) {
        for(auto &jelement : j["elements"]) {
            auto e = new Element(Element::Type::Dielectric);
            e->fromJSON(jelement);
            addElement(e);
        }
    }
}

void ElementList::addElement(Element *e)
{
    beginInsertRows(QModelIndex(), elements.size(), elements.size());
    elements.append(e);
    // TODO set up connections
    connect(e, &Element::typeChanged, this, [=](){
        auto i = findIndex(e);
        if(i != -1) {
            emit dataChanged(index(i, (int) Column::EpsilonR), index(i, (int) Column::EpsilonR));
        }
    });
    connect(e, &Element::destroyed, this, [=](){
        removeElement(e, false);
    });
    endInsertRows();
}

bool ElementList::removeElement(Element *e, bool del)
{
    int i = findIndex(e);
    if(i != -1) {
        return removeElement(i, del);
    } else {
        // not found
        return false;
    }
}

bool ElementList::removeElement(int index, bool del)
{
    if (index < 0 || index >= elements.size()) {
        return false;
    }
    beginRemoveRows(QModelIndex(), index, index);
    auto e = elements[index];
    elements.removeAt(index);
    disconnect(e, nullptr, this, nullptr);
    if(del) {
        delete e;
    }
    endRemoveRows();
    return true;
}

Element *ElementList::elementAt(int index) const
{
    if (index >= 0 || index < elements.size()) {
        return elements[index];
    } else {
        return nullptr;
    }
}

double ElementList::getDielectricConstantAt(const QPointF &p)
{
    for(unsigned int i=0;i<elements.size();i++) {
        auto e = elements[i];
        QPolygonF poly = QPolygonF(e->getVertices());
        if(poly.containsPoint(p, Qt::OddEvenFill)) {
            // this polygon defines the weight at these coordinates
            switch(e->getType()) {
            case Element::Type::GND:
            case Element::Type::TracePos:
            case Element::Type::TraceNeg:
                return 1.0;
            case Element::Type::Dielectric:
                return e->getEpsilonR();
            case Element::Type::Last:
                return 1.0;
            }
        }
    }
    // not found, we are in the air
    return 1.0;
}

QVariant ElementList::data(const QModelIndex &index, int role) const
{
    auto row = index.row();
    auto col = index.column();
    Element *e = elements[row];
    switch(role) {
    case Qt::DisplayRole:
        switch((Column) col) {
        case Column::Name: return e->getName();
        case Column::Type: return e->TypeToString(e->getType());
        case Column::EpsilonR:
            if(e->getType() == Element::Type::Dielectric) {
                return QString::number(e->getEpsilonR());
            } else {
                return "";
            }
        case Column::Last: return QVariant();
        }
        break;
    default: return QVariant();
    }
    return QVariant();
}

QVariant ElementList::headerData(int section, Qt::Orientation orientation, int role) const
{
    if (orientation == Qt::Vertical) {
        // only horizontal header
        return QVariant();
    }
    if (role != Qt::DisplayRole) {
        return QVariant();
    }
    switch(section) {
    case 0: return "Name";
    case 1: return "Type";
    case 2: return "εr";
    default: return QVariant();
    }
}

bool ElementList::setData(const QModelIndex &index, const QVariant &value, int role)
{
    auto row = index.row();
    auto col = index.column();
    Element *e = elements[row];
    switch(role) {
    case Qt::EditRole:
        switch((Column) col) {
        case Column::Name: e->setName(value.toString()); return true;
        case Column::Type: e->setType(Element::TypeFromString(value.toString())); return true;
        case Column::EpsilonR:
            if(e->getType() == Element::Type::Dielectric) {
                e->setEpsilonR(value.toDouble());
                return true;
            } else {
                return false;
            }
        case Column::Last: return false;
        }
        break;
    }

    return false;
}

Qt::ItemFlags ElementList::flags(const QModelIndex &index) const
{
    auto flags = QAbstractTableModel::flags(index);

    bool editable = false;
    auto row = index.row();
    auto col = index.column();
    Element *e = elements[row];
    switch((Column) col) {
    case Column::Name: editable = true; break;
    case Column::Type: editable = true; break;
    case Column::EpsilonR: editable = e->getType() == Element::Type::Dielectric; break;
    case Column::Last: break;
    }
    if (editable) {
        flags |= Qt::ItemIsEditable;
    } else {
        flags &= ~Qt::ItemIsSelectable;
    }
    return flags;
}

int ElementList::findIndex(Element *e)
{
    return elements.indexOf(e);
}

QWidget *TypeDelegate::createEditor(QWidget *parent, const QStyleOptionViewItem &option, const QModelIndex &index) const
{
    Q_UNUSED(option)
    auto model = (ElementList*) index.model();
    auto editor = new QComboBox(parent);
    //c->setMaximumHeight(rowHeight);
    connect(editor, qOverload<int>(&QComboBox::currentIndexChanged), [editor](int) {
        editor->clearFocus();
    });
    for(auto t : Element::getTypes()) {
        editor->addItem(Element::TypeToString(t));
    }
    editor->setCurrentIndex((int) model->elementAt(index.row())->getType());
    return editor;
}

void TypeDelegate::setEditorData(QWidget *editor, const QModelIndex &index) const
{
    auto e = static_cast<const ElementList*>(index.model())->elementAt(index.row());
    auto c = (QComboBox*) editor;
    e->setType(Element::TypeFromString(c->currentText()));
}

void TypeDelegate::setModelData(QWidget *editor, QAbstractItemModel *model, const QModelIndex &index) const
{
    auto list = (ElementList*) model;
    auto c = (QComboBox*) editor;
    list->setData(index, c->currentText());
}

#include "gauss.h"

#include "polygon.h"

Gauss::Gauss(QObject *parent)
    : QObject{parent}
{

}

double Gauss::getCharge(Laplace *laplace, ElementList *list, Element *e, double gridSize, double distance)
{
    // extend the element polygon a bit
    auto integral = Polygon::offset(e->getVertices(), distance);

    double chargeSum = 0;
    for(unsigned int i=0;i<integral.size();i++) {
        auto pp = integral[(i+integral.size()-1) % integral.size()];
        auto pc = integral[i];

        auto increment = QLineF(pp, pc);
        auto unitVector = increment;
        unitVector.setLength(1.0);
        unsigned int points = ceil(increment.length() / gridSize);
        double stepSize = increment.length() / points;
        increment.setLength(stepSize);
        auto point = pp + QPointF(increment.dx() / 2, increment.dy() / 2);
        for(unsigned int j=0;j<points;j++) {
            QLineF gradient = laplace->getGradient(point);
            if(list) {
                gradient.setLength(gradient.length() * list->getDielectricConstantAt(point));
            }
            // get amount of gradient that is perpendicular to our integration line
            double perp = gradient.dx() * unitVector.dy() - gradient.dy() * unitVector.dx();
            perp *= stepSize / gridSize;
            chargeSum += perp;
            point += QPointF(increment.dx(), increment.dy());
        }
    }

    if(!Polygon::isClockwise(integral)) {
        chargeSum *= -1;
    }

    return chargeSum;
}

#include "laplace.h"

#include <QPolygonF>

Laplace::Laplace(QObject *parent)
    : QObject{parent}
{
    calculationRunning = false;
    resultReady = false;
    list = nullptr;
    grid = 1e-5;
    threads = 1;
    threshold = 1e-6;
    lattice = nullptr;
    groundedBorders = true;
    ignoreDielectric = false;
}

void Laplace::setArea(const QPointF &topLeft, const QPointF &bottomRight)
{
    if(calculationRunning) {
        return;
    }
    this->topLeft = topLeft;
    this->bottomRight = bottomRight;
}

void Laplace::setGrid(double grid)
{
    if(calculationRunning) {
        return;
    }
    if(grid > 0) {
        this->grid = grid;
    }
}

void Laplace::setThreads(int threads)
{
    if(calculationRunning) {
        return;
    }
    if(threads > 0) {
        this->threads = threads;
    }
}

void Laplace::setThreshold(double threshold)
{
    if(calculationRunning) {
        return;
    }
    if (threshold > 0) {
        this->threshold = threshold;
    }
}

void Laplace::setGroundedBorders(bool gnd)
{
    if(calculationRunning) {
        return;
    }
    groundedBorders = gnd;
}

void Laplace::setIgnoreDielectric(bool ignore)
{
    if(calculationRunning) {
        return;
    }
    ignoreDielectric = ignore;
}

bool Laplace::startCalculation(ElementList *list)
{
    if(calculationRunning) {
        return false;
    }
    calculationRunning = true;
    resultReady = false;
    lastPercent = 0;
    emit info("Laplace calculation starting");
    if(lattice) {
        delete lattice;
        lattice = nullptr;
    }
    this->list = list;

    // start the calculation thread
    auto err = pthread_create(&thread, nullptr, calcThreadTrampoline, this);
    if(err) {
        emit error("Failed to start laplace thread");
        return false;
    }
    emit info("Laplace thread started");
    return true;
}

void Laplace::abortCalculation()
{
    if(!calculationRunning) {
        return;
    }
    // request abort of calculation
    lattice->abort = true;
}

double Laplace::getPotential(const QPointF &p)
{
    if(!resultReady) {
        return std::numeric_limits<double>::quiet_NaN();
    }
    auto pos = coordToRect(p);
    // convert to integers and shift by the added outside boundary of NaNs
    int index_x = round(pos.x) + 1;
    int index_y = round(pos.y) + 1;
    if(index_x < 0 || index_x >= (int) lattice->dim.x || index_y < 0 || index_y >= (int) lattice->dim.y) {
        return std::numeric_limits<double>::quiet_NaN();
    }
    auto c = &lattice->cells[index_x+index_y*lattice->dim.x];
    return c->value;
}

QLineF Laplace::getGradient(const QPointF &p)
{
    QLineF ret = QLineF(p, p);
    if(!resultReady) {
        return ret;
    }
    auto pos = coordToRect(p);
    // convert to integers and shift by the added outside boundary of NaNs
    int index_x = floor(pos.x) + 1;
    int index_y = floor(pos.y) + 1;

    if(index_x < 0 || index_x + 1 >= (int) lattice->dim.x || index_y < 0 || index_y + 1>= (int) lattice->dim.y) {
        return ret;
    }
    // calculate gradient
    auto c_floor = &lattice->cells[index_x+index_y*lattice->dim.x];
    auto c_x = &lattice->cells[index_x+1+index_y*lattice->dim.x];
    auto c_y = &lattice->cells[index_x+(index_y+1)*lattice->dim.x];
    auto grad_x = c_x->value - c_floor->value;
    auto grad_y = c_y->value - c_floor->value;
    ret.setP2(p + QPointF(grad_x, grad_y));
    return ret;
}

void Laplace::invalidateResult()
{
    resultReady = false;
}

QPointF Laplace::coordFromRect(rect *pos)
{
    QPointF ret;
    ret.rx() = pos->x * grid + topLeft.x();
    ret.ry() = pos->y * grid + bottomRight.y();
    return ret;
}

struct rect Laplace::coordToRect(const QPointF &pos)
{
    struct rect ret;
    ret.x = (pos.x() - topLeft.x()) / grid;
    ret.y = (pos.y() - bottomRight.y()) / grid;
    return ret;
}

bound *Laplace::boundary(bound *bound, rect *pos)
{
    auto coord = coordFromRect(pos);
    bound->value = 0;
    bound->cond = NONE;

    bool isBorder = qFuzzyCompare(coord.x(), topLeft.x()) || qFuzzyCompare(coord.x(), bottomRight.x()) || qFuzzyCompare(coord.y(), topLeft.y()) || qFuzzyCompare(coord.y(), bottomRight.y());

    // handle borders
    if(groundedBorders && isBorder) {
        bound->value = 0;
        bound->cond = DIRICHLET;
        return bound;
    } else {
        // find the matching polygon
        for(auto e : list->getElements()) {
            if(e->getType() == Element::Type::Dielectric) {
                // skip, dielectric has no influence on boundary and trace/GND should always take priority
                continue;
            }
            QPolygonF poly = e->toPolygon();
            if(poly.containsPoint(coord, Qt::OddEvenFill)) {
                // this polygon defines the boundary at these coordinates
                switch(e->getType()) {
                case Element::Type::GND:
                    bound->value = 0;
                    bound->cond = DIRICHLET;
                    return bound;
                case Element::Type::TracePos:
                    bound->value = 1.0;
                    bound->cond = DIRICHLET;
                    return bound;
                case Element::Type::TraceNeg:
                    bound->value = -1.0;
                    bound->cond = DIRICHLET;
                    return bound;
                case Element::Type::Dielectric:
                case Element::Type::Last:
                    return bound;
                }
            }
        }
    }
    return bound;
}

double Laplace::weight(rect *pos)
{
    if(ignoreDielectric) {
        return 1.0;
    }

    auto coord = coordFromRect(pos);
    return sqrt(list->getDielectricConstantAt(coord));
}

void* Laplace::calcThread()
{
    emit info("Creating lattice");
    struct rect size = {(bottomRight.x() - topLeft.x()) / grid, (topLeft.y() - bottomRight.y()) / grid};
    struct point dim = {(uint32_t) ((bottomRight.x() - topLeft.x()) / grid), (uint32_t) ((topLeft.y() - bottomRight.y()) / grid)};
    lattice = lattice_new(&size, &dim, &boundaryTrampoline, &weightTrampoline, this);
    if(lattice) {
        emit info("Lattice creation complete");
    } else {
        emit error("Lattice creation failed");
        return nullptr;
    }

    struct config conf = {(uint8_t) threads, 10, threshold};
    if(conf.threads > lattice->dim.y / 5) {
        conf.threads = lattice->dim.y / 5;
    }
    conf.distance = lattice->dim.y / threads;
    emit info("Starting calculation threads");
    auto it = lattice_compute_threaded(lattice, &conf, calcProgressFromDiffTrampoline, this);
    calculationRunning = false;
    if(lattice->abort) {
        emit warning("Laplace calculation aborted");
        resultReady = false;
        emit percentage(0);
        emit calculationAborted();
    } else {
        emit info("Laplace calculation complete, took "+QString::number(it)+" iterations");
        resultReady = true;
        emit percentage(100);
        emit calculationDone();
    }
    return nullptr;
}

void Laplace::calcProgressFromDiff(double diff)
{
    // diff is expected to go down from 1.0 to the threshold with exponetial decay
    double endTime = pow(-log(threshold), 6);
    double currentTime = pow(-log(diff), 6);
    double percent = currentTime * 100 / endTime;
    if(percent > 100) {
        percent = 100;
    } else if(percent < lastPercent) {
        percent = lastPercent;
    }
    lastPercent = percent;
    emit percentage(percent);
}

#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include <pthread.h>

#include "lattice.h"
#include "worker.h"

/**
 * This function setups each of the cell in the lattice.
 */
void lattice_set_size(struct lattice* lattice, struct rect* size);

/**
 * This function applies the boundary function to each of the cell.
 */
void lattice_apply_bound(struct lattice* lattice, bound_t func, void *ptr);

/**
 * This function applies the weight function to each of the cells.
 */
void lattice_apply_weight(struct lattice* lattice, weight_t func, void *ptr);

/**
 * This function generates the function for each of the cell.
 */
void lattice_generate_function(struct lattice* lattice);

/**
 * This function applies one sequential iteration.
 */
double lattice_iterate(struct lattice* lattice);

struct lattice* lattice_new(struct rect* size, struct point* dim, bound_t func, weight_t w_func, void *ptr) {
    struct cell* cells;
    struct lattice* lattice;
    double (**update)(struct lattice*, struct cell*);

    /* make sure the dimension is useful */
    if(dim->x == 0 || dim->y == 0)
        return NULL;

    /* add two rows and two columns */
    dim->x += 3;
    dim->y += 3;

    /* compute the number of cell */
    uint32_t m = dim->x*dim->y;

    /* allocate memory for the cells */
    cells = malloc(m*sizeof(struct cell));
    if(cells == NULL) goto ERROR;

    /* allocate memory for the functions */
    update = malloc(m*sizeof(double (*)(struct lattice*, struct cell*)));
    if(update == NULL) goto ERROR;

    /* allocate the memory for the lattice structure */
    lattice = malloc(sizeof(struct lattice));
    if(lattice == NULL) goto ERROR;

    /* initialise the lattice structure */
    lattice->dim.x = dim->x;
    lattice->dim.y = dim->y;
    lattice->cells = cells;
    lattice->update = update;
    lattice->abort = false;

    /* apply all the steps for finishing the lattice */
    lattice_set_size(lattice, size);
    lattice_apply_bound(lattice, func, ptr);
    lattice_apply_weight(lattice, w_func, ptr);
    lattice_generate_function(lattice);

    return lattice;

ERROR:
    if(cells   != NULL) free(cells);
    if(update  != NULL) free(update);
    if(lattice != NULL) free(lattice);

    return NULL;
}

void lattice_delete(struct lattice* lattice) {
    /* free all the allocated memory */
    free(lattice->cells);
    free(lattice->update);
    free(lattice);
}

void lattice_print(struct lattice* lattice) {
    /* extract the dimension of the lattice */
    uint32_t w = lattice->dim.x;
    uint32_t h = lattice->dim.y;

    for(uint32_t j = 0; j < h; j++) {
        for(uint32_t i = 0; i < w; i++) {
            /* extract the pointer to the specified cell */
            struct cell* cell = &lattice->cells[i+j*w];

            /* don't print cells if they contain neumann condition */
            if(cell->cond == NEUMANN)
                fprintf(stderr, "          ,");
            else
                fprintf(stderr, "% 10.5f,", cell->value);
        }
        fprintf(stderr, "\n");
    }
}

void lattice_set_size(struct lattice* lattice, struct rect* size) {
    /* extract the dimension of the lattice */
    int32_t w = lattice->dim.x;
    int32_t h = lattice->dim.y;

    /* compute the subdivisions */
    double dx = size->x/(w-3);
    double dy = size->y/(h-3);

    for(int32_t j = -1; j+1 < h; j++) {
        for(int32_t i = -1; i+1 < w; i++) {
            /* extract the pointer to the specified cell */
            struct cell* cell = &lattice->cells[(i+1)+(j+1)*w];

            /* initialise the cell */
            cell->pos.x = i*dx;
            cell->pos.y = j*dy;
            cell->index.x = i+1;
            cell->index.y = j+1;
            cell->value = 0;

            /* the limits of the lattice are Neumann conditions */
            if(i == -1 || j == -1 || i == w-2 || j == h-2) {
                /* we don't need the adjacent cells */
                cell->adj[0] = 0;
                cell->adj[1] = 0;
                cell->adj[2] = 0;
                cell->adj[3] = 0;

                /* we don't need the adjacent cells */
                cell->diag[0] = 0;
                cell->diag[1] = 0;
                cell->diag[2] = 0;
                cell->diag[3] = 0;

                cell->cond = NEUMANN;
            } else {
                /* compute the index of the adjacent cells */
                cell->adj[0] = (i+1)+(j-0)*w;
                cell->adj[1] = (i+1)+(j+2)*w;
                cell->adj[2] = (i-0)+(j+1)*w;
                cell->adj[3] = (i+2)+(j+1)*w;

                /* compute the index of the diagonal cells */
                cell->diag[0] = (i+2)+(j-0)*w;
                cell->diag[1] = (i+2)+(j+2)*w;
                cell->diag[2] = (i-0)+(j+2)*w;
                cell->diag[3] = (i-0)+(j-0)*w;

                cell->cond = UNSET;
            }
        }
    }
}

void lattice_apply_bound(struct lattice* lattice, bound_t func, void *ptr) {
    /* extract the dimension of the lattice */
    uint32_t w = lattice->dim.x;
    uint32_t h = lattice->dim.y;

    /* used for returning data from the boundary function */
    struct bound bound = {NONE, 0};

    /* apply the boundary function to each cell */
    for(uint32_t j = 0; j < h; j++) {
        for(uint32_t i = 0; i < w; i++) {
            /* extract the pointer to the specified cell */
            struct cell* cell = &lattice->cells[i+j*w];

            /* make sure the cell isn't already set */
            if(cell->cond != UNSET)
                continue;

            /* apply the boundary function */
            if(func(ptr, &bound, &cell->pos) == NULL)
                continue;

            /* update the cell */
            cell->value = bound.value;
            cell->cond  = bound.cond;
        }
    }
}

void lattice_apply_weight(struct lattice* lattice, weight_t func, void *ptr) {
    /* extract the dimension of the lattice */
    uint32_t w = lattice->dim.x;
    uint32_t h = lattice->dim.y;

    /* apply the weight function to each cell */
    for(uint32_t j = 0; j < h; j++) {
        for(uint32_t i = 0; i < w; i++) {
            /* extract the pointer to the specified cell */
            struct cell* cell = &lattice->cells[i+j*w];

            /* update the cell */
            cell->weight = func(ptr, &cell->pos);
        }
    }
}

/* definition of the supported configurations */
#define GETFORMULA_MIDDLE_0     (  v1*w1+  v2*w2+  v3*w3+  v4*w4)/(w1+w2+w3+w4)
#define GETFORMULA_SIDE_1       (     2*v2*w2+  v3*w3+  v4*w4)/(2*w2+w3+w4)
#define GETFORMULA_SIDE_2       (2*v1*w1     +  v3*w3+  v4*w4)/(2*w1+w3+w4)
#define GETFORMULA_SIDE_3       (  v1*w1+  v2*w2+    +2*v4*w4)/(2*w4+w1+w2)
#define GETFORMULA_SIDE_4       (  v1*w1+  v2*w2+2*v3*w3     )/(2*w3+w1+w2)
#define GETFORMULA_CORNER_1     (   v2*w2+v3*w3   )/(w2+w3)
#define GETFORMULA_CORNER_2     (v1*w1   +v3*w3   )/(w1+w3)
#define GETFORMULA_CORNER_3     (v1*w1      +v4*w4)/(w1+w4)
#define GETFORMULA_CORNER_4     (   v2*w2   +v4*w4)/(w2+w4)
#define GETFORMULA_INV_CORNER_1 (  v1*w1+2*v2*w2+2*v3*w3+  v4+w4)/(w1+2*w2+2*w3+w4)
#define GETFORMULA_INV_CORNER_2 (2*v1*w1+  v2*w2+2*v3*w3+  v4*w4)/(2*w1+w2+2*w3+w4)
#define GETFORMULA_INV_CORNER_3 (2*v1*w1+  v2*w2+  v3*w3+2*v4*w4)/(2*w1+w2+w3+2*w4)
#define GETFORMULA_INV_CORNER_4 (  v1*w1+2*v2*w2+  v3*w3+2*v4*w4)/(w1+2*w2+w3+2*w4)

/**
 * This macro generates function for the supported configurations.
 * Even though not each function needs all the adjacent point, the
 * compiler should optimize away points that are not needed.
 */
#define MAKE_FUNCPOINTS(NUM,IDX) \
double func_##NUM##_##IDX (struct lattice* lattice, struct cell* cell) {\
    uint32_t i1 = cell->adj[0];           \
    uint32_t i2 = cell->adj[1];           \
    uint32_t i3 = cell->adj[2];           \
    uint32_t i4 = cell->adj[3];           \
                                          \
    double v1 = lattice->cells[i1].value; \
    double v2 = lattice->cells[i2].value; \
    double v3 = lattice->cells[i3].value; \
    double v4 = lattice->cells[i4].value; \
                                          \
    double w1 = lattice->cells[i1].weight;\
    double w2 = lattice->cells[i2].weight;\
    double w3 = lattice->cells[i3].weight;\
    double w4 = lattice->cells[i4].weight;\
                                          \
    (void)v1;(void)v2;(void)v3;(void)v4;  \
    (void)w1;(void)w2;(void)w3;(void)w4;  \
    return GETFORMULA_##NUM##_##IDX ;     \
}

/* we generate the function for each configuration */
MAKE_FUNCPOINTS(MIDDLE,0)
MAKE_FUNCPOINTS(SIDE,1)
MAKE_FUNCPOINTS(SIDE,2)
MAKE_FUNCPOINTS(SIDE,3)
MAKE_FUNCPOINTS(SIDE,4)
MAKE_FUNCPOINTS(CORNER,1)
MAKE_FUNCPOINTS(CORNER,2)
MAKE_FUNCPOINTS(CORNER,3)
MAKE_FUNCPOINTS(CORNER,4)
MAKE_FUNCPOINTS(INV_CORNER,1)
MAKE_FUNCPOINTS(INV_CORNER,2)
MAKE_FUNCPOINTS(INV_CORNER,3)
MAKE_FUNCPOINTS(INV_CORNER,4)

void lattice_generate_function(struct lattice* lattice) {
    /* extract the dimension of the lattice */
    int32_t w = lattice->dim.x;
    int32_t h = lattice->dim.y;

    for(int32_t j = 0; j < h; j++) {
        for(int32_t i = 0; i < w; i++) {
            /* compute the index of the cell */
            uint32_t index = i+j*w;

            /* extract the pointer to the specified cell */
            struct cell* cell = &lattice->cells[index];

            /* we ignore neumann or dirichlet conditions */
            if(cell->cond == NEUMANN || cell->cond == DIRICHLET) {
                lattice->update[index] = NULL;

                continue;
            }

            /* extracts each adjacent cell */
            struct cell* a1 = &lattice->cells[cell->adj[0]];
            struct cell* a2 = &lattice->cells[cell->adj[1]];
            struct cell* a3 = &lattice->cells[cell->adj[2]];
            struct cell* a4 = &lattice->cells[cell->adj[3]];

            struct cell* d1 = &lattice->cells[cell->diag[0]];
            struct cell* d2 = &lattice->cells[cell->diag[1]];
            struct cell* d3 = &lattice->cells[cell->diag[2]];
            struct cell* d4 = &lattice->cells[cell->diag[3]];

            /* check if the adjacent cells are neumann boundary */
            int A1 = (a1->cond == NEUMANN) ? 1 : 0;
            int A2 = (a2->cond == NEUMANN) ? 1 : 0;
            int A3 = (a3->cond == NEUMANN) ? 1 : 0;
            int A4 = (a4->cond == NEUMANN) ? 1 : 0;

            /* check if the diagonal cells are neumann boundary */
            int D1 = (d1->cond == NEUMANN) ? 1 : 0;
            int D2 = (d2->cond == NEUMANN) ? 1 : 0;
            int D3 = (d3->cond == NEUMANN) ? 1 : 0;
            int D4 = (d4->cond == NEUMANN) ? 1 : 0;

            /* generate a function the supported configurations */
            #define f lattice->update[index]
            if(!A1 && !A2 && !A3 && !A4) {
                if     ( D1 && !D2 && !D3 && !D4) f = &func_INV_CORNER_1;
                else if(!D1 &&  D2 && !D3 && !D4) f = &func_INV_CORNER_2;
                else if(!D1 && !D2 &&  D3 && !D4) f = &func_INV_CORNER_3;
                else if(!D1 && !D2 && !D3 &&  D4) f = &func_INV_CORNER_4;
                else f = &func_MIDDLE_0;
            }
            else if( A1 && !A2 && !A3 && !A4) f = &func_SIDE_1;
            else if(!A1 &&  A2 && !A3 && !A4) f = &func_SIDE_2;
            else if(!A1 && !A2 &&  A3 && !A4) f = &func_SIDE_3;
            else if(!A1 && !A2 && !A3 &&  A4) f = &func_SIDE_4;
            else if( A1 && !A2 && !A3 &&  A4) f = &func_CORNER_1;
            else if(!A1 &&  A2 && !A3 &&  A4) f = &func_CORNER_2;
            else if(!A1 &&  A2 &&  A3 && !A4) f = &func_CORNER_3;
            else if( A1 && !A2 &&  A3 && !A4) f = &func_CORNER_4;
            else f = &func_MIDDLE_0;
            #undef f
        }
    }
}

uint32_t lattice_compute(struct lattice* lattice, double threshold) {
    uint32_t iterations = 0;

    /* apply iterations until the threshold is bigger */
    while(lattice_iterate(lattice) > threshold)
        iterations++;

    return iterations;
}

double lattice_iterate(struct lattice* lattice) {
    /* extract the dimension of the lattice */
    int
