#include <QWidget>

#include <QPointF>
#include "elementlist.h"
#include "laplace/laplace.h"

class PCBView : public QWidget
{
    Q_OBJECT
public:
    explicit PCBView(QWidget *parent = nullptr);

    void setCorners(QPointF topLeft, QPointF bottomRight);
    void setElementList(ElementList *list);
    void setLaplace(Laplace *laplace);

    void startAppending(Element *e);
    void setGrid(double grid);
    void setShowGrid(bool show);
    void setSnapToGrid(bool snap);
    void setShowPotential(bool show);
    void setKeepAspectRatio(bool keep);

    QPointF getTopLeft() const;

    QPointF getBottomRight() const;

signals:

protected:
    void paintEvent(QPaintEvent *event) override;
    void mousePressEvent(QMouseEvent *event) override;
    void mouseReleaseEvent(QMouseEvent *event) override;
    void mouseMoveEvent(QMouseEvent *event) override;
    void mouseDoubleClickEvent(QMouseEvent *event) override;
    void contextMenuEvent(QContextMenuEvent *event) override;
private:
    static const QColor backgroundColor;
    static const QColor GNDColor;
    static const QColor tracePosColor;
    static const QColor traceNegColor;
    static const QColor dielectricColor;
    static const QColor gridColor;
    static constexpr int vertexSize = 10;
    static constexpr int vertexCatchRadius = 15;
    double getPixelDistanceToVertex(QPoint cursor, QPointF vertex);
    void someElementChanged();

    using VertexInfo = struct {
        Element *e;
        int index;
    };
    VertexInfo catchVertex(QPoint cursor);

    using LineInfo = struct {
        Element *e;
        int index1, index2;
    };
    LineInfo catchLine(QPoint cursor);

    QPointF topLeft;
    QPointF bottomRight;
    QTransform transform;
    ElementList *list;
    Laplace *laplace;

    Element *appendElement;
    VertexInfo dragVertex;

    QPoint pressCoords;
    QPoint lastMouseCoords;
    bool pressCoordsValid;

    QPointF snapToGridPoint(const QPointF &pos);
    double grid;
    bool showGrid;
    bool snapToGrid;
    bool showPotential;
    bool keepAspectRatio;
};

#include <QPainter>
#include <QMouseEvent>
#include <QContextMenuEvent>
#include <QMenu>
#include <QAction>

#include "ui_vertexEditDialog.h"
#include "util.h"

#include "polygon.h"

const QColor PCBView::backgroundColor = Qt::lightGray;
const QColor PCBView::GNDColor = Qt::black;
const QColor PCBView::tracePosColor = Qt::red;
const QColor PCBView::traceNegColor = Qt::blue;
const QColor PCBView::dielectricColor = Qt::darkGreen;
const QColor PCBView::gridColor = Qt::gray;

PCBView::PCBView(QWidget *parent)
    : QWidget{parent}
{
    list = nullptr;
    laplace = nullptr;
    topLeft = QPointF(-1, 1);
    topLeft = QPointF(1, -1);
    appendElement = nullptr;
    dragVertex.e = nullptr;
    dragVertex.index = 0;
    pressCoordsValid = false;
    grid = 1e-4;
    showGrid = false;
    snapToGrid = false;
    showPotential = false;
    keepAspectRatio = true;
}

void PCBView::setCorners(QPointF topLeft, QPointF bottomRight)
{
    this->topLeft = topLeft;
    this->bottomRight = bottomRight;
}

void PCBView::setElementList(ElementList *list)
{
    this->list = list;
}

void PCBView::setLaplace(Laplace *laplace)
{
    this->laplace = laplace;
}

void PCBView::startAppending(Element *e)
{
    appendElement = e;
    setMouseTracking(true);
}

void PCBView::setGrid(double grid)
{
    this->grid = grid;
    update();
}

void PCBView::setShowGrid(bool show)
{
    showGrid = show;
    update();
}

void PCBView::setSnapToGrid(bool snap)
{
    snapToGrid = snap;
}

void PCBView::setShowPotential(bool show)
{
    showPotential = show;
    update();
}

