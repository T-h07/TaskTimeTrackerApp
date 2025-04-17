using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TaskTimeTrackerApp.Models;

namespace TaskTimeTrackerApp.Data
{
    public static class ProjectStorage
    {
        private static readonly string FilePath = "data.json";

        // Save all projects including their tasks
        public static void Save(List<Project> projects)
        {
            try
            {
                string json = JsonConvert.SerializeObject(projects, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        // Load all projects including tasks; ensure fallback to empty list
        public static List<Project> Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var loadedProjects = JsonConvert.DeserializeObject<List<Project>>(json) ?? new List<Project>();

                    // Ensure all Projects have Tasks list initialized (in case JSON is missing it)
                    foreach (var project in loadedProjects)
                    {
                        if (project.Tasks == null)
                            project.Tasks = new List<TaskItem>();
                    }

                    return loadedProjects;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }

            return new List<Project>();
        }
    }
}
