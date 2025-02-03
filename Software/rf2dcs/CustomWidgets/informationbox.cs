using System;
using System.Windows.Forms;
using System.Configuration;

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
