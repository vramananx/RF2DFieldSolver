using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

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
