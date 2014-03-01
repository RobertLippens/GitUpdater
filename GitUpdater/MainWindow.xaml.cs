using GitUpdater.Properties;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace GitUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ErrorSection.Foreground = Brushes.Red;
            this.Username.Text = Settings.Default.Username;
            this.Password.Password = Settings.Default.Password;
            this.VSBuildAuto.IsChecked = Settings.Default.VSAutoBuild;
            this.FrequencySelector.Text = Settings.Default.FrequencySelector.ToString();
            this.FolderLocator1.Text = Settings.Default.Directory1;
            this.FolderLocator2.Text = Settings.Default.Directory2;
            this.FolderLocator3.Text = Settings.Default.Directory3;
        }

        private void FolderBrowser(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            FrameworkElement feSource = e.Source as FrameworkElement;
            switch (feSource.Name)
            {
                case "FolderButton1":
                    FolderLocator1.Text = dialog.SelectedPath;
                    break;
                case "FolderButton2":
                    FolderLocator2.Text = dialog.SelectedPath;
                    break;
                case "FolderButton3":
                    FolderLocator3.Text = dialog.SelectedPath;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (settingsHaveBeenVerified())
            {
                Settings.Default.Username = Username.Text.ToString();
                Settings.Default.Password = Password.Password.ToString();
                Settings.Default.VSAutoBuild = VSBuildAuto.IsChecked.Value;
                Settings.Default.FrequencySelector = Convert.ToInt32(FrequencySelector.Text);
                Settings.Default.Directory1 = FolderLocator1.Text.ToString();
                Settings.Default.Directory2 = FolderLocator2.Text.ToString();
                Settings.Default.Directory3 = FolderLocator3.Text.ToString();
                Settings.Default.Save();
                var chooseReposWindow = new FileChooser();
                chooseReposWindow.Show();
                this.Close();
            }
        }

        private bool settingsHaveBeenVerified()
        {
            return (checkIfDirectoriesExist() && usernameExists() && passwordExists() && intervalExists());
        }

        private bool checkIfDirectoriesExist()
        {
            List<string> directories = new List<string>() {
                FolderLocator1.Text,
                FolderLocator2.Text,
                FolderLocator3.Text
            };

            if (directories.All(s => string.IsNullOrEmpty(s)))
            {
                this.ErrorSection.Text = "You must choose at least one directory.";
                return false;
            }

            foreach (string directory in directories)
            {
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    this.ErrorSection.Text = "The chosen file directory " + directory + " does not exist.";
                    return false;
                }
            }
            return true;
        }

        private bool usernameExists()
        {
            if (string.IsNullOrEmpty(Username.Text.ToString()))
            {
                this.ErrorSection.Text = "You must enter your username.";
                return false;
            }
            return true;
        }

        private bool passwordExists()
        {
            if (string.IsNullOrEmpty(Password.Password.ToString()))
            {
                this.ErrorSection.Text = "You must enter your password.";
                return false;
            }
            return true;
        }

        private bool intervalExists()
        {
            if (string.IsNullOrEmpty(FrequencySelector.Text.ToString()))
            {
                this.ErrorSection.Text = "You must enter an interval.";
                return false;
            }
            else if (Regex.IsMatch(FrequencySelector.Text.ToString(), @"\D+"))
            {
                this.ErrorSection.Text = "Intervals can only be numeric and non-negative.";
                return false;
            }
            else if (Convert.ToDouble(FrequencySelector.Text.ToString()) > 730)
            {
                this.ErrorSection.Text = "Intervals must be under 730 hours.";
                return false;
            }
            return true;
        }

    }
}
