using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace rf2dcs.Scenarios
{
    public class Scenario : Form
    {
        private List<Parameter> parameters;
        private string name;
        private TableLayoutPanel parametersLayout;
        private PictureBox image;
        private CheckBox autoArea;
        private NumericUpDown xleft;
        private NumericUpDown xright;
        private NumericUpDown ytop;
        private NumericUpDown ybottom;
        private Button acceptButton;
        private Button cancelButton;

        public Scenario()
        {
            parameters = new List<Parameter>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            parametersLayout = new TableLayoutPanel();
            image = new PictureBox();
            autoArea = new CheckBox();
            xleft = new NumericUpDown();
            xright = new NumericUpDown();
            ytop = new NumericUpDown();
            ybottom = new NumericUpDown();
            acceptButton = new Button();
            cancelButton = new Button();

            SuspendLayout();

            parametersLayout.ColumnCount = 2;
            parametersLayout.RowCount = 0;
            parametersLayout.AutoSize = true;
            parametersLayout.Location = new Point(12, 12);
            parametersLayout.Name = "parametersLayout";
            parametersLayout.TabIndex = 0;

            image.Location = new Point(12, 150);
            image.Name = "image";
            image.Size = new Size(200, 200);
            image.TabIndex = 1;
            image.TabStop = false;

            autoArea.Location = new Point(12, 370);
            autoArea.Name = "autoArea";
            autoArea.Size = new Size(200, 24);
            autoArea.TabIndex = 2;
            autoArea.Text = "Auto Area";
            autoArea.UseVisualStyleBackColor = true;

            xleft.Location = new Point(12, 400);
            xleft.Name = "xleft";
            xleft.Size = new Size(120, 20);
            xleft.TabIndex = 3;
            xleft.DecimalPlaces = 4;
            xleft.Increment = 0.0001M;
            xleft.Minimum = -10;
            xleft.Maximum = 10;

            xright.Location = new Point(12, 430);
            xright.Name = "xright";
            xright.Size = new Size(120, 20);
            xright.TabIndex = 4;
            xright.DecimalPlaces = 4;
            xright.Increment = 0.0001M;
            xright.Minimum = -10;
            xright.Maximum = 10;

            ytop.Location = new Point(12, 460);
            ytop.Name = "ytop";
            ytop.Size = new Size(120, 20);
            ytop.TabIndex = 5;
            ytop.DecimalPlaces = 4;
            ytop.Increment = 0.0001M;
            ytop.Minimum = -10;
            ytop.Maximum = 10;

            ybottom.Location = new Point(12, 490);
            ybottom.Name = "ybottom";
            ybottom.Size = new Size(120, 20);
            ybottom.TabIndex = 6;
            ybottom.DecimalPlaces = 4;
            ybottom.Increment = 0.0001M;
            ybottom.Minimum = -10;
            ybottom.Maximum = 10;

            acceptButton.Location = new Point(12, 520);
            acceptButton.Name = "acceptButton";
            acceptButton.Size = new Size(75, 23);
            acceptButton.TabIndex = 7;
            acceptButton.Text = "Accept";
            acceptButton.UseVisualStyleBackColor = true;
            acceptButton.Click += new EventHandler(AcceptButton_Click);

            cancelButton.Location = new Point(100, 520);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 8;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += new EventHandler(CancelButton_Click);

            ClientSize = new Size(284, 561);
            Controls.Add(parametersLayout);
            Controls.Add(image);
            Controls.Add(autoArea);
            Controls.Add(xleft);
            Controls.Add(xright);
            Controls.Add(ytop);
            Controls.Add(ybottom);
            Controls.Add(acceptButton);
            Controls.Add(cancelButton);
            Name = "Scenario";
            Text = "Scenario";
            ResumeLayout(false);
            PerformLayout();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            foreach (var parameter in parameters)
            {
                var control = parametersLayout.Controls.Find(parameter.Name, true)[0] as NumericUpDown;
                if (control != null)
                {
                    parameter.Value = (double)control.Value;
                }
            }

            var list = CreateScenario();
            OnScenarioCreated(new ScenarioEventArgs(new PointF((float)xleft.Value, (float)ytop.Value), new PointF((float)xright.Value, (float)ybottom.Value), list));
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static List<Scenario> CreateAll()
        {
            var scenarios = new List<Scenario>
            {
                new Microstrip(),
                new CoplanarMicrostrip(),
                new DifferentialMicrostrip(),
                new CoplanarDifferentialMicrostrip(),
                new Stripline(),
                new CoplanarStripline(),
                new DifferentialStripline(),
                new CoplanarDifferentialStripline()
            };

            foreach (var scenario in scenarios)
            {
                scenario.SetupParameters();
            }

            return scenarios;
        }

        public void SetupParameters()
        {
            parametersLayout.Controls.Clear();
            parametersLayout.RowCount = parameters.Count;

            foreach (var parameter in parameters)
            {
                var label = new Label
                {
                    Text = parameter.Name + ":",
                    AutoSize = true
                };

                var entry = new NumericUpDown
                {
                    Name = parameter.Name,
                    DecimalPlaces = parameter.Precision,
                    Increment = 0.0001M,
                    Minimum = -10,
                    Maximum = 10,
                    Value = (decimal)parameter.Value
                };

                parametersLayout.Controls.Add(label);
                parametersLayout.Controls.Add(entry);
            }

            Text = name + " Setup Dialog";
            image.Image = GetImage();
        }

        protected virtual ElementList CreateScenario()
        {
            throw new NotImplementedException();
        }

        protected virtual Image GetImage()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public event EventHandler<ScenarioEventArgs> ScenarioCreated;

        protected virtual void OnScenarioCreated(ScenarioEventArgs e)
        {
            ScenarioCreated?.Invoke(this, e);
        }

        public class Parameter
        {
            public string Name { get; set; }
            public string Unit { get; set; }
            public string Prefixes { get; set; }
            public int Precision { get; set; }
            public double Value { get; set; }
        }

        public class ScenarioEventArgs : EventArgs
        {
            public PointF TopLeft { get; }
            public PointF BottomRight { get; }
            public ElementList List { get; }

            public ScenarioEventArgs(PointF topLeft, PointF bottomRight, ElementList list)
            {
                TopLeft = topLeft;
                BottomRight = bottomRight;
                List = list;
            }
        }
    }
}
