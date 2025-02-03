namespace RF2DFieldSolver
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStartCalculation = new System.Windows.Forms.Button();
            this.btnAbortCalculation = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnAddElement = new System.Windows.Forms.Button();
            this.btnRemoveElement = new System.Windows.Forms.Button();
            this.btnShowPotential = new System.Windows.Forms.CheckBox();
            this.btnShowGrid = new System.Windows.Forms.CheckBox();
            this.btnSnapToGrid = new System.Windows.Forms.CheckBox();
            this.btnKeepAspectRatio = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnStartCalculation
            // 
            this.btnStartCalculation.Location = new System.Drawing.Point(12, 12);
            this.btnStartCalculation.Name = "btnStartCalculation";
            this.btnStartCalculation.Size = new System.Drawing.Size(150, 23);
            this.btnStartCalculation.TabIndex = 0;
            this.btnStartCalculation.Text = "Start Calculation";
            this.btnStartCalculation.UseVisualStyleBackColor = true;
            this.btnStartCalculation.Click += new System.EventHandler(this.btnStartCalculation_Click);
            // 
            // btnAbortCalculation
            // 
            this.btnAbortCalculation.Location = new System.Drawing.Point(12, 41);
            this.btnAbortCalculation.Name = "btnAbortCalculation";
            this.btnAbortCalculation.Size = new System.Drawing.Size(150, 23);
            this.btnAbortCalculation.TabIndex = 1;
            this.btnAbortCalculation.Text = "Abort Calculation";
            this.btnAbortCalculation.UseVisualStyleBackColor = true;
            this.btnAbortCalculation.Click += new System.EventHandler(this.btnAbortCalculation_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 70);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 99);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(150, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnAddElement
            // 
            this.btnAddElement.Location = new System.Drawing.Point(12, 128);
            this.btnAddElement.Name = "btnAddElement";
            this.btnAddElement.Size = new System.Drawing.Size(150, 23);
            this.btnAddElement.TabIndex = 4;
            this.btnAddElement.Text = "Add Element";
            this.btnAddElement.UseVisualStyleBackColor = true;
            this.btnAddElement.Click += new System.EventHandler(this.btnAddElement_Click);
            // 
            // btnRemoveElement
            // 
            this.btnRemoveElement.Location = new System.Drawing.Point(12, 157);
            this.btnRemoveElement.Name = "btnRemoveElement";
            this.btnRemoveElement.Size = new System.Drawing.Size(150, 23);
            this.btnRemoveElement.TabIndex = 5;
            this.btnRemoveElement.Text = "Remove Element";
            this.btnRemoveElement.UseVisualStyleBackColor = true;
            this.btnRemoveElement.Click += new System.EventHandler(this.btnRemoveElement_Click);
            // 
            // btnShowPotential
            // 
            this.btnShowPotential.AutoSize = true;
            this.btnShowPotential.Location = new System.Drawing.Point(12, 186);
            this.btnShowPotential.Name = "btnShowPotential";
            this.btnShowPotential.Size = new System.Drawing.Size(100, 17);
            this.btnShowPotential.TabIndex = 6;
            this.btnShowPotential.Text = "Show Potential";
            this.btnShowPotential.UseVisualStyleBackColor = true;
            this.btnShowPotential.CheckedChanged += new System.EventHandler(this.btnShowPotential_CheckedChanged);
            // 
            // btnShowGrid
            // 
            this.btnShowGrid.AutoSize = true;
            this.btnShowGrid.Location = new System.Drawing.Point(12, 209);
            this.btnShowGrid.Name = "btnShowGrid";
            this.btnShowGrid.Size = new System.Drawing.Size(75, 17);
            this.btnShowGrid.TabIndex = 7;
            this.btnShowGrid.Text = "Show Grid";
            this.btnShowGrid.UseVisualStyleBackColor = true;
            this.btnShowGrid.CheckedChanged += new System.EventHandler(this.btnShowGrid_CheckedChanged);
            // 
            // btnSnapToGrid
            // 
            this.btnSnapToGrid.AutoSize = true;
            this.btnSnapToGrid.Location = new System.Drawing.Point(12, 232);
            this.btnSnapToGrid.Name = "btnSnapToGrid";
            this.btnSnapToGrid.Size = new System.Drawing.Size(85, 17);
            this.btnSnapToGrid.TabIndex = 8;
            this.btnSnapToGrid.Text = "Snap to Grid";
            this.btnSnapToGrid.UseVisualStyleBackColor = true;
            this.btnSnapToGrid.CheckedChanged += new System.EventHandler(this.btnSnapToGrid_CheckedChanged);
            // 
            // btnKeepAspectRatio
            // 
            this.btnKeepAspectRatio.AutoSize = true;
            this.btnKeepAspectRatio.Location = new System.Drawing.Point(12, 255);
            this.btnKeepAspectRatio.Name = "btnKeepAspectRatio";
            this.btnKeepAspectRatio.Size = new System.Drawing.Size(115, 17);
            this.btnKeepAspectRatio.TabIndex = 9;
            this.btnKeepAspectRatio.Text = "Keep Aspect Ratio";
            this.btnKeepAspectRatio.UseVisualStyleBackColor = true;
            this.btnKeepAspectRatio.CheckedChanged += new System.EventHandler(this.btnKeepAspectRatio_CheckedChanged);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnKeepAspectRatio);
            this.Controls.Add(this.btnSnapToGrid);
            this.Controls.Add(this.btnShowGrid);
            this.Controls.Add(this.btnShowPotential);
            this.Controls.Add(this.btnRemoveElement);
            this.Controls.Add(this.btnAddElement);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAbortCalculation);
            this.Controls.Add(this.btnStartCalculation);
            this.Name = "MainForm";
            this.Text = "RF2DFieldSolver";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnStartCalculation;
        private System.Windows.Forms.Button btnAbortCalculation;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnAddElement;
        private System.Windows.Forms.Button btnRemoveElement;
        private System.Windows.Forms.CheckBox btnShowPotential;
        private System.Windows.Forms.CheckBox btnShowGrid;
        private System.Windows.Forms.CheckBox btnSnapToGrid;
        private System.Windows.Forms.CheckBox btnKeepAspectRatio;
    }
}
