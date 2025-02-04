using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class MainWindow : Form
{
    private ElementList list;
    private Laplace laplace;
    private Gauss gauss;
    private TextBox status;
    private Button update;
    private Button abort;
    private Button add;
    private Button remove;
    private DataGridView table;
    private PCBView view;
    private NumericUpDown resolution;
    private NumericUpDown gaussDistance;
    private NumericUpDown tolerance;
    private NumericUpDown threads;
    private CheckBox borderIsGND;
    private NumericUpDown xleft;
    private NumericUpDown xright;
    private NumericUpDown ytop;
    private NumericUpDown ybottom;
    private NumericUpDown gridsize;
    private CheckBox showPotential;
    private CheckBox showGrid;
    private CheckBox snapGrid;
    private ComboBox viewMode;
    private NumericUpDown capacitanceP;
    private NumericUpDown inductanceP;
    private NumericUpDown impedanceP;
    private NumericUpDown capacitanceN;
    private NumericUpDown inductanceN;
    private NumericUpDown impedanceN;
    private NumericUpDown impedanceDiff;
    private ProgressBar progress;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Initialize components and set properties
        // This is a simplified version, you need to add all the components and set their properties accordingly
        this.status = new TextBox();
        this.update = new Button();
        this.abort = new Button();
        this.add = new Button();
        this.remove = new Button();
        this.table = new DataGridView();
        this.view = new PCBView();
        this.resolution = new NumericUpDown();
        this.gaussDistance = new NumericUpDown();
        this.tolerance = new NumericUpDown();
        this.threads = new NumericUpDown();
        this.borderIsGND = new CheckBox();
        this.xleft = new NumericUpDown();
        this.xright = new NumericUpDown();
        this.ytop = new NumericUpDown();
        this.ybottom = new NumericUpDown();
        this.gridsize = new NumericUpDown();
        this.showPotential = new CheckBox();
        this.showGrid = new CheckBox();
        this.snapGrid = new CheckBox();
        this.viewMode = new ComboBox();
        this.capacitanceP = new NumericUpDown();
        this.inductanceP = new NumericUpDown();
        this.impedanceP = new NumericUpDown();
        this.capacitanceN = new NumericUpDown();
        this.inductanceN = new NumericUpDown();
        this.impedanceN = new NumericUpDown();
        this.impedanceDiff = new NumericUpDown();
        this.progress = new ProgressBar();

        // Add components to the form
        this.Controls.Add(this.status);
        this.Controls.Add(this.update);
        this.Controls.Add(this.abort);
        this.Controls.Add(this.add);
        this.Controls.Add(this.remove);
        this.Controls.Add(this.table);
        this.Controls.Add(this.view);
        this.Controls.Add(this.resolution);
        this.Controls.Add(this.gaussDistance);
        this.Controls.Add(this.tolerance);
        this.Controls.Add(this.threads);
        this.Controls.Add(this.borderIsGND);
        this.Controls.Add(this.xleft);
        this.Controls.Add(this.xright);
        this.Controls.Add(this.ytop);
        this.Controls.Add(this.ybottom);
        this.Controls.Add(this.gridsize);
        this.Controls.Add(this.showPotential);
        this.Controls.Add(this.showGrid);
        this.Controls.Add(this.snapGrid);
        this.Controls.Add(this.viewMode);
        this.Controls.Add(this.capacitanceP);
        this.Controls.Add(this.inductanceP);
        this.Controls.Add(this.impedanceP);
        this.Controls.Add(this.capacitanceN);
        this.Controls.Add(this.inductanceN);
        this.Controls.Add(this.impedanceN);
        this.Controls.Add(this.impedanceDiff);
        this.Controls.Add(this.progress);

        // Set form properties
        this.Text = "RF 2D Field Solver";
        this.WindowState = FormWindowState.Maximized;

        // Add event handlers
        this.update.Click += new EventHandler(this.StartCalculation);
        this.abort.Click += new EventHandler(this.AbortCalculation);
        this.add.Click += new EventHandler(this.AddElement);
        this.remove.Click += new EventHandler(this.RemoveElement);
    }

    private void StartCalculation(object sender, EventArgs e)
    {
        // Start the calculations
        this.progress.Value = 0;
        this.update.Enabled = false;
        this.abort.Enabled = true;
        this.view.Enabled = false;
        this.table.Enabled = false;
        this.gridsize.Enabled = false;
        this.xleft.Enabled = false;
        this.xright.Enabled = false;
        this.ytop.Enabled = false;
        this.ybottom.Enabled = false;
        this.resolution.Enabled = false;
        this.gaussDistance.Enabled = false;
        this.threads.Enabled = false;
        this.tolerance.Enabled = false;
        this.borderIsGND.Enabled = false;
        this.add.Enabled = false;
        this.remove.Enabled = false;

        // Clear status and reset values
        this.status.Clear();
        this.capacitanceP.Value = 0;
        this.inductanceP.Value = 0;
        this.impedanceP.Value = 0;
        this.capacitanceN.Value = 0;
        this.inductanceN.Value = 0;
        this.impedanceN.Value = 0;
        this.impedanceDiff.Value = 0;

        // Invalidate result and update view
        this.laplace.InvalidateResult();
        this.view.Update();

        // Start the dielectric laplace calculation
        this.laplace.SetArea(new PointF((float)this.xleft.Value, (float)this.ytop.Value), new PointF((float)this.xright.Value, (float)this.ybottom.Value));
        this.laplace.SetGrid((double)this.resolution.Value);
        this.laplace.SetThreads((int)this.threads.Value);
        this.laplace.SetThreshold((double)this.tolerance.Value);
        this.laplace.SetGroundedBorders(this.borderIsGND.Checked);
        this.laplace.StartCalculation(this.list);
        this.view.Update();
    }

    private void AbortCalculation(object sender, EventArgs e)
    {
        // Abort the calculation
        this.laplace.AbortCalculation();
    }

    private void AddElement(object sender, EventArgs e)
    {
        // Add a new element
        var element = new Element(ElementType.TracePos);
        this.list.AddElement(element);
        this.view.StartAppending(element);
    }

    private void RemoveElement(object sender, EventArgs e)
    {
        // Remove the selected element
        var row = this.table.CurrentCell.RowIndex;
        if (row >= 0 && row < this.list.GetElements().Count)
        {
            this.list.RemoveElement(row);
            this.view.Update();
        }
    }

    private void Info(string info)
    {
        this.status.AppendText(info + Environment.NewLine);
    }

    private void Warning(string warning)
    {
        this.status.AppendText("Warning: " + warning + Environment.NewLine);
    }

    private void Error(string error)
    {
        this.status.AppendText("Error: " + error + Environment.NewLine);
    }

    public JObject ToJSON()
    {
        var j = new JObject();
        j["xleft"] = this.xleft.Value;
        j["xright"] = this.xright.Value;
        j["ytop"] = this.ytop.Value;
        j["ybottom"] = this.ybottom.Value;
        j["viewGrid"] = this.gridsize.Value;
        j["showPotential"] = this.showPotential.Checked;
        j["showGrid"] = this.showGrid.Checked;
        j["snapToGrid"] = this.snapGrid.Checked;
        j["viewMode"] = this.viewMode.Text;
        j["simulationGrid"] = this.resolution.Value;
        j["gaussDistance"] = this.gaussDistance.Value;
        j["tolerance"] = this.tolerance.Value;
        j["threads"] = this.threads.Value;
        j["borderIsGND"] = this.borderIsGND.Checked;
        j["list"] = this.list.ToJSON();
        return j;
    }

    public void FromJSON(JObject j)
    {
        this.xleft.Value = (decimal)j.Value<double>("xleft");
        this.xright.Value = (decimal)j.Value<double>("xright");
        this.ytop.Value = (decimal)j.Value<double>("ytop");
        this.ybottom.Value = (decimal)j.Value<double>("ybottom");
        this.gridsize.Value = (decimal)j.Value<double>("viewGrid");
        this.showPotential.Checked = j.Value<bool>("showPotential");
        this.showGrid.Checked = j.Value<bool>("showGrid");
        this.snapGrid.Checked = j.Value<bool>("snapToGrid");
        this.viewMode.Text = j.Value<string>("viewMode");
        this.resolution.Value = (decimal)j.Value<double>("simulationGrid");
        this.gaussDistance.Value = (decimal)j.Value<double>("gaussDistance");
        this.tolerance.Value = (decimal)j.Value<double>("tolerance");
        this.threads.Value = j.Value<int>("threads");
        this.borderIsGND.Checked = j.Value<bool>("borderIsGND");
        this.list.FromJSON(j.Value<JObject>("list"));
    }
}

