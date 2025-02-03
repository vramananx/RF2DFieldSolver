using System;
using System.Windows.Forms;
using RF2DFieldSolver.Scenarios;

namespace RF2DFieldSolver
{
    public partial class MainForm : Form
    {
        private ElementList list;
        private Laplace laplace;

        public MainForm()
        {
            InitializeComponent();
            list = new ElementList();
            laplace = new Laplace();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize form components and event handlers
            SetUpEventHandlers();
        }

        private void SetUpEventHandlers()
        {
            btnStartCalculation.Click += btnStartCalculation_Click;
            btnAbortCalculation.Click += btnAbortCalculation_Click;
            btnSave.Click += btnSave_Click;
            btnLoad.Click += btnLoad_Click;
            btnAddElement.Click += btnAddElement_Click;
            btnRemoveElement.Click += btnRemoveElement_Click;
            btnShowPotential.CheckedChanged += btnShowPotential_CheckedChanged;
            btnShowGrid.CheckedChanged += btnShowGrid_CheckedChanged;
            btnSnapToGrid.CheckedChanged += btnSnapToGrid_CheckedChanged;
            btnKeepAspectRatio.CheckedChanged += btnKeepAspectRatio_CheckedChanged;
        }

        private void btnStartCalculation_Click(object sender, EventArgs e)
        {
            // Start the calculation process
            StartCalculation();
        }

        private void btnAbortCalculation_Click(object sender, EventArgs e)
        {
            // Abort the calculation process
            laplace.AbortCalculation();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save the current project
            SaveProject();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Load a project
            LoadProject();
        }

        private void btnAddElement_Click(object sender, EventArgs e)
        {
            // Add a new element to the project
            AddElement();
        }

        private void btnRemoveElement_Click(object sender, EventArgs e)
        {
            // Remove the selected element from the project
            RemoveElement();
        }

        private void btnShowPotential_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle the display of potential field
            ToggleShowPotential();
        }

        private void btnShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle the display of grid
            ToggleShowGrid();
        }

        private void btnSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle the snap to grid feature
            ToggleSnapToGrid();
        }

        private void btnKeepAspectRatio_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle the keep aspect ratio feature
            ToggleKeepAspectRatio();
        }

        private void StartCalculation()
        {
            // Implementation for starting the calculation
            // Disable UI elements during calculation
            btnStartCalculation.Enabled = false;
            btnAbortCalculation.Enabled = true;
            btnSave.Enabled = false;
            btnLoad.Enabled = false;
            btnAddElement.Enabled = false;
            btnRemoveElement.Enabled = false;
            btnShowPotential.Enabled = false;
            btnShowGrid.Enabled = false;
            btnSnapToGrid.Enabled = false;
            btnKeepAspectRatio.Enabled = false;

            // Start the calculation process
            laplace.StartCalculation(list);

            // Enable UI elements after calculation
            btnStartCalculation.Enabled = true;
            btnAbortCalculation.Enabled = false;
            btnSave.Enabled = true;
            btnLoad.Enabled = true;
            btnAddElement.Enabled = true;
            btnRemoveElement.Enabled = true;
            btnShowPotential.Enabled = true;
            btnShowGrid.Enabled = true;
            btnSnapToGrid.Enabled = true;
            btnKeepAspectRatio.Enabled = true;
        }

        private void SaveProject()
        {
            // Implementation for saving the project
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "RF 2D field solver files (*.RF2Dproj)|*.RF2Dproj";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the project to the selected file
                    list.SaveToFile(saveFileDialog.FileName);
                }
            }
        }

        private void LoadProject()
        {
            // Implementation for loading the project
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "RF 2D field solver files (*.RF2Dproj)|*.RF2Dproj";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the project from the selected file
                    list.LoadFromFile(openFileDialog.FileName);
                }
            }
        }

        private void AddElement()
        {
            // Implementation for adding a new element
            Element element = new Element();
            list.AddElement(element);
        }

        private void RemoveElement()
        {
            // Implementation for removing an element
            if (list.SelectedElement != null)
            {
                list.RemoveElement(list.SelectedElement);
            }
        }

        private void ToggleShowPotential()
        {
            // Implementation for toggling the display of potential field
            laplace.ShowPotential = btnShowPotential.Checked;
        }

        private void ToggleShowGrid()
        {
            // Implementation for toggling the display of grid
            laplace.ShowGrid = btnShowGrid.Checked;
        }

        private void ToggleSnapToGrid()
        {
            // Implementation for toggling the snap to grid feature
            laplace.SnapToGrid = btnSnapToGrid.Checked;
        }

        private void ToggleKeepAspectRatio()
        {
            // Implementation for toggling the keep aspect ratio feature
            laplace.KeepAspectRatio = btnKeepAspectRatio.Checked;
        }

        private void ShowScenarioDialog()
        {
            var scenarios = Scenario.CreateAll();
            foreach (var scenario in scenarios)
            {
                scenario.ScenarioCreated += (topLeft, bottomRight, elementList) =>
                {
                    list = elementList;
                    laplace.SetArea(topLeft, bottomRight);
                };
                scenario.ShowDialog();
            }
        }

        private void ShowVertexEditDialog(PointF vertex)
        {
            using (var dialog = new VertexEditDialog())
            {
                dialog.X = vertex.X;
                dialog.Y = vertex.Y;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the vertex with new values
                    vertex.X = dialog.X;
                    vertex.Y = dialog.Y;
                }
            }
        }
    }
}