void PCBView::setKeepAspectRatio(bool keep)
{
    keepAspectRatio = keep;
    update();
}

void PCBView::paintEvent(QPaintEvent *event)
{
    Q_UNUSED(event)

    QPainter p(this);

    p.setViewport(0, 0, width(), height());
    // p.setWindow is limited to integers, this implementation works with floating point
    transform = QTransform();
    if(keepAspectRatio) {
        // figure out which ratio is the limiting factor
        auto x_ratio = p.viewport().width() / (bottomRight.x() - topLeft.x());
        auto y_ratio = p.viewport().height() / (bottomRight.y() - topLeft.y());
        auto ratio = std::min(abs(x_ratio), abs(y_ratio));
        transform.scale(copysign(ratio, x_ratio), copysign(ratio, y_ratio));

        auto above = (topLeft.y() - bottomRight.y()) * (abs(y_ratio) / ratio - 1) / 2;
        auto left = (topLeft.x() - bottomRight.x()) * (abs(x_ratio) / ratio - 1) / 2;

        transform.translate(-topLeft.x() - left, -topLeft.y() - above);
    } else {
        transform.scale(p.viewport().width() / (bottomRight.x() - topLeft.x()),
                        p.viewport().height() / (bottomRight.y() - topLeft.y()));
        transform.translate(-topLeft.x(), -topLeft.y());
    }

    p.fillRect(QRectF(transform.map(topLeft), transform.map(bottomRight)), Qt::lightGray);
    p.setBackground(QBrush(Qt::black));

    // show potential field
    // TODO make this optional
    if(showPotential && laplace && laplace->isResultReady()) {
        for(int i=0;i<width();i++) {
            for(int j=0;j<height();j++) {
                auto coord = transform.inverted().map(QPointF(i, j));
                auto v = laplace->getPotential(coord);
                p.setPen(Util::getIntensityGradeColor(v));
                p.setOpacity(sqrt(abs(v)));
                p.drawPoint(i, j);
            }
        }
    }
    p.setOpacity(1.0);

    // draw grid
    if(showGrid) {
        // x axis
        p.setPen(gridColor);
        for(double x = snapToGridPoint(topLeft).x(); x < bottomRight.x(); x += grid) {
            auto top = transform.map(QPointF(x, topLeft.y()));
            auto bottom = transform.map(QPointF(x, bottomRight.y()));
            p.drawLine(top, bottom);
        }
        // y axis
        for(double y = snapToGridPoint(bottomRight).y(); y < topLeft.y(); y += grid) {
            auto left = transform.map(QPointF(topLeft.x(), y));
            auto right = transform.map(QPointF(bottomRight.x(), y));
            p.drawLine(left, right);
        }
    }

    // Show elements
    if(list) {
        for(auto e : list->getElements()) {
            QColor elementColor;
            switch(e->getType()) {
            case Element::Type::Dielectric: elementColor = dielectricColor; break;
            case Element::Type::TracePos: elementColor = tracePosColor; break;
            case Element::Type::TraceNeg: elementColor = traceNegColor; break;
            case Element::Type::GND: elementColor = GNDColor; break;
            default: elementColor = Qt::gray; break;
            }
            p.setBrush(elementColor);
            p.setPen(elementColor);

            auto vertices = e->getVertices();
            // paint vertices in viewport to get constant vertex size
            for(auto v : vertices) {
                auto devicePoint = transform.map(v);
                p.drawEllipse(devicePoint, vertexSize/2, vertexSize/2);
            }
            // draw connections between vertices
            if(vertices.size() > 1) {
                for(unsigned int i=0;i<vertices.size();i++) {
                    int prev = i-1;
                    if(prev < 0) {
                        if(e == appendElement) {
                            // we are appending to this element, do not draw last line in polygon
                            continue;
                        }
                        prev = vertices.size() -  1;
                    }
                    QPointF start = transform.map(vertices[i]);
                    QPointF stop = transform.map(vertices[prev]);
                    p.drawLine(start, stop);
                }
            }
            if(vertices.size() > 0 && e == appendElement){
                        // draw line from last vertex to pointer
                        QPointF start = transform.map(vertices[vertices.size()-1]);
                        QPointF stop = lastMouseCoords;
                        if(snapToGrid) {
                            stop = transform.map(snapToGridPoint(transform.inverted().map(stop)));
                        }
                        p.drawLine(start, stop);
            }
        }
    }
}

