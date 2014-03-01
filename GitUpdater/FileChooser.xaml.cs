using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GitUpdater.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Reflection;
using System.Collections.Specialized;

namespace GitUpdater
{
    /// <summary>
    /// Interaction logic for FileChooser.xaml
    /// </summary>
    public partial class FileChooser : Window
    {
        Dictionary<string, string> fullRepos = new Dictionary<string, string>();

        private List<string> gitDirectories = new List<string>();

        private List<string> directories = new List<string>() {
                Settings.Default.Directory1,
                Settings.Default.Directory2,
                Settings.Default.Directory3
            };

        public FileChooser()
        {
            InitializeComponent();
            Settings.Default.FullRepoLocation.Clear();
            if (Settings.Default.ChosenRepositories != null) { 
                foreach (string chosenRepo in Settings.Default.ChosenRepositories) { 
                this.ChosenRepos.Items.Add(chosenRepo);
                }
            }

            foreach (string directory in directories)
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    gitDirectories.AddRange(Directory.GetDirectories(directory));
                }
            }

            foreach (string gitRepo in gitDirectories)
            {
                string fullPath = System.IO.Path.GetFullPath(gitRepo).TrimEnd(System.IO.Path.DirectorySeparatorChar);
                string projectName = System.IO.Path.GetFileName(fullPath);
                if (Directory.Exists(fullPath))
                {
                    this.ListOfRepos.Items.Add(projectName);
                    fullRepos.Add(projectName, fullPath);
                }
            }
        }

        private void MoveToChosenList(object sender, KeyEventArgs e)
        {

            foreach (string chosenRepo in ListOfRepos.SelectedItems)
            {
                if (e.Key == Key.Right && !ChosenRepos.Items.Contains(chosenRepo))
                {
                    ChosenRepos.Items.Add(chosenRepo);
                }
            }
        }

        private void RemoveFromChosenList(object sender, KeyEventArgs e)
        {

            List<string> reposToBeRemoved = new List<string>();
            foreach (string chosenRepo in ChosenRepos.SelectedItems)
            {
                if (e.Key == Key.Left)
                {
                    reposToBeRemoved.Add(chosenRepo);
                }
            }

            for (int i = 0; i < reposToBeRemoved.Count; i++)
            {
                ChosenRepos.Items.Remove(reposToBeRemoved[i]);
            }

            ChosenRepos.Items.Refresh();
        }

        private void scheduleNextTask()
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Automatically updates selected Git repositories.";
                DailyTrigger timeToUpdateRepositories = new DailyTrigger();
                timeToUpdateRepositories.Repetition.Interval = TimeSpan.FromHours(Settings.Default.FrequencySelector);
                td.Triggers.Add(timeToUpdateRepositories);
                td.Actions.Add(new ExecAction(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\GitUpdater.exe", null, null));
                ts.RootFolder.RegisterTaskDefinition(@"GitUpdater", td);
            }
        }

        private void SavePreferencesAndRun(object sender, RoutedEventArgs e)
        {
            Settings.Default.ChosenRepositories = new StringCollection();
            foreach (string chosenRepo in ChosenRepos.Items)
            {
                Settings.Default.ChosenRepositories.Add(chosenRepo);
                if (fullRepos.Keys.Contains(chosenRepo))
                {
                    Settings.Default.FullRepoLocation.Add(fullRepos[chosenRepo]);
                }
            }
            Settings.Default.Save();
            scheduleNextTask();
            Close();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MainWindow>().Any(w => w.Name.Equals("PreferencesPane")))
            {
                Application.Current.Windows.OfType<MainWindow>().Single().Focus();
            }
            else
            {
                var preferencePanel = new MainWindow();
                preferencePanel.Show();
            }
            Close();
        }

    }
}
