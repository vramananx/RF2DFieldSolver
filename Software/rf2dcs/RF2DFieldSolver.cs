using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace RF2DFieldSolver
{
    public static class Util
    {
        public static T Scale<T>(T value, T fromLow, T fromHigh, T toLow, T toHigh, bool logFrom = false, bool logTo = false) where T : IConvertible
        {
            double normalized;
            if (logFrom)
            {
                normalized = Math.Log10(Convert.ToDouble(value) / Convert.ToDouble(fromLow)) / Math.Log10(Convert.ToDouble(fromHigh) / Convert.ToDouble(fromLow));
            }
            else
            {
                normalized = (Convert.ToDouble(value) - Convert.ToDouble(fromLow)) / (Convert.ToDouble(fromHigh) - Convert.ToDouble(fromLow));
            }
            if (logTo)
            {
                value = (T)Convert.ChangeType(Convert.ToDouble(toLow) * Math.Pow(10.0, normalized * Math.Log10(Convert.ToDouble(toHigh) / Convert.ToDouble(toLow))), typeof(T));
            }
            else
            {
                value = (T)Convert.ChangeType(normalized * (Convert.ToDouble(toHigh) - Convert.ToDouble(toLow)) + Convert.ToDouble(toLow), typeof(T));
            }
            return value;
        }

        public static double DistanceToLine(PointF point, PointF l1, PointF l2, out PointF closestLinePoint, out double pointRatio)
        {
            var M = new PointF(l2.X - l1.X, l2.Y - l1.Y);
            var t0 = (M.X * (point.X - l1.X) + M.Y * (point.Y - l1.Y)) / (M.X * M.X + M.Y * M.Y);
            PointF closestPoint;
            if (t0 <= 0)
            {
                closestPoint = l1;
                t0 = 0;
            }
            else if (t0 >= 1)
            {
                closestPoint = l2;
                t0 = 1;
            }
            else
            {
                closestPoint = new PointF(l1.X + t0 * M.X, l1.Y + t0 * M.Y);
            }
            closestLinePoint = closestPoint;
            pointRatio = t0;
            return Math.Sqrt(Math.Pow(point.X - closestPoint.X, 2) + Math.Pow(point.Y - closestPoint.Y, 2));
        }

        public static Color GetIntensityGradeColor(double intensity)
        {
            if (intensity < -1.0)
            {
                return Color.Blue;
            }
            else if (intensity > 1.0)
            {
                return Color.White;
            }
            else if (intensity >= -1.0 && intensity <= 1.0)
            {
                int hue = (int)Scale(intensity, -1.0, 1.0, 240, 0);
                int saturation = 255;
                int value = (int)(Math.Abs(intensity) * 255);
                return ColorFromHSV(hue, saturation, value);
            }
            else
            {
                return Color.Black;
            }
        }

        private static Color ColorFromHSV(int hue, int saturation, int value)
        {
            double h = hue / 360.0;
            double s = saturation / 100.0;
            double v = value / 100.0;

            int hi = (int)(h * 6);
            double f = h * 6 - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            v *= 255;
            p *= 255;
            q *= 255;
            t *= 255;

            if (hi == 0)
                return Color.FromArgb(255, (int)v, (int)t, (int)p);
            else if (hi == 1)
                return Color.FromArgb(255, (int)q, (int)v, (int)p);
            else if (hi == 2)
                return Color.FromArgb(255, (int)p, (int)v, (int)t);
            else if (hi == 3)
                return Color.FromArgb(255, (int)p, (int)q, (int)v);
            else if (hi == 4)
                return Color.FromArgb(255, (int)t, (int)p, (int)v);
            else
                return Color.FromArgb(255, (int)v, (int)p, (int)q);
        }
    }

    public abstract class Savable
    {
        public abstract JObject ToJSON();
        public abstract void FromJSON(JObject j);

        public bool OpenFromFileDialog(string title, string filetype)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filetype,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                try
                {
                    string jsonText = File.ReadAllText(filename);
                    JObject j = JObject.Parse(jsonText);
                    FromJSON(j);
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to parse the setup file (" + e.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }

        public bool SaveToFileDialog(string title, string filetype, string ending = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filetype,
                AddExtension = true,
                DefaultExt = ending
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;
                if (!string.IsNullOrEmpty(ending) && !filename.EndsWith(ending))
                {
                    filename += ending;
                }

                try
                {
                    JObject j = ToJSON();
                    File.WriteAllText(filename, j.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to save the file (" + e.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }
    }

    public class Element
    {
        public enum Type
        {
            Dielectric,
            TracePos,
            TraceNeg,
            GND,
            Last
        }

        private List<PointF> vertices;
        private string name;
        private Type type;
        private double epsilon_r;

        public Element(Type type)
        {
            this.type = type;
            epsilon_r = 4.3;
            switch (type)
            {
                case Type.TracePos:
                    name = "RF+";
                    break;
                case Type.TraceNeg:
                    name = "RF-";
                    break;
                case Type.Dielectric:
                    name = "Substrate";
                    break;
                case Type.GND:
                    name = "GND";
                    break;
                case Type.Last:
                    break;
            }
        }

        public JObject ToJSON()
        {
            JObject j = new JObject();
            j["name"] = name;
            j["type"] = TypeToString(type);
            j["e_r"] = epsilon_r;
            JArray jvertices = new JArray();
            foreach (var v in vertices)
            {
                JObject jvertex = new JObject();
                jvertex["x"] = v.X;
                jvertex["y"] = v.Y;
                jvertices.Add(jvertex);
            }
            j["vertices"] = jvertices;
            return j;
        }

        public void FromJSON(JObject j)
        {
            name = j.Value<string>("name") ?? name;
            type = TypeFromString(j.Value<string>("type") ?? "");
            epsilon_r = j.Value<double?>("e_r") ?? epsilon_r;
            vertices.Clear();
            if (j.ContainsKey("vertices"))
            {
                foreach (var jvertex in j["vertices"])
                {
                    PointF p = new PointF();
                    p.X = jvertex.Value<float>("x");
                    p.Y = jvertex.Value<float>("y");
                    vertices.Add(p);
                }
            }
        }

        public static string TypeToString(Type type)
        {
            switch (type)
            {
                case Type.Dielectric:
                    return "Dielectric";
                case Type.GND:
                    return "GND";
                case Type.TracePos:
                    return "Trace+";
                case Type.TraceNeg:
                    return "Trace-";
                case Type.Last:
                    return "";
            }
            return "";
        }

        public static Type TypeFromString(string s)
        {
            foreach (Type t in Enum.GetValues(typeof(Type)))
            {
                if (s == TypeToString(t))
                {
                    return t;
                }
            }
            return Type.Last;
        }

        public static List<Type> GetTypes()
        {
            List<Type> ret = new List<Type>();
            foreach (Type t in Enum.GetValues(typeof(Type)))
            {
                ret.Add(t);
            }
            return ret;
        }

        public void AddVertex(int index, PointF vertex)
        {
            vertices.Insert(index, vertex);
        }

        public void AppendVertex(PointF vertex)
        {
            vertices.Add(vertex);
        }

        public void RemoveVertex(int index)
        {
            if (index >= 0 && index < vertices.Count)
            {
                vertices.RemoveAt(index);
            }
        }

        public void ChangeVertex(int index, PointF newCoords)
        {
            if (index >= 0 && index < vertices.Count)
            {
                vertices[index] = newCoords;
            }
        }

        public void SetType(Type t)
        {
            type = t;
            // Emit typeChanged event
        }

        public List<PointF> ToPolygon()
        {
            return new List<PointF>(vertices);
        }

        public string GetName() { return name; }
        public Type GetType() { return type; }
        public double GetEpsilonR() { return epsilon_r; }
        public List<PointF> GetVertices() { return vertices; }
        public void SetName(string s) { name = s; }
        public void SetEpsilonR(double er) { epsilon_r = er; }
    }

    public class ElementList : DataGridView, ISavable
    {
        private List<Element> elements;

        public ElementList()
        {
            elements = new List<Element>();
        }

        public string ToJSON()
        {
            var jelements = elements.Select(e => e.ToJSON()).ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { elements = jelements });
        }

        public void FromJSON(string json)
        {
            var j = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            elements.Clear();
            foreach (var jelement in j.elements)
            {
                var e = new Element(Element.Type.Dielectric);
                e.FromJSON(jelement.ToString());
                AddElement(e);
            }
        }

        public void AddElement(Element e)
        {
            elements.Add(e);
            e.TypeChanged += (sender, args) =>
            {
                var i = FindIndex(e);
                if (i != -1)
                {
                    this.InvalidateRow(i);
                }
            };
            e.Disposed += (sender, args) => RemoveElement(e, false);
        }

        public bool RemoveElement(Element e, bool del = true)
        {
            int i = FindIndex(e);
            if (i != -1)
            {
                return RemoveElement(i, del);
            }
            else
            {
                return false;
            }
        }

        public bool RemoveElement(int index, bool del = true)
        {
            if (index < 0 || index >= elements.Count)
            {
                return false;
            }
            var e = elements[index];
            elements.RemoveAt(index);
            if (del)
            {
                e.Dispose();
            }
            return true;
        }

        public Element ElementAt(int index)
        {
            if (index >= 0 && index < elements.Count)
            {
                return elements[index];
            }
            else
            {
                return null;
            }
        }

        public double GetDielectricConstantAt(System.Drawing.PointF p)
        {
            foreach (var e in elements)
            {
                var poly = new System.Drawing.Drawing2D.GraphicsPath();
                poly.AddPolygon(e.GetVertices().ToArray());
                if (poly.IsVisible(p))
                {
                    switch (e.GetType())
                    {
                        case Element.Type.GND:
                        case Element.Type.TracePos:
                        case Element.Type.TraceNeg:
                            return 1.0;
                        case Element.Type.Dielectric:
                            return e.GetEpsilonR();
                        default:
                            return 1.0;
                    }
                }
            }
            return 1.0;
        }

        private int FindIndex(Element e)
        {
            return elements.IndexOf(e);
        }
    }

    public class TypeDelegate : DataGridViewComboBoxColumn
    {
        public TypeDelegate()
        {
            foreach (var t in Element.GetTypes())
            {
                this.Items.Add(Element.TypeToString(t));
            }
        }

        public void SetEditorData(DataGridViewComboBoxCell editor, Element e)
        {
            editor.Value = Element.TypeToString(e.GetType());
        }

        public void SetModelData(DataGridViewComboBoxCell editor, ElementList list, int rowIndex)
        {
            var e = list.ElementAt(rowIndex);
            e.SetType(Element.TypeFromString(editor.Value.ToString()));
        }
    }

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

            int steps = Math.Abs(e.Delta / 120);
            int sign = e.Delta > 0 ? 1 : -1;
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
            if (!this.Text.IsEmpty())
            {
                ContinueEditing();
            }
        }

        private void ParseNewValue(double factor)
        {
            string input = this.Text;
            if (input.IsEmpty())
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

                if (double.TryParse(input, out double v))
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

        protected virtual void OnValueUpdated(object sender)
        {
            ValueUpdated?.Invoke(sender, EventArgs.Empty);
        }

        protected virtual void OnEditingAborted()
        {
            EditingAborted?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Laplace : Form
    {
        private bool calculationRunning;
        private bool resultReady;
        private ElementList list;
        private PointF topLeft;
        private PointF bottomRight;
        private double grid;
        private int threads;
        private double threshold;
        private bool groundedBorders;
        private bool ignoreDielectric;
        private Lattice lattice;
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
            Console.WriteLine("Laplace calculation starting");
            if (lattice != null)
            {
                lattice.Dispose();
                lattice = null;
            }
            this.list = list;

            thread = new Thread(CalcThread);
            thread.Start();
            Console.WriteLine("Laplace thread started");
            return true;
        }

        public void AbortCalculation()
        {
            if (!calculationRunning)
            {
                return;
            }
            lattice.Abort = true;
        }

        public double GetPotential(PointF p)
        {
            if (!resultReady)
            {
                return double.NaN;
            }
            var pos = CoordToRect(p);
            int index_x = (int)Math.Round(pos.X) + 1;
            int index_y = (int)Math.Round(pos.Y) + 1;
            if (index_x < 0 || index_x >= lattice.Dim.X || index_y < 0 || index_y >= lattice.Dim.Y)
            {
                return double.NaN;
            }
            var c = lattice.Cells[index_x + index_y * lattice.Dim.X];
            return c.Value;
        }

        public PointF GetGradient(PointF p)
        {
            PointF ret = new PointF(p.X, p.Y);
            if (!resultReady)
            {
                return ret;
            }
            var pos = CoordToRect(p);
            int index_x = (int)Math.Floor(pos.X) + 1;
            int index_y = (int)Math.Floor(pos.Y) + 1;

            if (index_x < 0 || index_x + 1 >= lattice.Dim.X || index_y < 0 || index_y + 1 >= lattice.Dim.Y)
            {
                return ret;
            }

            var c_floor = lattice.Cells[index_x + index_y * lattice.Dim.X];
            var c_x = lattice.Cells[index_x + 1 + index_y * lattice.Dim.X];
            var c_y = lattice.Cells[index_x + (index_y + 1) * lattice.Dim.X];
            var grad_x = c_x.Value - c_floor.Value;
            var grad_y = c_y.Value - c_floor.Value;
            ret = new PointF(p.X + (float)grad_x, p.Y + (float)grad_y);
            return ret;
        }

        public void InvalidateResult()
        {
            resultReady = false;
        }

        private PointF CoordFromRect(Rect pos)
        {
            PointF ret = new PointF();
            ret.X = (float)(pos.X * grid + topLeft.X);
            ret.Y = (float)(pos.Y * grid + bottomRight.Y);
            return ret;
        }

        private Rect CoordToRect(PointF pos)
        {
            Rect ret = new Rect();
            ret.X = (pos.X - topLeft.X) / grid;
            ret.Y = (pos.Y - bottomRight.Y) / grid;
            return ret;
        }

        private Bound Boundary(Bound bound, Rect pos)
        {
            var coord = CoordFromRect(pos);
            bound.Value = 0;
            bound.Cond = Condition.None;

            bool isBorder = Math.Abs(coord.X - topLeft.X) < 1e-6 || Math.Abs(coord.X - bottomRight.X) < 1e-6 || Math.Abs(coord.Y - topLeft.Y) < 1e-6 || Math.Abs(coord.Y - bottomRight.Y) < 1e-6;

            if (groundedBorders && isBorder)
            {
                bound.Value = 0;
                bound.Cond = Condition.Dirichlet;
                return bound;
            }
            else
            {
                foreach (var e in list.GetElements())
                {
                    if (e.GetType() == Element.Type.Dielectric)
                    {
                        continue;
                    }
                    var poly = e.ToPolygon();
                    if (poly.Contains(coord))
                    {
                        switch (e.GetType())
                        {
                            case Element.Type.GND:
                                bound.Value = 0;
                                bound.Cond = Condition.Dirichlet;
                                return bound;
                            case Element.Type.TracePos:
                                bound.Value = 1.0;
                                bound.Cond = Condition.Dirichlet;
                                return bound;
                            case Element.Type.TraceNeg:
                                bound.Value = -1.0;
                                bound.Cond = Condition.Dirichlet;
                                return bound;
                            default:
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
            Console.WriteLine("Creating lattice");
            Rect size = new Rect { X = (bottomRight.X - topLeft.X) / grid, Y = (topLeft.Y - bottomRight.Y) / grid };
            Point dim = new Point { X = (uint)((bottomRight.X - topLeft.X) / grid), Y = (uint)((topLeft.Y - bottomRight.Y) / grid) };
            lattice = new Lattice(size, dim, Boundary, Weight);
            if (lattice != null)
            {
                Console.WriteLine("Lattice creation complete");
            }
            else
            {
                Console.WriteLine("Lattice creation failed");
                return;
            }

            Config conf = new Config { Threads = (byte)threads, Distance = 10, Threshold = threshold };
            if (conf.Threads > lattice.Dim.Y / 5)
            {
                conf.Threads = (byte)(lattice.Dim.Y / 5);
            }
            conf.Distance = (byte)(lattice.Dim.Y / threads);
            Console.WriteLine("Starting calculation threads");
            var it = lattice.ComputeThreaded(conf, CalcProgressFromDiff);
            calculationRunning = false;
            if (lattice.Abort)
            {
                Console.WriteLine("Laplace calculation aborted");
                resultReady = false;
                Console.WriteLine("0%");
                Console.WriteLine("Calculation aborted");
            }
            else
            {
                Console.WriteLine("Laplace calculation complete, took " + it + " iterations");
                resultReady = true;
                Console.WriteLine("100%");
                Console.WriteLine("Calculation done");
            }
        }

        private void CalcProgressFromDiff(double diff)
        {
            double endTime = Math.Pow(-Math.Log(threshold), 6);
            double currentTime = Math.Pow(-Math.Log(diff), 6);
            double percent = currentTime * 100 / endTime;
            if (percent > 100)
            {
                percent = 100;
            }
            else if (percent < lastPercent)
            {
                percent = lastPercent;
            }
            lastPercent = percent;
            Console.WriteLine(percent + "%");
        }
    }

    public class Gauss : Form
    {
        public Gauss()
        {
        }

        public static double GetCharge(Laplace laplace, ElementList list, Element e, double gridSize, double distance)
        {
            var integral = Polygon.Offset(e.GetVertices(), distance);

            double chargeSum = 0;
            for (int i = 0; i < integral.Count; i++)
            {
                var pp = integral[(i + integral.Count - 1) % integral.Count];
                var pc = integral[i];

                var increment = new Line(pp, pc);
                var unitVector = increment;
                unitVector.Length = 1.0;
                int points = (int)Math.Ceiling(increment.Length / gridSize);
                double stepSize = increment.Length / points;
                increment.Length = stepSize;
                var point = pp + new PointF((float)(increment.Dx / 2), (float)(increment.Dy / 2));
                for (int j = 0; j < points; j++)
                {
                    var gradient = laplace.GetGradient(point);
                    if (list != null)
                    {
                        gradient.Length *= list.GetDielectricConstantAt(point);
                    }
                    double perp = gradient.Dx * unitVector.Dy - gradient.Dy * unitVector.Dx;
                    perp *= stepSize / gridSize;
                    chargeSum += perp;
                    point += new PointF((float)increment.Dx, (float)increment.Dy);
                }
            }

            if (!Polygon.IsClockwise(integral))
            {
                chargeSum *= -1;
            }

            return chargeSum;
        }
    }
}
