#include "pcbview.h"

#include <System.Drawing.h>
#include <System.Windows.Forms.h>

#include "ui_vertexEditDialog.h"
#include "util.h"

#include "polygon.h"

const System::Drawing::Color PCBView::backgroundColor = System::Drawing::Color::LightGray;
const System::Drawing::Color PCBView::GNDColor = System::Drawing::Color::Black;
const System::Drawing::Color PCBView::tracePosColor = System::Drawing::Color::Red;
const System::Drawing::Color PCBView::traceNegColor = System::Drawing::Color::Blue;
const System::Drawing::Color PCBView::dielectricColor = System::Drawing::Color::DarkGreen;
const System::Drawing::Color PCBView::gridColor = System::Drawing::Color::Gray;

PCBView::PCBView(System::Windows::Forms::UserControl^ parent)
    : UserControl{parent}
{
    list = nullptr;
    laplace = nullptr;
    topLeft = System::Drawing::PointF(-1, 1);
    topLeft = System::Drawing::PointF(1, -1);
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

void PCBView::setCorners(System::Drawing::PointF topLeft, System::Drawing::PointF bottomRight)
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
    this->MouseMove += gcnew System::Windows::Forms::MouseEventHandler(this, &PCBView::mouseMoveEvent);
}

void PCBView::setGrid(double grid)
{
    this->grid = grid;
    this->Invalidate();
}

void PCBView::setShowGrid(bool show)
{
    showGrid = show;
    this->Invalidate();
}

void PCBView::setSnapToGrid(bool snap)
{
    snapToGrid = snap;
}

void PCBView::setShowPotential(bool show)
{
    showPotential = show;
    this->Invalidate();
}

void PCBView::setKeepAspectRatio(bool keep)
{
    keepAspectRatio = keep;
    this->Invalidate();
}

void PCBView::OnPaint(System::Windows::Forms::PaintEventArgs^ event) override
{
    System::Drawing::Graphics^ p = event->Graphics;

    p->SetClip(System::Drawing::Rectangle(0, 0, this->Width, this->Height));
    // p->SetClip is limited to integers, this implementation works with floating point
    transform = gcnew System::Drawing::Drawing2D::Matrix();
    if(keepAspectRatio) {
        // figure out which ratio is the limiting factor
        auto x_ratio = p->ClipBounds.Width / (bottomRight.X - topLeft.X);
        auto y_ratio = p->ClipBounds.Height / (bottomRight.Y - topLeft.Y);
        auto ratio = std::min(abs(x_ratio), abs(y_ratio));
        transform->Scale(copysign(ratio, x_ratio), copysign(ratio, y_ratio));

        auto above = (topLeft.Y - bottomRight.Y) * (abs(y_ratio) / ratio - 1) / 2;
        auto left = (topLeft.X - bottomRight.X) * (abs(x_ratio) / ratio - 1) / 2;

        transform->Translate(-topLeft.X - left, -topLeft.Y - above);
    } else {
        transform->Scale(p->ClipBounds.Width / (bottomRight.X - topLeft.X),
                        p->ClipBounds.Height / (bottomRight.Y - topLeft.Y));
        transform->Translate(-topLeft.X, -topLeft.Y);
    }

    p->FillRectangle(gcnew System::Drawing::SolidBrush(System::Drawing::Color::LightGray), System::Drawing::RectangleF(transform->TransformPoint(topLeft), transform->TransformPoint(bottomRight)));
    p->SetClip(System::Drawing::Rectangle(0, 0, this->Width, this->Height));

    // show potential field
    // TODO make this optional
    if(showPotential && laplace && laplace->isResultReady()) {
        for(int i=0;i<this->Width;i++) {
            for(int j=0;j<this->Height;j++) {
                auto coord = transform->TransformPoint(System::Drawing::PointF(i, j));
                auto v = laplace->getPotential(coord);
                p->DrawLine(gcnew System::Drawing::Pen(Util::getIntensityGradeColor(v)), i, j, i, j);
            }
        }
    }

    // draw grid
    if(showGrid) {
        // x axis
        p->DrawLine(gcnew System::Drawing::Pen(gridColor), transform->TransformPoint(System::Drawing::PointF(topLeft.X, topLeft.Y)), transform->TransformPoint(System::Drawing::PointF(bottomRight.X, topLeft.Y)));
        for(double x = snapToGridPoint(topLeft).X; x < bottomRight.X; x += grid) {
            auto top = transform->TransformPoint(System::Drawing::PointF(x, topLeft.Y));
            auto bottom = transform->TransformPoint(System::Drawing::PointF(x, bottomRight.Y));
            p->DrawLine(gcnew System::Drawing::Pen(gridColor), top, bottom);
        }
        // y axis
        for(double y = snapToGridPoint(bottomRight).Y; y < topLeft.Y; y += grid) {
            auto left = transform->TransformPoint(System::Drawing::PointF(topLeft.X, y));
            auto right = transform->TransformPoint(System::Drawing::PointF(bottomRight.X, y));
            p->DrawLine(gcnew System::Drawing::Pen(gridColor), left, right);
        }
    }

    // Show elements
    if(list) {
        for(auto e : list->getElements()) {
            System::Drawing::Color elementColor;
            switch(e->getType()) {
            case Element::Type::Dielectric: elementColor = dielectricColor; break;
            case Element::Type::TracePos: elementColor = tracePosColor; break;
            case Element::Type::TraceNeg: elementColor = traceNegColor; break;
            case Element::Type::GND: elementColor = GNDColor; break;
            default: elementColor = System::Drawing::Color::Gray; break;
            }
            p->FillRectangle(gcnew System::Drawing::SolidBrush(elementColor), System::Drawing::RectangleF(transform->TransformPoint(e->getVertices()[0]), System::Drawing::SizeF(vertexSize, vertexSize)));
            p->DrawRectangle(gcnew System::Drawing::Pen(elementColor), System::Drawing::RectangleF(transform->TransformPoint(e->getVertices()[0]), System::Drawing::SizeF(vertexSize, vertexSize)));

            auto vertices = e->getVertices();
            // paint vertices in viewport to get constant vertex size
            for(auto v : vertices) {
                auto devicePoint = transform->TransformPoint(v);
                p->FillEllipse(gcnew System::Drawing::SolidBrush(elementColor), System::Drawing::RectangleF(devicePoint, System::Drawing::SizeF(vertexSize, vertexSize)));
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
                    System::Drawing::PointF start = transform->TransformPoint(vertices[i]);
                    System::Drawing::PointF stop = transform->TransformPoint(vertices[prev]);
                    p->DrawLine(gcnew System::Drawing::Pen(elementColor), start, stop);
                }
            }
            if(vertices.size() > 0 && e == appendElement){
                        // draw line from last vertex to pointer
                        System::Drawing::PointF start = transform->TransformPoint(vertices[vertices.size()-1]);
                        System::Drawing::PointF stop = lastMouseCoords;
                        if(snapToGrid) {
                            stop = transform->TransformPoint(snapToGridPoint(transform->TransformPoint(stop)));
                        }
                        p->DrawLine(gcnew System::Drawing::Pen(elementColor), start, stop);
            }
        }
    }
}

