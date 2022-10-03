using Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace OpenVivaLauncher
{
    /// <summary>
    /// Interaction logic for StartupWIndow.xaml
    /// </summary>
    public partial class StartupWIndow : Window
    {
        public StartupWIndow()
        {
            InitializeComponent();
            base.Loaded += StartupWindow_Loaded;
            base.MouseDown += StartupWindow_OnMouseDown;
        }

        private async void StartupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            SetProgressBar(1, "Checking for folder existance " + Directories.GameInstallLocation, 0, 4);
            if (!Directory.Exists(Directories.GameInstallLocation))
            {
                SetProgressBar(2, "Folder Not Found, Creating " + Directories.GameInstallLocation, 0, 4);
                Directory.CreateDirectory(Directories.GameInstallLocation);
            }
            try
            {
                SetProgressBar(3, "Checking for updates", 0, 4);
                string repourl = "https://github.com/OpenViva/OpenVivaLauncher";
                using var mgr = new GithubUpdateManager(repourl);
                if (await mgr.UpdateApp() != null)
                {
                    SetProgressBar(4, "Update installed Restarting", 0, 4);
                    await Task.Delay(1000);
                    GithubUpdateManager.RestartApp();
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow(ex, "Updater").ShowDialog();
            }
            Close();
        }

        private void StartupWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SetProgressBar(int value, string text, int minimum, int maximum)
        {
            PrimaryBar.Value = value;
			//Since visual studio wont let me set the alignment im doing it here
            PrimaryText.HorizontalContentAlignment = HorizontalAlignment.Center;
            PrimaryText.Content = text;
            PrimaryBar.Maximum = maximum;          
            PrimaryBar.Minimum = minimum;           
        }

    }
}
