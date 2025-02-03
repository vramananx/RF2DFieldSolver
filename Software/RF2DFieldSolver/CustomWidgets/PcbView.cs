using System;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.CustomWidgets
{
    public class PcbView : Control
    {
        private ElementList list;
        private Laplace laplace;
        private PointF topLeft;
        private PointF bottomRight;
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
        private const int vertexSize = 10;
        private const int vertexCatchRadius = 15;
        private static readonly Color backgroundColor = Color.LightGray;
        private static readonly Color GNDColor = Color.Black;
        private static readonly Color tracePosColor = Color.Red;
        private static readonly Color traceNegColor = Color.Blue;
        private static readonly Color dielectricColor = Color.DarkGreen;
        private static readonly Color gridColor = Color.Gray;

        public PcbView()
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
            this.MouseMove += PcbView_MouseMove;
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

            // Show potential field
            if (showPotential && laplace != null && laplace.IsResultReady())
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        PointF coord = Transform.InverseTransformPoint(new PointF(i, j));
                        double v = laplace.GetPotential(coord);
                        g.FillRectangle(new SolidBrush(Util.GetIntensityGradeColor(v)), i, j, 1, 1);
                    }
                }
            }

            // Draw grid
            if (showGrid)
            {
                Pen gridPen = new Pen(gridColor);
                for (double x = SnapToGridPoint(topLeft).X; x < bottomRight.X; x += grid)
                {
                    PointF top = Transform.TransformPoint(new PointF((float)x, topLeft.Y));
                    PointF bottom = Transform.TransformPoint(new PointF((float)x, bottomRight.Y));
                    g.DrawLine(gridPen, top, bottom);
                }
                for (double y = SnapToGridPoint(bottomRight).Y; y < topLeft.Y; y += grid)
                {
                    PointF left = Transform.TransformPoint(new PointF(topLeft.X, (float)y));
                    PointF right = Transform.TransformPoint(new PointF(bottomRight.X, (float)y));
                    g.DrawLine(gridPen, left, right);
                }
            }

            // Show elements
            if (list != null)
            {
                foreach (Element e in list.GetElements())
                {
                    Color elementColor;
                    switch (e.GetType())
                    {
                        case Element.Type.Dielectric:
                            elementColor = dielectricColor;
                            break;
                        case Element.Type.TracePos:
                            elementColor = tracePosColor;
                            break;
                        case Element.Type.TraceNeg:
                            elementColor = traceNegColor;
                            break;
                        case Element.Type.GND:
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
                        PointF devicePoint = Transform.TransformPoint(v);
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
                            PointF start = Transform.TransformPoint(vertices[i]);
                            PointF stop = Transform.TransformPoint(vertices[prev]);
                            g.DrawLine(elementPen, start, stop);
                        }
                    }

                    if (vertices.Length > 0 && e == appendElement)
                    {
                        PointF start = Transform.TransformPoint(vertices[vertices.Length - 1]);
                        PointF stop = lastMouseCoords;
                        if (snapToGrid)
                        {
                            stop = Transform.TransformPoint(SnapToGridPoint(Transform.InverseTransformPoint(stop)));
                        }
                        g.DrawLine(elementPen, start, stop);
                    }
                }
            }
        }

        private void PcbView_MouseMove(object sender, MouseEventArgs e)
        {
            if (appendElement != null)
            {
                lastMouseCoords = e.Location;
                Invalidate();
            }
            else if (dragVertex.e != null)
            {
                PointF vertexPoint = Transform.InverseTransformPoint(e.Location);
                if (snapToGrid)
                {
                    vertexPoint = SnapToGridPoint(vertexPoint);
                }
                dragVertex.e.ChangeVertex(dragVertex.index, vertexPoint);
                Invalidate();
            }
        }

        private PointF SnapToGridPoint(PointF pos)
        {
            float snapX = (float)Math.Round(pos.X / grid) * (float)grid;
            float snapY = (float)Math.Round(pos.Y / grid) * (float)grid;
            return new PointF(snapX, snapY);
        }

        private class VertexInfo
        {
            public Element e;
            public int index;
        }
    }
}