public class InformationBox : Form
{
    private static bool has_gui = true;
    private CheckBox cb;
    private uint hash;

    public static void ShowMessage(string title, string message, string messageID = "", bool block = false, Form parent = null)
    {
        if (!has_gui)
        {
            return;
        }

        uint hash = string.IsNullOrEmpty(messageID) ? (uint)message.GetHashCode() : (uint)messageID.GetHashCode();

        if (!ConfigurationManager.AppSettings.AllKeys.Contains(hashToSettingsKey(hash)))
        {
            InformationBox box = new InformationBox(title, message, MessageBoxIcon.Information, hash, parent);
            if (block)
            {
                box.ShowDialog();
            }
            else
            {
                box.Show();
            }
        }
    }

    public static void ShowMessageBlocking(string title, string message, string messageID = "", Form parent = null)
    {
        ShowMessage(title, message, messageID, true, parent);
    }

    public static void ShowError(string title, string message, Form parent = null)
    {
        if (!has_gui)
        {
            return;
        }
        InformationBox box = new InformationBox(title, message, MessageBoxIcon.Error, 0, parent);
        box.Show();
    }

    public static bool AskQuestion(string title, string question, bool defaultAnswer, string messageID = "", Form parent = null)
    {
        if (!has_gui)
        {
            return defaultAnswer;
        }

        uint hash = string.IsNullOrEmpty(messageID) ? (uint)question.GetHashCode() : (uint)messageID.GetHashCode();

        if (!ConfigurationManager.AppSettings.AllKeys.Contains(hashToSettingsKey(hash)))
        {
            InformationBox box = new InformationBox(title, question, MessageBoxIcon.Question, hash, parent);
            box.Controls.Add(new Button { Text = "Yes", DialogResult = DialogResult.Yes });
            box.Controls.Add(new Button { Text = "No", DialogResult = DialogResult.No });
            DialogResult ret = box.ShowDialog();
            return ret == DialogResult.Yes;
        }
        else
        {
            return bool.Parse(ConfigurationManager.AppSettings[hashToSettingsKey(hash)]);
        }
    }

