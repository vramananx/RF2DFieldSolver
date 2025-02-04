namespace RF2DFieldSolver
{
    partial class MainWindow
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
            this.status = new System.Windows.Forms.TextBox();
            this.update = new System.Windows.Forms.Button();
            this.abort = new System.Windows.Forms.Button();
            this.add = new System.Windows.Forms.Button();
            this.remove = new System.Windows.Forms.Button();
            this.table = new System.Windows.Forms.DataGridView();
            this.view = new PCBView();
            this.resolution = new System.Windows.Forms.NumericUpDown();
            this.gaussDistance = new System.Windows.Forms.NumericUpDown();
            this.tolerance = new System.Windows.Forms.NumericUpDown();
            this.threads = new System.Windows.Forms.NumericUpDown();
            this.borderIsGND = new System.Windows.Forms.CheckBox();
            this.xleft = new System.Windows.Forms.NumericUpDown();
            this.xright = new System.Windows.Forms.NumericUpDown();
            this.ytop = new System.Windows.Forms.NumericUpDown();
            this.ybottom = new System.Windows.Forms.NumericUpDown();
            this.gridsize = new System.Windows.Forms.NumericUpDown();
            this.showPotential = new System.Windows.Forms.CheckBox();
            this.showGrid = new System.Windows.Forms.CheckBox();
            this.snapGrid = new System.Windows.Forms.CheckBox();
            this.viewMode = new System.Windows.Forms.ComboBox();
            this.capacitanceP = new System.Windows.Forms.NumericUpDown();
            this.inductanceP = new System.Windows.Forms.NumericUpDown();
            this.impedanceP = new System.Windows.Forms.NumericUpDown();
            this.capacitanceN = new System.Windows.Forms.NumericUpDown();
            this.inductanceN = new System.Windows.Forms.NumericUpDown();
            this.impedanceN = new System.Windows.Forms.NumericUpDown();
            this.impedanceDiff = new System.Windows.Forms.NumericUpDown();
            this.progress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolution)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gaussDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.threads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xleft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xright)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ytop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ybottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridsize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.capacitanceP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inductanceP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.capacitanceN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inductanceN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceDiff)).BeginInit();
            this.SuspendLayout();
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(12, 12);
            this.status.Multiline = true;
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(776, 100);
            this.status.TabIndex = 0;
            // 
            // update
            // 
            this.update.Location = new System.Drawing.Point(12, 118);
            this.update.Name = "update";
            this.update.Size = new System.Drawing.Size(75, 23);
            this.update.TabIndex = 1;
            this.update.Text = "Update";
            this.update.UseVisualStyleBackColor = true;
            this.update.Click += new System.EventHandler(this.StartCalculation);
            // 
            // abort
            // 
            this.abort.Location = new System.Drawing.Point(93, 118);
            this.abort.Name = "abort";
            this.abort.Size = new System.Drawing.Size(75, 23);
            this.abort.TabIndex = 2;
            this.abort.Text = "Abort";
            this.abort.UseVisualStyleBackColor = true;
            this.abort.Click += new System.EventHandler(this.AbortCalculation);
            // 
            // add
            // 
            this.add.Location = new System.Drawing.Point(174, 118);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(75, 23);
            this.add.TabIndex = 3;
            this.add.Text = "Add";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.AddElement);
            // 
            // remove
            // 
            this.remove.Location = new System.Drawing.Point(255, 118);
            this.remove.Name = "remove";
            this.remove.Size = new System.Drawing.Size(75, 23);
            this.remove.TabIndex = 4;
            this.remove.Text = "Remove";
            this.remove.UseVisualStyleBackColor = true;
            this.remove.Click += new System.EventHandler(this.RemoveElement);
            // 
            // table
            // 
            this.table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.table.Location = new System.Drawing.Point(12, 147);
            this.table.Name = "table";
            this.table.Size = new System.Drawing.Size(776, 150);
            this.table.TabIndex = 5;
            // 
            // view
            // 
            this.view.Location = new System.Drawing.Point(12, 303);
            this.view.Name = "view";
            this.view.Size = new System.Drawing.Size(776, 150);
            this.view.TabIndex = 6;
            // 
            // resolution
            // 
            this.resolution.Location = new System.Drawing.Point(12, 459);
            this.resolution.Name = "resolution";
            this.resolution.Size = new System.Drawing.Size(120, 20);
            this.resolution.TabIndex = 7;
            // 
            // gaussDistance
            // 
            this.gaussDistance.Location = new System.Drawing.Point(138, 459);
            this.gaussDistance.Name = "gaussDistance";
            this.gaussDistance.Size = new System.Drawing.Size(120, 20);
            this.gaussDistance.TabIndex = 8;
            // 
            // tolerance
            // 
            this.tolerance.Location = new System.Drawing.Point(264, 459);
            this.tolerance.Name = "tolerance";
            this.tolerance.Size = new System.Drawing.Size(120, 20);
            this.tolerance.TabIndex = 9;
            // 
            // threads
            // 
            this.threads.Location = new System.Drawing.Point(390, 459);
            this.threads.Name = "threads";
            this.threads.Size = new System.Drawing.Size(120, 20);
            this.threads.TabIndex = 10;
            // 
            // borderIsGND
            // 
            this.borderIsGND.AutoSize = true;
            this.borderIsGND.Location = new System.Drawing.Point(516, 460);
            this.borderIsGND.Name = "borderIsGND";
            this.borderIsGND.Size = new System.Drawing.Size(92, 17);
            this.borderIsGND.TabIndex = 11;
            this.borderIsGND.Text = "Border is GND";
            this.borderIsGND.UseVisualStyleBackColor = true;
            // 
            // xleft
            // 
            this.xleft.Location = new System.Drawing.Point(12, 485);
            this.xleft.Name = "xleft";
            this.xleft.Size = new System.Drawing.Size(120, 20);
            this.xleft.TabIndex = 12;
            // 
            // xright
            // 
            this.xright.Location = new System.Drawing.Point(138, 485);
            this.xright.Name = "xright";
            this.xright.Size = new System.Drawing.Size(120, 20);
            this.xright.TabIndex = 13;
            // 
            // ytop
            // 
            this.ytop.Location = new System.Drawing.Point(264, 485);
            this.ytop.Name = "ytop";
            this.ytop.Size = new System.Drawing.Size(120, 20);
            this.ytop.TabIndex = 14;
            // 
            // ybottom
            // 
            this.ybottom.Location = new System.Drawing.Point(390, 485);
            this.ybottom.Name = "ybottom";
            this.ybottom.Size = new System.Drawing.Size(120, 20);
            this.ybottom.TabIndex = 15;
            // 
            // gridsize
            // 
            this.gridsize.Location = new System.Drawing.Point(516, 485);
            this.gridsize.Name = "gridsize";
            this.gridsize.Size = new System.Drawing.Size(120, 20);
            this.gridsize.TabIndex = 16;
            // 
            // showPotential
            // 
            this.showPotential.AutoSize = true;
            this.showPotential.Location = new System.Drawing.Point(642, 486);
            this.showPotential.Name = "showPotential";
            this.showPotential.Size = new System.Drawing.Size(96, 17);
            this.showPotential.TabIndex = 17;
            this.showPotential.Text = "Show Potential";
            this.showPotential.UseVisualStyleBackColor = true;
            // 
            // showGrid
            // 
            this.showGrid.AutoSize = true;
            this.showGrid.Location = new System.Drawing.Point(12, 511);
            this.showGrid.Name = "showGrid";
            this.showGrid.Size = new System.Drawing.Size(74, 17);
            this.showGrid.TabIndex = 18;
            this.showGrid.Text = "Show Grid";
            this.showGrid.UseVisualStyleBackColor = true;
            // 
            // snapGrid
            // 
            this.snapGrid.AutoSize = true;
            this.snapGrid.Location = new System.Drawing.Point(92, 511);
            this.snapGrid.Name = "snapGrid";
            this.snapGrid.Size = new System.Drawing.Size(75, 17);
            this.snapGrid.TabIndex = 19;
            this.snapGrid.Text = "Snap Grid";
            this.snapGrid.UseVisualStyleBackColor = true;
            // 
            // viewMode
            // 
            this.viewMode.FormattingEnabled = true;
            this.viewMode.Location = new System.Drawing.Point(173, 509);
            this.viewMode.Name = "viewMode";
            this.viewMode.Size = new System.Drawing.Size(121, 21);
            this.viewMode.TabIndex = 20;
            // 
            // capacitanceP
            // 
            this.capacitanceP.Location = new System.Drawing.Point(300, 510);
            this.capacitanceP.Name = "capacitanceP";
            this.capacitanceP.Size = new System.Drawing.Size(120, 20);
            this.capacitanceP.TabIndex = 21;
            // 
            // inductanceP
            // 
            this.inductanceP.Location = new System.Drawing.Point(426, 510);
            this.inductanceP.Name = "inductanceP";
            this.inductanceP.Size = new System.Drawing.Size(120, 20);
            this.inductanceP.TabIndex = 22;
            // 
            // impedanceP
            // 
            this.impedanceP.Location = new System.Drawing.Point(552, 510);
            this.impedanceP.Name = "impedanceP";
            this.impedanceP.Size = new System.Drawing.Size(120, 20);
            this.impedanceP.TabIndex = 23;
            // 
            // capacitanceN
            // 
            this.capacitanceN.Location = new System.Drawing.Point(678, 510);
            this.capacitanceN.Name = "capacitanceN";
            this.capacitanceN.Size = new System.Drawing.Size(120, 20);
            this.capacitanceN.TabIndex = 24;
            // 
            // inductanceN
            // 
            this.inductanceN.Location = new System.Drawing.Point(804, 510);
            this.inductanceN.Name = "inductanceN";
            this.inductanceN.Size = new System.Drawing.Size(120, 20);
            this.inductanceN.TabIndex = 25;
            // 
            // impedanceN
            // 
            this.impedanceN.Location = new System.Drawing.Point(930, 510);
            this.impedanceN.Name = "impedanceN";
            this.impedanceN.Size = new System.Drawing.Size(120, 20);
            this.impedanceN.TabIndex = 26;
            // 
            // impedanceDiff
            // 
            this.impedanceDiff.Location = new System.Drawing.Point(1056, 510);
            this.impedanceDiff.Name = "impedanceDiff";
            this.impedanceDiff.Size = new System.Drawing.Size(120, 20);
            this.impedanceDiff.TabIndex = 27;
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(12, 534);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(776, 23);
            this.progress.TabIndex = 28;
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(800, 569);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.impedanceDiff);
            this.Controls.Add(this.impedanceN);
            this.Controls.Add(this.inductanceN);
            this.Controls.Add(this.capacitanceN);
            this.Controls.Add(this.impedanceP);
            this.Controls.Add(this.inductanceP);
            this.Controls.Add(this.capacitanceP);
            this.Controls.Add(this.viewMode);
            this.Controls.Add(this.snapGrid);
            this.Controls.Add(this.showGrid);
            this.Controls.Add(this.showPotential);
            this.Controls.Add(this.gridsize);
            this.Controls.Add(this.ybottom);
            this.Controls.Add(this.ytop);
            this.Controls.Add(this.xright);
            this.Controls.Add(this.xleft);
            this.Controls.Add(this.borderIsGND);
            this.Controls.Add(this.threads);
            this.Controls.Add(this.tolerance);
            this.Controls.Add(this.gaussDistance);
            this.Controls.Add(this.resolution);
            this.Controls.Add(this.view);
            this.Controls.Add(this.table);
            this.Controls.Add(this.remove);
            this.Controls.Add(this.add);
            this.Controls.Add(this.abort);
            this.Controls.Add(this.update);
            this.Controls.Add(this.status);
            this.Name = "MainWindow";
            this.Text = "RF 2D Field Solver";
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolution)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gaussDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.threads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xleft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xright)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ytop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ybottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridsize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.capacitanceP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inductanceP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.capacitanceN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inductanceN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.impedanceDiff)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox status;
        private System.Windows.Forms.Button update;
        private System.Windows.Forms.Button abort;
        private System.Windows.Forms.Button add;
        private System.Windows.Forms.Button remove;
        private System.Windows.Forms.DataGridView table;
        private PCBView view;
        private System.Windows.Forms.NumericUpDown resolution;
        private System.Windows.Forms.NumericUpDown gaussDistance;
        private System.Windows.Forms.NumericUpDown tolerance;
        private System.Windows.Forms.NumericUpDown threads;
        private System.Windows.Forms.CheckBox borderIsGND;
        private System.Windows.Forms.NumericUpDown xleft;
        private System.Windows.Forms.NumericUpDown xright;
        private System.Windows.Forms.NumericUpDown ytop;
        private System.Windows.Forms.NumericUpDown ybottom;
        private System.Windows.Forms.NumericUpDown gridsize;
        private System.Windows.Forms.CheckBox showPotential;
        private System.Windows.Forms.CheckBox showGrid;
        private System.Windows.Forms.CheckBox snapGrid;
        private System.Windows.Forms.ComboBox viewMode;
        private System.Windows.Forms.NumericUpDown capacitanceP;
        private System.Windows.Forms.NumericUpDown inductanceP;
        private System.Windows.Forms.NumericUpDown impedanceP;
        private System.Windows.Forms.NumericUpDown capacitanceN;
        private System.Windows.Forms.NumericUpDown inductanceN;
        private System.Windows.Forms.NumericUpDown impedanceN;
        private System.Windows.Forms.NumericUpDown impedanceDiff;
        private System.Windows.Forms.ProgressBar progress;
    }
}
