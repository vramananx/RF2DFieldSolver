using System;
using System.Windows.Forms;

namespace RF2DFieldSolver.CustomWidgets
{
    public class InformationBox : Form
    {
        private static bool hasGui = true;
        private CheckBox dontShowAgainCheckBox;
        private string messageId;
        private static readonly string settingsKeyPrefix = "DoNotShowDialog/";

        public InformationBox(string title, string message, MessageBoxIcon icon, string messageId)
        {
            this.messageId = messageId;
            Text = title;
            Label messageLabel = new Label
            {
                Text = message,
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };
            dontShowAgainCheckBox = new CheckBox
            {
                Text = "Do not show this message again",
                AutoSize = true,
                Location = new System.Drawing.Point(10, messageLabel.Bottom + 10)
            };
            Button okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(10, dontShowAgainCheckBox.Bottom + 10)
            };
            okButton.Click += (sender, e) => Close();
            Controls.Add(messageLabel);
            Controls.Add(dontShowAgainCheckBox);
            Controls.Add(okButton);
            ClientSize = new System.Drawing.Size(messageLabel.Width + 20, okButton.Bottom + 10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Icon = System.Drawing.SystemIcons.Information;
        }

        public static void ShowMessage(string title, string message, string messageId = "", bool block = false)
        {
            if (!hasGui)
            {
                return;
            }

            string hash = string.IsNullOrEmpty(messageId) ? message.GetHashCode().ToString() : messageId;
            string settingsKey = settingsKeyPrefix + hash;

            if (!Properties.Settings.Default[settingsKey].Equals(true))
            {
                InformationBox box = new InformationBox(title, message, MessageBoxIcon.Information, hash);
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

        public static void ShowMessageBlocking(string title, string message, string messageId = "")
        {
            ShowMessage(title, message, messageId, true);
        }

        public static void ShowError(string title, string message)
        {
            if (!hasGui)
            {
                return;
            }
            InformationBox box = new InformationBox(title, message, MessageBoxIcon.Error, "");
            box.Show();
        }

        public static bool AskQuestion(string title, string question, bool defaultAnswer, string messageId = "")
        {
            if (!hasGui)
            {
                return defaultAnswer;
            }

            string hash = string.IsNullOrEmpty(messageId) ? question.GetHashCode().ToString() : messageId;
            string settingsKey = settingsKeyPrefix + hash;

            if (!Properties.Settings.Default[settingsKey].Equals(true))
            {
                InformationBox box = new InformationBox(title, question, MessageBoxIcon.Question, hash);
                box.Controls.Add(new Button
                {
                    Text = "Yes",
                    DialogResult = DialogResult.Yes,
                    Location = new System.Drawing.Point(10, box.dontShowAgainCheckBox.Bottom + 10)
                });
                box.Controls.Add(new Button
                {
                    Text = "No",
                    DialogResult = DialogResult.No,
                    Location = new System.Drawing.Point(90, box.dontShowAgainCheckBox.Bottom + 10)
                });
                DialogResult result = box.ShowDialog();
                return result == DialogResult.Yes;
            }
            else
            {
                return (bool)Properties.Settings.Default[settingsKey];
            }
        }

        public static void SetGui(bool enable)
        {
            hasGui = enable;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (dontShowAgainCheckBox.Checked)
            {
                Properties.Settings.Default[settingsKeyPrefix + messageId] = true;
                Properties.Settings.Default.Save();
            }
        }
    }
}
