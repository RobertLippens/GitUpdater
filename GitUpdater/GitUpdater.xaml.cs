using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GitUpdater.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Reflection;

namespace GitUpdater
{
    /// <summary>
    /// Interaction logic for Logger.xaml
    /// </summary>
    public partial class Logger : Window
    {
        private readonly List<string> directories = new List<string>() {
                Settings.Default.Directory1,
                Settings.Default.Directory2,
                Settings.Default.Directory3
            };

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public Logger()
        {
            InitializeComponent();
            Loaded += Logger_Loaded;
            worker.DoWork += RunUpdater;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.ProgressChanged += new ProgressChangedEventHandler(UiUpdater);
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoggingSection.Items.Add("Finished.");
            LoggingSection.Items.Refresh();
        }

        public void Logger_Loaded(object sender, RoutedEventArgs e)
        {
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += OnTimerEvent;
            dispatcherTimer.Interval = new TimeSpan(Settings.Default.FrequencySelector, 0, 0);
            dispatcherTimer.Start();
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                LoggingSection.Items.Add("Beginning process...");
                worker.RunWorkerAsync();
            }
            else
            {
                LoggingSection.Items.Add("The process is already running!");
                LoggingSection.Items.Refresh();
            }
            worker.Dispose();
        }

        private void BuildSolution()
        {
            if (!Settings.Default.VSAutoBuild) return;
            var solutionBuilder = new SolutionBuilder();
            try
            {
                solutionBuilder.BuildSolutions(solutionBuilder.FindSolutions());
            }
            catch (Exception e)
            {
                worker.ReportProgress(0, e.Message);
            }
            worker.ReportProgress(0, solutionBuilder.buildLog.ToString());
        }

        private void OpenPreferences(object sender, RoutedEventArgs e)
        {
            showPreferences();
        }

        private void CloseProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private bool PreferencesAreSet() {
            return (!string.IsNullOrEmpty(Settings.Default.Username) &&
                    !string.IsNullOrEmpty(Settings.Default.Password) &&
                    directories.Any(s => string.IsNullOrEmpty(s)));
        }


        private void showPreferences()
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
            if (Application.Current.Windows.OfType<FileChooser>().Any(w => w.Name.Equals("FileChooserWindow")))
            {
                Application.Current.Windows.OfType<FileChooser>().Single().Close();
            }
        }

        private void RunGitLogger()
        {
            var gitPuller = new GitRepoPuller();
            foreach (string repoLocation in Properties.Settings.Default.FullRepoLocation)
            {
                    gitPuller.UpdateRepository(repoLocation);
                    worker.ReportProgress(0, gitPuller.gitLog.ToString());
            }
        }

        private void CheckTaskTimer()
        {
            using (var ts = new TaskService())
            {
                    DateTime taskRuntime = ts.RootFolder.Tasks.Single(t => t.Name.Equals("GitUpdater")).NextRunTime;
                    LoggingSection.Items.Add("Next scheduled update:  " + taskRuntime.ToString());
            }
        }

        private void DeleteScheduledTask(object sender, RoutedEventArgs e)
        {
            using (TaskService ts = new TaskService())
            {
                if (ts.RootFolder.Tasks.Any(t => t.Name.Equals("GitUpdater")))
                {
                    ts.RootFolder.DeleteTask("GitUpdater");
                    LoggingSection.Items.Add("Updates stopped.");
                }
                else
                {
                    LoggingSection.Items.Add("All automatic updates have already been stopped.");
                }
            }
        }

        private void RunUpdater(object source, DoWorkEventArgs e)
        {
            if (PreferencesAreSet())
            {
                    RunGitLogger();
                    BuildSolution();
                    CheckTaskTimer();
            }
            else
            {
                showPreferences();
            }
        }

        private void UiUpdater(object sender, ProgressChangedEventArgs e)
        {
            LoggingSection.Items.Add(e.UserState.ToString());
        }
    }
}