void PCBView::mousePressEvent(QMouseEvent *event)
{
    if (appendElement) {
        // check if we clicked on the first vertex
        auto vertices = appendElement->getVertices();
        if(vertices.size() > 0 && getPixelDistanceToVertex(event->pos(), vertices[0]) < vertexCatchRadius) {
            // clicked on the first element again, abort append mode
            appendElement = nullptr;
            setMouseTracking(false);
            update();
        } else {
            // record coordinates to place vertex when the mouse is released
            pressCoords = event->pos();
            lastMouseCoords = pressCoords;
            pressCoordsValid = true;
        }
    } else {
        // not appending, may have been a click on a vertex
        dragVertex = catchVertex(event->pos());
    }
}

void PCBView::mouseReleaseEvent(QMouseEvent *event)
{
    Q_UNUSED(event);
    if (appendElement && pressCoordsValid) {
        // add vertex at indicated coordinates
        auto vertexPoint = transform.inverted().map(QPointF(pressCoords));
        if(snapToGrid) {
            vertexPoint = snapToGridPoint(vertexPoint);
        }
        appendElement->appendVertex(vertexPoint);
        someElementChanged();
        pressCoordsValid = false;
        update();
    } else if (dragVertex.e) {
        dragVertex.e = nullptr;
    }
}

void PCBView::mouseMoveEvent(QMouseEvent *event)
{
    if (appendElement) {
        // ignore, just record mouse position
        lastMouseCoords = event->pos();
        update();
    } else if(dragVertex.e) {
        // dragging a vertex
        auto vertexPoint = transform.inverted().map(QPointF(event->pos()));
        if(snapToGrid) {
            vertexPoint = snapToGridPoint(vertexPoint);
        }
        dragVertex.e->changeVertex(dragVertex.index, vertexPoint);
        someElementChanged();
        update();
    }
}

void PCBView::mouseDoubleClickEvent(QMouseEvent *event)
{
    if (appendElement) {
        // double-click aborts appending
        appendElement = nullptr;
        setMouseTracking(false);
        update();
    } else {
        auto info = catchVertex(event->pos());
        if(info.e) {
            // edit vertex coordinates
            auto d = new QDialog(this);
            d->setAttribute(Qt::WA_DeleteOnClose);
            auto ui = new Ui::VertexEditDialog;
            ui->setupUi(d);

            // save previous coordinates
            auto oldCoords = info.e->getVertices()[info.index];

            auto updateVertex = [=](const QPointF &p){
                info.e->changeVertex(info.index, p);
                update();
            };

            ui->xpos->setUnit("m");
            ui->xpos->setPrefixes("um ");
            ui->xpos->setPrecision(4);
            ui->xpos->setValue(oldCoords.x());
            connect(ui->xpos, &SIUnitEdit::valueChanged, this, [=](){
                updateVertex(QPointF(ui->xpos->value(), ui->ypos->value()));
            });

            ui->ypos->setUnit("m");
            ui->ypos->setPrefixes("um ");
            ui->ypos->setPrecision(4);
            ui->ypos->setValue(oldCoords.y());
            connect(ui->ypos, &SIUnitEdit::valueChanged, this, [=](){
                updateVertex(QPointF(ui->xpos->value(), ui->ypos->value()));
            });

            connect(ui->buttonBox, &QDialogButtonBox::accepted, d, &QDialog::accept);
            connect(ui->buttonBox, &QDialogButtonBox::rejected, this, [=](){
                // restore old coordinates
                info.e->changeVertex(info.index, oldCoords);
                update();
                d->reject();
            });

            d->show();
        }
    }
}