    public static void setGUI(bool enable)
    {
        has_gui = enable;
    }

    private InformationBox(string title, string message, MessageBoxIcon icon, uint hash, Form parent)
    {
        this.hash = hash;
        this.Text = title;
        this.Controls.Add(new Label { Text = message, AutoSize = true });
        this.cb = new CheckBox { Text = "Do not show this message again" };
        this.Controls.Add(cb);
        this.FormClosed += InformationBox_FormClosed;
    }

    private void InformationBox_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (cb.Checked)
        {
            bool value = this.DialogResult == DialogResult.No ? false : true;
            ConfigurationManager.AppSettings[hashToSettingsKey(hash)] = value.ToString();
        }
    }

    private static string hashToSettingsKey(uint hash)
    {
        return "DoNotShowDialog/" + hash.ToString();
    }
}

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

public class SIUnitEdit : TextBox
{
    private string unit;
    private string prefixes;
    private int precision;
    private double _value;

    public SIUnitEdit(string unit = "", string prefixes = " ", int precision = 0)
    {
        this.unit = unit;
        this.prefixes = prefixes;
        this.precision = precision;
        this.TextAlign = HorizontalAlignment.Center;
        this.LostFocus += (sender, e) => ParseNewValue(1.0);
        SetValueQuiet(0);
    }

    public void SetUnit(string unit)
    {
        this.unit = unit;
        SetValueQuiet(_value);
    }

    public void SetPrefixes(string prefixes)
    {
        this.prefixes = prefixes;
        SetValueQuiet(_value);
    }

