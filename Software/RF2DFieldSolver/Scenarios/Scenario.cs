using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public abstract class Scenario : Form
    {
        protected List<Parameter> parameters;
        protected string name;
        protected TableLayoutPanel ui;

        public Scenario()
        {
            parameters = new List<Parameter>();
            ui = new TableLayoutPanel();
            ui.Dock = DockStyle.Fill;
            Controls.Add(ui);

            var buttonBox = new FlowLayoutPanel();
            buttonBox.Dock = DockStyle.Bottom;
            Controls.Add(buttonBox);

            var acceptButton = new Button { Text = "OK" };
            acceptButton.Click += (sender, e) => OnAccept();
            buttonBox.Controls.Add(acceptButton);

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (sender, e) => OnCancel();
            buttonBox.Controls.Add(cancelButton);

            var autoAreaCheckBox = new CheckBox { Text = "Auto Area" };
            ui.Controls.Add(autoAreaCheckBox);

            var xLeftLabel = new Label { Text = "X Left" };
            ui.Controls.Add(xLeftLabel);
            var xLeftTextBox = new TextBox();
            ui.Controls.Add(xLeftTextBox);

            var xRightLabel = new Label { Text = "X Right" };
            ui.Controls.Add(xRightLabel);
            var xRightTextBox = new TextBox();
            ui.Controls.Add(xRightTextBox);

            var yTopLabel = new Label { Text = "Y Top" };
            ui.Controls.Add(yTopLabel);
            var yTopTextBox = new TextBox();
            ui.Controls.Add(yTopTextBox);

            var yBottomLabel = new Label { Text = "Y Bottom" };
            ui.Controls.Add(yBottomLabel);
            var yBottomTextBox = new TextBox();
            ui.Controls.Add(yBottomTextBox);
        }

        protected abstract ElementList CreateScenario();
        protected abstract Image GetImage();

        private void OnAccept()
        {
            foreach (var parameter in parameters)
            {
                var control = ui.Controls[parameter.Name] as TextBox;
                if (control != null)
                {
                    parameter.Value = double.Parse(control.Text);
                }
            }

            var list = CreateScenario();
            var topLeft = new PointF(float.Parse(ui.Controls["X Left"].Text), float.Parse(ui.Controls["Y Top"].Text));
            var bottomRight = new PointF(float.Parse(ui.Controls["X Right"].Text), float.Parse(ui.Controls["Y Bottom"].Text));
            ScenarioCreated?.Invoke(topLeft, bottomRight, list);
            Close();
        }

        private void OnCancel()
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
            foreach (var parameter in parameters)
            {
                var label = new Label { Text = parameter.Name + ":" };
                var textBox = new TextBox { Name = parameter.Name, Text = parameter.Value.ToString() };
                ui.Controls.Add(label);
                ui.Controls.Add(textBox);
            }

            Text = name + " Setup Dialog";

            var pictureBox = new PictureBox
            {
                Image = GetImage(),
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            ui.Controls.Add(pictureBox);
        }

        public string Name => name;

        public event Action<PointF, PointF, ElementList> ScenarioCreated;

        public class Parameter
        {
            public string Name { get; set; }
            public string Unit { get; set; }
            public string Prefixes { get; set; }
            public int Precision { get; set; }
            public double Value { get; set; }
        }
    }
}
