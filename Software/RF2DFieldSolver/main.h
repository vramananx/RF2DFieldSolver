#ifndef MAIN_H
#define MAIN_H

#include <QMessageBox>
#include <QObject>
#include <QList>
#include <QPointF>
#include <QPolygonF>
#include <QAbstractTableModel>
#include <QStyledItemDelegate>
#include <QComboBox>
#include <QCheckBox>
#include <QSettings>
#include <QDebug>
#include <QPixmap>
#include <QApplication>
#include <pthread.h>
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include "nlohmann/json.hpp"
#include "lattice.h"
#include "worker.h"
#include "polygon.h"
#include "savable.h"
#include "qpointervariant.h"
#include "unit.h"
#include "util.h"

class InformationBox : public QMessageBox
{
    Q_OBJECT
public:
    static void ShowMessage(QString title, QString message, QString messageID = QString(), bool block = false, QWidget *parent = nullptr);
    static void ShowMessageBlocking(QString title, QString message, QString messageID = QString(), QWidget *parent = nullptr);
    static void ShowError(QString title, QString message, QWidget *parent = nullptr);
    // Display a dialog with yes/no buttons. Returns true if yes is clicked, false otherwise. If the user has selected to never see this message again, defaultAnswer is returned instead
    static bool AskQuestion(QString title, QString question, bool defaultAnswer, QString messageID = QString(), QWidget *parent = nullptr);

    static void setGUI(bool enable);
private:
    InformationBox(QString title, QString message, QMessageBox::Icon icon, unsigned int hash, QWidget *parent);
    ~InformationBox();
    static QString hashToSettingsKey(unsigned int hash);
    unsigned int hash;
    static bool has_gui;
};

class Element : public QObject, public Savable
{
    Q_OBJECT
public:
    enum class Type {
        Dielectric,
        TracePos,
        TraceNeg,
        GND,
        Last,
    };

    explicit Element(Type type);

    virtual nlohmann::json toJSON() override;
    virtual void fromJSON(nlohmann::json j) override;

    static QString TypeToString(Type type);
    static Type TypeFromString(QString s);
    static QList<Type> getTypes();

    QString getName() const {return name;}
    Type getType() const {return type;}
    double getEpsilonR() const {return epsilon_r;}
    const QList<QPointF>& getVertices() const {return vertices;}
    void addVertex(int index, QPointF vertex);
    void appendVertex(QPointF vertex);
    void removeVertex(int index);
    void changeVertex(int index, QPointF newCoords);

    void setName(QString s) {name = s;}
    void setType(Type t);
    void setEpsilonR(double er) {epsilon_r = er;}
    QPolygonF toPolygon();

signals:
    void typeChanged();

private:
    QList<QPointF> vertices;
    QString name;
    Type type;
    double epsilon_r;
};

class TypeDelegate : public QStyledItemDelegate
{
    Q_OBJECT
    //QSize sizeHint ( const QStyleOptionViewItem & option, const QModelIndex & index ) const override;
    QWidget *createEditor(QWidget * parent, const QStyleOptionViewItem & option, const QModelIndex & index) const override;
    void setEditorData(QWidget * editor, const QModelIndex & index) const override;
    void setModelData(QWidget * editor, QAbstractItemModel * model, const QModelIndex & index) const override;
};

class ElementList : public QAbstractTableModel, public Savable
{
    Q_OBJECT
public:
    explicit ElementList(QObject *parent = nullptr);

    virtual nlohmann::json toJSON() override;
    virtual void fromJSON(nlohmann::json j) override;

    enum class Column {
        Name,
        Type,
        EpsilonR,
        Last,
    };

    void addElement(Element *e);
    bool removeElement(Element *e, bool del = true);
    bool removeElement(int index, bool del = true);
    Element *elementAt(int index) const;
    const QList<Element*> getElements() const {return elements;}
    double getDielectricConstantAt(const QPointF &p);

    int rowCount(const QModelIndex &parent) const override { Q_UNUSED(parent) return elements.size();}
    int columnCount(const QModelIndex &parent) const override {Q_UNUSED(parent) return (int) Column::Last;}
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const override;
    QVariant headerData(int section, Qt::Orientation orientation, int role = Qt::DisplayRole) const override;
    bool setData(const QModelIndex &index, const QVariant &value, int role = Qt::EditRole) override;
    Qt::ItemFlags flags(const QModelIndex &index) const override;

private:

    int findIndex(Element *e);

    QList<Element*> elements;
};

class Gauss : public QObject
{
    Q_OBJECT
public:
    explicit Gauss(QObject *parent = nullptr);

    static double getCharge(Laplace *laplace, ElementList *list, Element *e, double gridSize, double distance);

signals:
    void info(QString info);
    void warning(QString warning);
    void error(QString error);
};

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

class DifferentialMicrostrip
{
public:
    DifferentialMicrostrip();
    ElementList *createScenario();
    QPixmap getImage();

private:
    QString name;
    double width;
    double height;
    double gap;
    double substrate_height;
    double e_r;
    QList<Parameter> parameters;
};

#endif // MAIN_H
