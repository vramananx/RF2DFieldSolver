using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

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