void PCBView::mousePressEvent(System::Windows::Forms::MouseEventArgs^ event)
{
    if (appendElement) {
        // check if we clicked on the first vertex
        auto vertices = appendElement->getVertices();
        if(vertices.size() > 0 && getPixelDistanceToVertex(event->Location, vertices[0]) < vertexCatchRadius) {
            // clicked on the first element again, abort append mode
            appendElement = nullptr;
            this->MouseMove -= gcnew System::Windows::Forms::MouseEventHandler(this, &PCBView::mouseMoveEvent);
            this->Invalidate();
        } else {
            // record coordinates to place vertex when the mouse is released
            pressCoords = event->Location;
            lastMouseCoords = pressCoords;
            pressCoordsValid = true;
        }
    } else {
        // not appending, may have been a click on a vertex
        dragVertex = catchVertex(event->Location);
    }
}

void PCBView::mouseReleaseEvent(System::Windows::Forms::MouseEventArgs^ event)
{
    if (appendElement && pressCoordsValid) {
        // add vertex at indicated coordinates
        auto vertexPoint = transform->TransformPoint(System::Drawing::PointF(pressCoords.X, pressCoords.Y));
        if(snapToGrid) {
            vertexPoint = snapToGridPoint(vertexPoint);
        }
        appendElement->appendVertex(vertexPoint);
        someElementChanged();
        pressCoordsValid = false;
        this->Invalidate();
    } else if (dragVertex.e) {
        dragVertex.e = nullptr;
    }
}

void PCBView::mouseMoveEvent(System::Windows::Forms::MouseEventArgs^ event)
{
    if (appendElement) {
        // ignore, just record mouse position
        lastMouseCoords = event->Location;
        this->Invalidate();
    } else if(dragVertex.e) {
        // dragging a vertex
        auto vertexPoint = transform->TransformPoint(System::Drawing::PointF(event->Location.X, event->Location.Y));
        if(snapToGrid) {
            vertexPoint = snapToGridPoint(vertexPoint);
        }
        dragVertex.e->changeVertex(dragVertex.index, vertexPoint);
        someElementChanged();
        this->Invalidate();
    }
}

