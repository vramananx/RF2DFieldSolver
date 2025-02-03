using System;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver
{
    public class VertexEditDialog : Form
    {
        private TextBox txtX;
        private TextBox txtY;
        private Button btnOk;
        private Button btnCancel;

        public VertexEditDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtX = new TextBox();
            this.txtY = new TextBox();
            this.btnOk = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            // txtX
            this.txtX.Location = new Point(12, 12);
            this.txtX.Name = "txtX";
            this.txtX.Size = new Size(100, 20);
            this.txtX.TabIndex = 0;

            // txtY
            this.txtY.Location = new Point(12, 38);
            this.txtY.Name = "txtY";
            this.txtY.Size = new Size(100, 20);
            this.txtY.TabIndex = 1;

            // btnOk
            this.btnOk.Location = new Point(12, 64);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new EventHandler(this.BtnOk_Click);

            // btnCancel
            this.btnCancel.Location = new Point(93, 64);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click);

            // VertexEditDialog
            this.ClientSize = new Size(184, 101);
            this.Controls.Add(this.txtX);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Name = "VertexEditDialog";
            this.Text = "Edit Vertex";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public float X
        {
            get { return float.Parse(txtX.Text); }
            set { txtX.Text = value.ToString(); }
        }

        public float Y
        {
            get { return float.Parse(txtY.Text); }
            set { txtY.Text = value.ToString(); }
        }
    }
}
