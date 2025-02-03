using System;
using System.Drawing;
using System.Windows.Forms;

public class PCBView : Control
{
    private static readonly Color backgroundColor = Color.LightGray;
    private static readonly Color GNDColor = Color.Black;
    private static readonly Color tracePosColor = Color.Red;
    private static readonly Color traceNegColor = Color.Blue;
    private static readonly Color dielectricColor = Color.DarkGreen;
    private static readonly Color gridColor = Color.Gray;
    private const int vertexSize = 10;
    private const int vertexCatchRadius = 15;

    private PointF topLeft;
    private PointF bottomRight;
    private Matrix transform;
    private ElementList list;
    private Laplace laplace;
    private Element appendElement;
    private VertexInfo dragVertex;
    private Point pressCoords;
    private Point lastMouseCoords;
    private bool pressCoordsValid;
    private double grid;
    private bool showGrid;
    private bool snapToGrid;
    private bool showPotential;
    private bool keepAspectRatio;

    public PCBView()
    {
        list = null;
        laplace = null;
        topLeft = new PointF(-1, 1);
        bottomRight = new PointF(1, -1);
        appendElement = null;
        dragVertex = new VertexInfo();
        pressCoordsValid = false;
        grid = 1e-4;
        showGrid = false;
        snapToGrid = false;
        showPotential = false;
        keepAspectRatio = true;
    }

    public void SetCorners(PointF topLeft, PointF bottomRight)
    {
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
    }

    public void SetElementList(ElementList list)
    {
        this.list = list;
    }

    public void SetLaplace(Laplace laplace)
    {
        this.laplace = laplace;
    }

    public void StartAppending(Element e)
    {
        appendElement = e;
        this.MouseMove += PCBView_MouseMove;
    }

    public void SetGrid(double grid)
    {
        this.grid = grid;
        Invalidate();
    }

    public void SetShowGrid(bool show)
    {
        showGrid = show;
        Invalidate();
    }

    public void SetSnapToGrid(bool snap)
    {
        snapToGrid = snap;
    }

    public void SetShowPotential(bool show)
    {
        showPotential = show;
        Invalidate();
    }