void PCBView::contextMenuEvent(QContextMenuEvent *event)
{
    if (appendElement) {
        // ignore
        return;
    }
    auto menu = new QMenu();
    auto infoVertex = catchVertex(event->pos());
    auto infoLine = catchLine(event->pos());
    Element *e = nullptr;
    if(infoVertex.e) {
        e = infoVertex.e;
        // clicked on a vertex
        auto actionDeleteVertex = new QAction("Delete Vertex");
        menu->addAction(actionDeleteVertex);
        connect(actionDeleteVertex, &QAction::triggered, [=](){
            infoVertex.e->removeVertex(infoVertex.index);
            someElementChanged();
        });
    } else if(infoLine.e) {
        e = infoLine.e;
        // clicked on a line
        auto actionInsertVertex = new QAction("Insert Vertex here");
        menu->addAction(actionInsertVertex);
        connect(actionInsertVertex, &QAction::triggered, [=](){
            // figure out at which index we have to insert the new vertex.
            // Usually this is the higher of the two but there is a special case if one index is the first vertex and the other the last
            int insertIndex = std::max(infoLine.index1, infoLine.index2);
            if((infoLine.index1 == 0 && infoLine.index2 == infoLine.e->getVertices().size() - 1)
                    || (infoLine.index1 == 0 && infoLine.index2 == infoLine.e->getVertices().size() - 1)) {
                // special case
                insertIndex++;
            }
            auto vertexPoint = transform.inverted().map(QPointF(event->pos()));
            infoLine.e->addVertex(insertIndex, vertexPoint);
            someElementChanged();
        });
    }
    // TODO check if connection between vertices was clicked
    if(e) {
        // clicked on something connected to an element
        auto actionDeleteElement = new QAction("Delete Element");
        menu->addAction(actionDeleteElement);
        connect(actionDeleteElement, &QAction::triggered, [=](){
            list->removeElement(e);
            someElementChanged();
        });
    }
    menu->exec(event->globalPos());
    update();
}

double PCBView::getPixelDistanceToVertex(QPoint cursor, QPointF vertex)
{
    // convert vertex into pixel coordinates
    QPoint vertexPixel = transform.map(vertex).toPoint();
    auto diff = vertexPixel - cursor;
    return std::sqrt(diff.x()*diff.x()+diff.y()*diff.y());
}

void PCBView::someElementChanged()
{
    if(laplace && laplace->isResultReady()) {
        laplace->invalidateResult();
    }
}

PCBView::VertexInfo PCBView::catchVertex(QPoint cursor)
{
    VertexInfo info;
    info.e = nullptr;
    double closestDistance = vertexCatchRadius;
    for(auto e : list->getElements()) {
        for(unsigned int i=0;i<e->getVertices().size();i++) {
            auto distance = getPixelDistanceToVertex(cursor, e->getVertices()[i]);
            if(distance < closestDistance) {
                closestDistance = distance;
                info.e = e;
                info.index = i;
            }
        }
    }
    return info;
}

PCBView::LineInfo PCBView::catchLine(QPoint cursor)
{
    LineInfo info;
    info.e = nullptr;
    double closestDistance = vertexCatchRadius;
    for(auto e : list->getElements()) {
        if(e->getVertices().size() < 2) {
            continue;
        }
        for(unsigned int i=0;i<e->getVertices().size();i++) {
            int prev = (int) i - 1;
            if(prev < 0) {
                prev = e->getVertices().size() - 1;
            }
            QPointF vertexPixel1 = transform.map(e->getVertices()[i]).toPoint();
            QPointF vertexPixel2 = transform.map(e->getVertices()[prev]).toPoint();
            auto distance = Util::distanceToLine(QPointF(cursor), vertexPixel1, vertexPixel2);
            if(distance < closestDistance) {
                closestDistance = distance;
                info.e = e;
                info.index1 = i;
                info.index2 = prev;
            }
        }
    }
    return info;
}

QPointF PCBView::getBottomRight() const
{
    return bottomRight;
}

QPointF PCBView::getTopLeft() const
{
    return topLeft;
}

QPointF PCBView::snapToGridPoint(const QPointF &pos)
{
    double snap_x = std::round(pos.x() / grid) * grid;
    double snap_y = std::round(pos.y() / grid) * grid;
    return QPointF(snap_x, snap_y);
}
