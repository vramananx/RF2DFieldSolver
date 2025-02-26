#include <QObject>
#include <QPointF>

#include <pthread.h>

#include "elementlist.h"
#include "lattice.h"

class Laplace : public QObject
{
    Q_OBJECT
public:
    explicit Laplace(QObject *parent = nullptr);

    void setArea(const QPointF &topLeft, const QPointF &bottomRight);
    void setGrid(double grid);
    void setThreads(int threads);
    void setThreshold(double threshold);
    void setGroundedBorders(bool gnd);
    void setIgnoreDielectric(bool ignore);

    bool startCalculation(ElementList *list);
    void abortCalculation();
    double getPotential(const QPointF &p);
    QLineF getGradient(const QPointF &p);
    bool isResultReady() {return resultReady;}
    void invalidateResult();

    double weight(rect *pos);

signals:
    void percentage(int percent);
    void calculationDone();
    void calculationAborted();
    void info(QString info);
    void warning(QString warning);
    void error(QString error);

private:
    QPointF coordFromRect(struct rect *pos);
    struct rect coordToRect(const QPointF &pos);
    bound* boundary(struct bound* bound, struct rect* pos);
    static struct bound* boundaryTrampoline(void *ptr, struct bound* bound, struct rect* pos) {
        return ((Laplace*)ptr)->boundary(bound, pos);
    }
    static double weightTrampoline(void *ptr, struct rect* pos) {
        return ((Laplace*)ptr)->weight(pos);
    }
    void* calcThread();
    static void* calcThreadTrampoline(void *ptr) {
        return ((Laplace*)ptr)->calcThread();
    }
    void calcProgressFromDiff(double diff);
    static void calcProgressFromDiffTrampoline(void *ptr, double diff) {
        ((Laplace*)ptr)->calcProgressFromDiff(diff);
    }
    bool calculationRunning;
    bool resultReady;
    ElementList *list;
    QPointF topLeft, bottomRight;
    double grid;
    int threads;
    double threshold;
    bool groundedBorders;
    bool ignoreDielectric;
    struct lattice *lattice;
    int lastPercent;

    pthread_t thread;
};

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