    public void SetKeepAspectRatio(bool keep)
    {
        keepAspectRatio = keep;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.Clear(backgroundColor);

        transform = new Matrix();
        if (keepAspectRatio)
        {
            float x_ratio = (float)ClientSize.Width / (bottomRight.X - topLeft.X);
            float y_ratio = (float)ClientSize.Height / (bottomRight.Y - topLeft.Y);
            float ratio = Math.Min(Math.Abs(x_ratio), Math.Abs(y_ratio));
            transform.Scale(Math.Sign(x_ratio) * ratio, Math.Sign(y_ratio) * ratio);

            float above = (topLeft.Y - bottomRight.Y) * (Math.Abs(y_ratio) / ratio - 1) / 2;
            float left = (topLeft.X - bottomRight.X) * (Math.Abs(x_ratio) / ratio - 1) / 2;

            transform.Translate(-topLeft.X - left, -topLeft.Y - above);
        }
        else
        {
            transform.Scale((float)ClientSize.Width / (bottomRight.X - topLeft.X),
                            (float)ClientSize.Height / (bottomRight.Y - topLeft.Y));
            transform.Translate(-topLeft.X, -topLeft.Y);
        }

        g.Transform = transform;

        if (showPotential && laplace != null && laplace.IsResultReady())
        {
            for (int i = 0; i < ClientSize.Width; i++)
            {
                for (int j = 0; j < ClientSize.Height; j++)
                {
                    PointF coord = transform.TransformPoint(new PointF(i, j));
                    double v = laplace.GetPotential(coord);
                    g.FillRectangle(new SolidBrush(Util.GetIntensityGradeColor(v)), i, j, 1, 1);
                }
            }
        }

        if (showGrid)
        {
            Pen gridPen = new Pen(gridColor);
            for (double x = SnapToGridPoint(topLeft).X; x < bottomRight.X; x += grid)
            {
                PointF top = transform.TransformPoint(new PointF((float)x, topLeft.Y));
                PointF bottom = transform.TransformPoint(new PointF((float)x, bottomRight.Y));
                g.DrawLine(gridPen, top, bottom);
            }
            for (double y = SnapToGridPoint(bottomRight).Y; y < topLeft.Y; y += grid)
            {
                PointF left = transform.TransformPoint(new PointF(topLeft.X, (float)y));
                PointF right = transform.TransformPoint(new PointF(bottomRight.X, (float)y));
                g.DrawLine(gridPen, left, right);
            }
        }

        if (list != null)
        {
            foreach (Element e in list.GetElements())
            {
                Color elementColor;
                switch (e.GetType())
                {
                    case ElementType.Dielectric:
                        elementColor = dielectricColor;
                        break;
                    case ElementType.TracePos:
                        elementColor = tracePosColor;
                        break;
                    case ElementType.TraceNeg:
                        elementColor = traceNegColor;
                        break;
                    case ElementType.GND:
                        elementColor = GNDColor;
                        break;
                    default:
                        elementColor = Color.Gray;
                        break;
                }

                Brush elementBrush = new SolidBrush(elementColor);
                Pen elementPen = new Pen(elementColor);

                PointF[] vertices = e.GetVertices();
                foreach (PointF v in vertices)
                {
                    PointF devicePoint = transform.TransformPoint(v);
                    g.FillEllipse(elementBrush, devicePoint.X - vertexSize / 2, devicePoint.Y - vertexSize / 2, vertexSize, vertexSize);
                }

                if (vertices.Length > 1)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        int prev = i - 1;
                        if (prev < 0)
                        {
                            if (e == appendElement)
                            {
                                continue;
                            }
                            prev = vertices.Length - 1;
                        }
                        PointF start = transform.TransformPoint(vertices[i]);
                        PointF stop = transform.TransformPoint(vertices[prev]);
                        g.DrawLine(elementPen, start, stop);
                    }
                }

                if (vertices.Length > 0 && e == appendElement)
                {
                    PointF start = transform.TransformPoint(vertices[vertices.Length - 1]);
                    PointF stop = lastMouseCoords;
                    if (snapToGrid)
                    {
                        stop = transform.TransformPoint(SnapToGridPoint(transform.Invert().TransformPoint(stop)));
                    }
                    g.DrawLine(elementPen, start, stop);
                }
            }
        }
    }

    private void PCBView_MouseMove(object sender, MouseEventArgs e)
    {
        if (appendElement != null)
        {
            lastMouseCoords = e.Location;
            Invalidate();
        }
        else if (dragVertex.e != null)
        {
            PointF vertexPoint = transform.Invert().TransformPoint(e.Location);
            if (snapToGrid)
            {
                vertexPoint = SnapToGridPoint(vertexPoint);
            }
            dragVertex.e.ChangeVertex(dragVertex.index, vertexPoint);
            SomeElementChanged();
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (appendElement != null)
        {
            PointF[] vertices = appendElement.GetVertices();
            if (vertices.Length > 0 && GetPixelDistanceToVertex(e.Location, vertices[0]) < vertexCatchRadius)
            {
                appendElement = null;
                this.MouseMove -= PCBView_MouseMove;
                Invalidate();
            }
            else
            {
                pressCoords = e.Location;
                lastMouseCoords = pressCoords;
                pressCoordsValid = true;
            }
        }
        else
        {
            dragVertex = CatchVertex(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (appendElement != null && pressCoordsValid)
        {
            PointF vertexPoint = transform.Invert().TransformPoint(pressCoords);
            if (snapToGrid)
            {
                vertexPoint = SnapToGridPoint(vertexPoint);
            }
            appendElement.AppendVertex(vertexPoint);
            SomeElementChanged();
            pressCoordsValid = false;
            Invalidate();
        }
        else if (dragVertex.e != null)
        {
            dragVertex.e = null;
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (appendElement != null)
        {
            appendElement = null;
            this.MouseMove -= PCBView_MouseMove;
            Invalidate();
        }
        else
        {
            VertexInfo info = CatchVertex(e.Location);
            if (info.e != null)
            {
                VertexEditDialog d = new VertexEditDialog();
                d.SetVertex(info.e, info.index);
                d.ShowDialog();
                Invalidate();
            }
        }
    }

    protected override void OnContextMenu(MouseEventArgs e)
    {
        base.OnContextMenu(e);

        if (appendElement != null)
        {
            return;
        }

        ContextMenu menu = new ContextMenu();
        VertexInfo infoVertex = CatchVertex(e.Location);
        LineInfo infoLine = CatchLine(e.Location);
        Element e = null;

        if (infoVertex.e != null)
        {
            e = infoVertex.e;
            MenuItem deleteVertex = new MenuItem("Delete Vertex");
            deleteVertex.Click += (s, args) =>
            {
                infoVertex.e.RemoveVertex(infoVertex.index);
                SomeElementChanged();
            };
            menu.MenuItems.Add(deleteVertex);
        }
        else if (infoLine.e != null)
        {
            e = infoLine.e;
            MenuItem insertVertex = new MenuItem("Insert Vertex here");
            insertVertex.Click += (s, args) =>
            {
                int insertIndex = Math.Max(infoLine.index1, infoLine.index2);
                if ((infoLine.index1 == 0 && infoLine.index2 == infoLine.e.GetVertices().Length - 1) ||
                    (infoLine.index1 == infoLine.e.GetVertices().Length - 1 && infoLine.index2 == 0))
                {
                    insertIndex++;
                }
                PointF vertexPoint = transform.Invert().TransformPoint(e.Location);
                infoLine.e.AddVertex(insertIndex, vertexPoint);
                SomeElementChanged();
            };
            menu.MenuItems.Add(insertVertex);
        }

        if (e != null)
        {
            MenuItem deleteElement = new MenuItem("Delete Element");
            deleteElement.Click += (s, args) =>
            {
                list.RemoveElement(e);
                SomeElementChanged();
            };
            menu.MenuItems.Add(deleteElement);
        }

        menu.Show(this, e.Location);
        Invalidate();
    }

    private double GetPixelDistanceToVertex(Point cursor, PointF vertex)
    {
        Point vertexPixel = transform.TransformPoint(vertex).ToPoint();
        Point diff = new Point(vertexPixel.X - cursor.X, vertexPixel.Y - cursor.Y);
        return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
    }

    private void SomeElementChanged()
    {
        if (laplace != null && laplace.IsResultReady())
        {
            laplace.InvalidateResult();
        }
    }

    private VertexInfo CatchVertex(Point cursor)
    {
        VertexInfo info = new VertexInfo();
        double closestDistance = vertexCatchRadius;
        foreach (Element e in list.GetElements())
        {
            PointF[] vertices = e.GetVertices();
            for (int i = 0; i < vertices.Length; i++)
            {
                double distance = GetPixelDistanceToVertex(cursor, vertices[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    info.e = e;
                    info.index = i;
                }
            }
        }
        return info;
    }

    private LineInfo CatchLine(Point cursor)
    {
        LineInfo info = new LineInfo();
        double closestDistance = vertexCatchRadius;
        foreach (Element e in list.GetElements())
        {
            PointF[] vertices = e.GetVertices();
            if (vertices.Length < 2)
            {
                continue;
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                int prev = i - 1;
                if (prev < 0)
                {
                    prev = vertices.Length - 1;
                }
                PointF vertexPixel1 = transform.TransformPoint(vertices[i]);
                PointF vertexPixel2 = transform.TransformPoint(vertices[prev]);
                double distance = Util.DistanceToLine(new PointF(cursor.X, cursor.Y), vertexPixel1, vertexPixel2);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    info.e = e;
                    info.index1 = i;
                    info.index2 = prev;
                }
            }
        }
        return info;
    }

    public PointF GetBottomRight()
    {
        return bottomRight;
    }

    public PointF GetTopLeft()
    {
        return topLeft;
    }

    private PointF SnapToGridPoint(PointF pos)
    {
        float snap_x = (float)Math.Round(pos.X / grid) * (float)grid;
        float snap_y = (float)Math.Round(pos.Y / grid) * (float)grid;
        return new PointF(snap_x, snap_y);
    }

    private class VertexInfo
    {
        public Element e;
        public int index;
    }

    private class LineInfo
    {
        public Element e;
        public int index1;
        public int index2;
    }
}