void PCBView::mouseDoubleClickEvent(System::Windows::Forms::MouseEventArgs^ event)
{
    if (appendElement) {
        // double-click aborts appending
        appendElement = nullptr;
        this->MouseMove -= gcnew System::Windows::Forms::MouseEventHandler(this, &PCBView::mouseMoveEvent);
        this->Invalidate();
    } else {
        auto info = catchVertex(event->Location);
        if(info.e) {
            // edit vertex coordinates
            auto d = gcnew System::Windows::Forms::Form();
            d->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
            d->ShowInTaskbar = false;
            d->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
            d->MinimizeBox = false;
            d->MaximizeBox = false;
            d->ShowIcon = false;
            d->Text = "Edit Vertex";

            auto ui = gcnew Ui::VertexEditDialog;
            ui->setupUi(d);

            // save previous coordinates
            auto oldCoords = info.e->getVertices()[info.index];

            auto updateVertex = [=](const System::Drawing::PointF &p){
                info.e->changeVertex(info.index, p);
                this->Invalidate();
            };

            ui->xpos->setUnit("m");
            ui->xpos->setPrefixes("um ");
            ui->xpos->setPrecision(4);
            ui->xpos->setValue(oldCoords.X);
            ui->xpos->ValueChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
                updateVertex(System::Drawing::PointF(ui->xpos->Value, ui->ypos->Value));
            });

            ui->ypos->setUnit("m");
            ui->ypos->setPrefixes("um ");
            ui->ypos->setPrecision(4);
            ui->ypos->setValue(oldCoords.Y);
            ui->ypos->ValueChanged += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
                updateVertex(System::Drawing::PointF(ui->xpos->Value, ui->ypos->Value));
            });

            ui->buttonBox->Accepted += gcnew System::EventHandler(d, &System::Windows::Forms::Form::Close);
            ui->buttonBox->Rejected += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
                // restore old coordinates
                info.e->changeVertex(info.index, oldCoords);
                this->Invalidate();
                d->Close();
            });

            d->ShowDialog();
        }
    }
}

void PCBView::contextMenuEvent(System::Windows::Forms::MouseEventArgs^ event)
{
    if (appendElement) {
        // ignore
        return;
    }
    auto menu = gcnew System::Windows::Forms::ContextMenuStrip();
    auto infoVertex = catchVertex(event->Location);
    auto infoLine = catchLine(event->Location);
    Element *e = nullptr;
    if(infoVertex.e) {
        e = infoVertex.e;
        // clicked on a vertex
        auto actionDeleteVertex = gcnew System::Windows::Forms::ToolStripMenuItem("Delete Vertex");
        menu->Items->Add(actionDeleteVertex);
        actionDeleteVertex->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
            infoVertex.e->removeVertex(infoVertex.index);
            someElementChanged();
        });
    } else if(infoLine.e) {
        e = infoLine.e;
        // clicked on a line
        auto actionInsertVertex = gcnew System::Windows::Forms::ToolStripMenuItem("Insert Vertex here");
        menu->Items->Add(actionInsertVertex);
        actionInsertVertex->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
            // figure out at which index we have to insert the new vertex.
            // Usually this is the higher of the two but there is a special case if one index is the first vertex and the other the last
            int insertIndex = std::max(infoLine.index1, infoLine.index2);
            if((infoLine.index1 == 0 && infoLine.index2 == infoLine.e->getVertices().size() - 1)
                    || (infoLine.index1 == 0 && infoLine.index2 == infoLine.e->getVertices().size() - 1)) {
                // special case
                insertIndex++;
            }
            auto vertexPoint = transform->TransformPoint(System::Drawing::PointF(event->Location.X, event->Location.Y));
            infoLine.e->addVertex(insertIndex, vertexPoint);
            someElementChanged();
        });
    }
    // TODO check if connection between vertices was clicked
    if(e) {
        // clicked on something connected to an element
        auto actionDeleteElement = gcnew System::Windows::Forms::ToolStripMenuItem("Delete Element");
        menu->Items->Add(actionDeleteElement);
        actionDeleteElement->Click += gcnew System::EventHandler(this, [=](System::Object^ sender, System::EventArgs^ e){
            list->removeElement(e);
            someElementChanged();
        });
    }
    menu->Show(this, event->Location);
    this->Invalidate();
}

double PCBView::getPixelDistanceToVertex(System::Drawing::Point cursor, System::Drawing::PointF vertex)
{
    // convert vertex into pixel coordinates
    System::Drawing::Point vertexPixel = transform->TransformPoint(vertex).ToPoint();
    auto diff = System::Drawing::Point(vertexPixel.X - cursor.X, vertexPixel.Y - cursor.Y);
    return std::sqrt(diff.X*diff.X+diff.Y*diff.Y);
}

void PCBView::someElementChanged()
{
    if(laplace && laplace->isResultReady()) {
        laplace->invalidateResult();
    }
}

PCBView::VertexInfo PCBView::catchVertex(System::Drawing::Point cursor)
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

PCBView::LineInfo PCBView::catchLine(System::Drawing::Point cursor)
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
            System::Drawing::PointF vertexPixel1 = transform->TransformPoint(e->getVertices()[i]).ToPoint();
            System::Drawing::PointF vertexPixel2 = transform->TransformPoint(e->getVertices()[prev]).ToPoint();
            auto distance = Util::distanceToLine(System::Drawing::PointF(cursor.X, cursor.Y), vertexPixel1, vertexPixel2);
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

System::Drawing::PointF PCBView::getBottomRight() const
{
    return bottomRight;
}

System::Drawing::PointF PCBView::getTopLeft() const
{
    return topLeft;
}

System::Drawing::PointF PCBView::snapToGridPoint(const System::Drawing::PointF &pos)
{
    double snap_x = std::round(pos.X / grid) * grid;
    double snap_y = std::round(pos.Y / grid) * grid;
    return System::Drawing::PointF(snap_x, snap_y);
}