    public void SetPrecision(int precision)
    {
        this.precision = precision;
        SetValueQuiet(_value);
    }

    public double Value
    {
        get { return _value; }
    }

    public void SetValue(double value)
    {
        if (value != _value)
        {
            SetValueQuiet(value);
            OnValueChanged(value);
            OnValueUpdated(this);
        }
    }

    public event EventHandler<double> ValueChanged;
    public event EventHandler ValueUpdated;
    public event EventHandler EditingAborted;
    public event EventHandler FocusLost;

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (e.KeyChar == (char)Keys.Escape)
        {
            Clear();
            SetValueQuiet(_value);
            OnEditingAborted();
            this.Parent.Focus();
            e.Handled = true;
        }
        else if (e.KeyChar == (char)Keys.Return)
        {
            ParseNewValue(1.0);
            ContinueEditing();
            e.Handled = true;
        }
        else if (prefixes.Contains(e.KeyChar.ToString()))
        {
            ParseNewValue(Unit.SIPrefixToFactor(e.KeyChar));
            ContinueEditing();
            e.Handled = true;
        }
        else if (prefixes.Contains(SwapUpperLower(e.KeyChar).ToString()))
        {
            ParseNewValue(Unit.SIPrefixToFactor(SwapUpperLower(e.KeyChar)));
            ContinueEditing();
            e.Handled = true;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_value == 0.0)
        {
            return;
        }

        int increment = e.Delta / 120;
        int steps = Math.Abs(increment);
        int sign = increment > 0 ? 1 : -1;
        double newVal = _value;

        if (this.Focused)
        {
            int cursor = this.SelectionStart;
            if (cursor == 0)
            {
                return;
            }

            int nthDigit = cursor;
            if (_value < 0)
            {
                nthDigit--;
            }

            int dotPos = this.Text.IndexOf('.');
            if (dotPos >= 0 && dotPos < nthDigit)
            {
                nthDigit--;
            }

            if (this.Text.StartsWith("-0.") || this.Text.StartsWith("0."))
            {
                nthDigit--;
            }

            double step_size = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
            newVal += step_size * steps * sign;
            SetValue(newVal);
            this.Text = this.PlaceholderText;
            this.SelectionStart = cursor;
        }
        else
        {
            const int nthDigit = 3;
            while (steps > 0)
            {
                double step_size = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(newVal))) - nthDigit + 1);
                newVal += step_size * sign;
                steps--;
            }
            SetValue(newVal);
        }
    }

    private void SetValueQuiet(double value)
    {
        _value = value;
        this.PlaceholderText = Unit.ToString(value, unit, prefixes, precision);
        if (!string.IsNullOrEmpty(this.Text))
        {
            ContinueEditing();
        }
    }

    private void ParseNewValue(double factor)
    {
        string input = this.Text;
        if (string.IsNullOrEmpty(input))
        {
            SetValueQuiet(_value);
            OnEditingAborted();
        }
        else
        {
            if (input.EndsWith(unit))
            {
                input = input.Substring(0, input.Length - unit.Length);
            }

            char lastChar = input[input.Length - 1];
            if (prefixes.Contains(lastChar.ToString()))
            {
                factor = Unit.SIPrefixToFactor(lastChar);
                input = input.Substring(0, input.Length - 1);
            }

            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
                Clear();
                SetValue(v * factor);
            }
            else
            {
                Console.WriteLine("SIUnit conversion failure: " + input);
            }
        }
    }

    private void ContinueEditing()
    {
        this.Text = this.PlaceholderText;
        this.SelectAll();
    }

    private static char SwapUpperLower(char c)
    {
        if (char.IsUpper(c))
        {
            return char.ToLower(c);
        }
        else if (char.IsLower(c))
        {
            return char.ToUpper(c);
        }
        else
        {
            return c;
        }
    }

    protected virtual void OnValueChanged(double newValue)
    {
        ValueChanged?.Invoke(this, newValue);
    }

    protected virtual void OnValueUpdated(EventArgs e)
    {
        ValueUpdated?.Invoke(this, e);
    }

    protected virtual void OnEditingAborted()
    {
        EditingAborted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnFocusLost()
    {
        FocusLost?.Invoke(this, EventArgs.Empty);
    }
}

