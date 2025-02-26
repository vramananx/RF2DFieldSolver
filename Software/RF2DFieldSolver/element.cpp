#include <QObject>
#include <QList>
#include <QPointF>
#include <QPolygonF>
#include "savable.h"

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
