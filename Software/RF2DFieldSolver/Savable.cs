using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RF2DFieldSolver
{
    public abstract class Savable
    {
        public abstract string ToJSON();
        public abstract void FromJSON(string json);

        public bool OpenFromFileDialog(string title, string filetype)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = title;
                openFileDialog.Filter = filetype;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        FromJSON(json);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return false;
        }

        public bool SaveToFileDialog(string title, string filetype, string ending = "")
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = title;
                saveFileDialog.Filter = filetype;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filename = saveFileDialog.FileName;
                        if (!string.IsNullOrEmpty(ending) && !filename.EndsWith(ending))
                        {
                            filename += ending;
                        }
                        File.WriteAllText(filename, ToJSON());
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return false;
        }

        public class SettingDescription
        {
            public SettingDescription(object var, string name, object def)
            {
                Var = var;
                Name = name;
                Default = def;
            }

            public object Var { get; }
            public string Name { get; }
            public object Default { get; }
        }

        public static void ParseJSON(string json, List<SettingDescription> descr)
        {
            var j = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var e in descr)
            {
                if (j.TryGetValue(e.Name, out var value))
                {
                    e.Var = Convert.ChangeType(value, e.Var.GetType());
                }
                else
                {
                    e.Var = e.Default;
                }
            }
        }

        public static string CreateJSON(List<SettingDescription> descr)
        {
            var j = new Dictionary<string, object>();
            foreach (var e in descr)
            {
                j[e.Name] = e.Var;
            }
            return JsonConvert.SerializeObject(j, Formatting.Indented);
        }
    }
}