public static class Unit
{
    public static double FromString(string str, string unit, string prefixes)
    {
        if (string.IsNullOrEmpty(str))
        {
            return double.NaN;
        }

        if (str.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
        {
            str = str.Substring(0, str.Length - unit.Length);
        }

        double factor = 1.0;
        if (prefixes.Contains(str[str.Length - 1].ToString()))
        {
            char prefix = str[str.Length - 1];
            factor = SIPrefixToFactor(prefix);
            str = str.Substring(0, str.Length - 1);
        }

        if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value * factor;
        }
        else
        {
            return double.NaN;
        }
    }

    public static string ToString(double value, string unit, string prefixes, int precision)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return "NaN";
        }
        else if (Math.Abs(value) <= double.Epsilon)
        {
            return "0 " + unit;
        }
        else
        {
            string sValue = value < 0 ? "-" : "";
            value = Math.Abs(value);

            int prefixIndex = 0;
            int preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            while (preDotDigits > 3 && prefixIndex < prefixes.Length - 1)
            {
                prefixIndex++;
                preDotDigits = (int)Math.Log10(value / SIPrefixToFactor(prefixes[prefixIndex])) + 1;
            }

            value /= SIPrefixToFactor(prefixes[prefixIndex]);
            sValue += value.ToString("F" + Math.Max(0, precision - preDotDigits), CultureInfo.InvariantCulture);
            sValue += prefixes[prefixIndex] + unit;

            return sValue;
        }
    }

    public static double SIPrefixToFactor(char prefix)
    {
        switch (prefix)
        {
            case 'f': return 1e-15;
            case 'p': return 1e-12;
            case 'n': return 1e-9;
            case 'u': return 1e-6;
            case 'm': return 1e-3;
            case ' ': return 1e0;
            case 'k': return 1e3;
            case 'M': return 1e6;
            case 'G': return 1e9;
            case 'T': return 1e12;
            case 'P': return 1e15;
            default: return 1e0;
        }
    }
}

public class Laplace
{
    private bool calculationRunning;
    private bool resultReady;
    private ElementList list;
    private double grid;
    private int threads;
    private double threshold;
    private Lattice lattice;
    private bool groundedBorders;
    private bool ignoreDielectric;
    private PointF topLeft;
    private PointF bottomRight;
    private int lastPercent;
    private Thread thread;

    public Laplace()
    {
        calculationRunning = false;
        resultReady = false;
        list = null;
        grid = 1e-5;
        threads = 1;
        threshold = 1e-6;
        lattice = null;
        groundedBorders = true;
        ignoreDielectric = false;
    }

    public void SetArea(PointF topLeft, PointF bottomRight)
    {
        if (calculationRunning)
        {
            return;
        }
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
    }

    public void SetGrid(double grid)
    {
        if (calculationRunning)
        {
            return;
        }
        if (grid > 0)
        {
            this.grid = grid;
        }
    }

    public void SetThreads(int threads)
    {
        if (calculationRunning)
        {
            return;
        }
        if (threads > 0)
        {
            this.threads = threads;
        }
    }

    public void SetThreshold(double threshold)
    {
        if (calculationRunning)
        {
            return;
        }
        if (threshold > 0)
        {
            this.threshold = threshold;
        }
    }

    public void SetGroundedBorders(bool gnd)
    {
        if (calculationRunning)
        {
            return;
        }
        groundedBorders = gnd;
    }

    public void SetIgnoreDielectric(bool ignore)
    {
        if (calculationRunning)
        {
            return;
        }
        ignoreDielectric = ignore;
    }

    public bool StartCalculation(ElementList list)
    {
        if (calculationRunning)
        {
            return false;
        }
        calculationRunning = true;
        resultReady = false;
        lastPercent = 0;
        Info("Laplace calculation starting");
        if (lattice != null)
        {
            lattice = null;
        }
        this.list = list;

        // start the calculation thread
        thread = new Thread(CalcThread);
        thread.Start();
        Info("Laplace thread started");
        return true;
    }

    public void AbortCalculation()
    {
        if (!calculationRunning)
        {
            return;
        }
        // request abort of calculation
        lattice.Abort = true;
    }

