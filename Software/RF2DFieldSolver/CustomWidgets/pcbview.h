#ifndef PCBVIEW_H
#define PCBVIEW_H

#include <Windows.h>
#include <System.Drawing.h>
#include <System.Windows.Forms.h>

class PCBView : public System::Windows::Forms::UserControl
{
public:
    explicit PCBView(System::Windows::Forms::UserControl^ parent = nullptr);

    void setCorners(System::Drawing::PointF topLeft, System::Drawing::PointF bottomRight);
    void setElementList(ElementList *list);
    void setLaplace(Laplace *laplace);

    void startAppending(Element *e);
    void setGrid(double grid);
    void setShowGrid(bool show);
    void setSnapToGrid(bool snap);
    void setShowPotential(bool show);
    void setKeepAspectRatio(bool keep);

    System::Drawing::PointF getTopLeft() const;

    System::Drawing::PointF getBottomRight() const;

protected:
    void OnPaint(System::Windows::Forms::PaintEventArgs^ event) override;
    void mousePressEvent(System::Windows::Forms::MouseEventArgs^ event);
    void mouseReleaseEvent(System::Windows::Forms::MouseEventArgs^ event);
    void mouseMoveEvent(System::Windows::Forms::MouseEventArgs^ event);
    void mouseDoubleClickEvent(System::Windows::Forms::MouseEventArgs^ event);
    void contextMenuEvent(System::Windows::Forms::MouseEventArgs^ event);

private:
    static const System::Drawing::Color backgroundColor;
    static const System::Drawing::Color GNDColor;
    static const System::Drawing::Color tracePosColor;
    static const System::Drawing::Color traceNegColor;
    static const System::Drawing::Color dielectricColor;
    static const System::Drawing::Color gridColor;
    static constexpr int vertexSize = 10;
    static constexpr int vertexCatchRadius = 15;
    double getPixelDistanceToVertex(System::Drawing::Point cursor, System::Drawing::PointF vertex);
    void someElementChanged();

    using VertexInfo = struct {
        Element *e;
        int index;
    };
    VertexInfo catchVertex(System::Drawing::Point cursor);

    using LineInfo = struct {
        Element *e;
        int index1, index2;
    };
    LineInfo catchLine(System::Drawing::Point cursor);

    System::Drawing::PointF topLeft;
    System::Drawing::PointF bottomRight;
    System::Drawing::Drawing2D::Matrix^ transform;
    ElementList *list;
    Laplace *laplace;

    Element *appendElement;
    VertexInfo dragVertex;

    System::Drawing::Point pressCoords;
    System::Drawing::Point lastMouseCoords;
    bool pressCoordsValid;

    System::Drawing::PointF snapToGridPoint(const System::Drawing::PointF &pos);
    double grid;
    bool showGrid;
    bool snapToGrid;
    bool showPotential;
    bool keepAspectRatio;
};

#endif // PCBVIEW_H