    public double GetPotential(PointF p)
    {
        if (!resultReady)
        {
            return double.NaN;
        }
        var pos = CoordToRect(p);
        // convert to integers and shift by the added outside boundary of NaNs
        int index_x = (int)Math.Round(pos.X) + 1;
        int index_y = (int)Math.Round(pos.Y) + 1;
        if (index_x < 0 || index_x >= lattice.Dim.X || index_y < 0 || index_y >= lattice.Dim.Y)
        {
            return double.NaN;
        }
        var c = lattice.Cells[index_x + index_y * lattice.Dim.X];
        return c.Value;
    }

    public LineF GetGradient(PointF p)
    {
        var ret = new LineF(p, p);
        if (!resultReady)
        {
            return ret;
        }
        var pos = CoordToRect(p);
        // convert to integers and shift by the added outside boundary of NaNs
        int index_x = (int)Math.Floor(pos.X) + 1;
        int index_y = (int)Math.Floor(pos.Y) + 1;

        if (index_x < 0 || index_x + 1 >= lattice.Dim.X || index_y < 0 || index_y + 1 >= lattice.Dim.Y)
        {
            return ret;
        }
        // calculate gradient
        var c_floor = lattice.Cells[index_x + index_y * lattice.Dim.X];
        var c_x = lattice.Cells[index_x + 1 + index_y * lattice.Dim.X];
        var c_y = lattice.Cells[index_x + (index_y + 1) * lattice.Dim.X];
        var grad_x = c_x.Value - c_floor.Value;
        var grad_y = c_y.Value - c_floor.Value;
        ret.P2 = new PointF(p.X + (float)grad_x, p.Y + (float)grad_y);
        return ret;
    }

    public void InvalidateResult()
    {
        resultReady = false;
    }

    private PointF CoordFromRect(Rect pos)
    {
        return new PointF((float)(pos.X * grid + topLeft.X), (float)(pos.Y * grid + bottomRight.Y));
    }

    private Rect CoordToRect(PointF pos)
    {
        return new Rect((pos.X - topLeft.X) / grid, (pos.Y - bottomRight.Y) / grid);
    }

    private Bound Boundary(Bound bound, Rect pos)
    {
        var coord = CoordFromRect(pos);
        bound.Value = 0;
        bound.Cond = Condition.None;

        bool isBorder = Math.Abs(coord.X - topLeft.X) < 1e-6 || Math.Abs(coord.X - bottomRight.X) < 1e-6 || Math.Abs(coord.Y - topLeft.Y) < 1e-6 || Math.Abs(coord.Y - bottomRight.Y) < 1e-6;

        // handle borders
        if (groundedBorders && isBorder)
        {
            bound.Value = 0;
            bound.Cond = Condition.Dirichlet;
            return bound;
        }
        else
        {
            // find the matching polygon
            foreach (var e in list.Elements)
            {
                if (e.Type == ElementType.Dielectric)
                {
                    // skip, dielectric has no influence on boundary and trace/GND should always take priority
                    continue;
                }
                var poly = e.ToPolygon();
                if (poly.Contains(coord))
                {
                    // this polygon defines the boundary at these coordinates
                    switch (e.Type)
                    {
                        case ElementType.GND:
                            bound.Value = 0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.TracePos:
                            bound.Value = 1.0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.TraceNeg:
                            bound.Value = -1.0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.Dielectric:
                        case ElementType.Last:
                            return bound;
                    }
                }
            }
        }
        return bound;
    }

    private double Weight(Rect pos)
    {
        if (ignoreDielectric)
        {
            return 1.0;
        }

        var coord = CoordFromRect(pos);
        return Math.Sqrt(list.GetDielectricConstantAt(coord));
    }

    private void CalcThread()
    {
        Info("Creating lattice");
        var size = new Rect((bottomRight.X - topLeft.X) / grid, (topLeft.Y - bottomRight.Y) / grid);
        var dim = new Point((int)((bottomRight.X - topLeft.X) / grid), (int)((topLeft.Y - bottomRight.Y) / grid));
        lattice = new Lattice(size, dim, Boundary, Weight, this);
        if (
