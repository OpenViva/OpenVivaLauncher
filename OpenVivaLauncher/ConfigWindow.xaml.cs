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
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace OpenVivaLauncher
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Config cfg = Config.GetSettings();
            FolderText.Text = cfg.GameInstallLocation;
            UpdateCheckBox.IsChecked = cfg.CheckforLauncherUpdates;
        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            Config cfg = Config.GetSettings();
            VistaFolderBrowserDialog folder = new VistaFolderBrowserDialog();
            folder.ShowNewFolderButton = true;

            folder.RootFolder = Environment.SpecialFolder.ProgramFiles;

            if(folder.ShowDialog() == true)
            {
                cfg.GameInstallLocation = folder.SelectedPath + "\\Game\\";
                FolderText.Text = cfg.GameInstallLocation;
            }
        }

        private void FolderText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void UpdateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Config cfg = Config.GetSettings();
            cfg.CheckforLauncherUpdates = true;
        }

        private void UpdateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Config cfg = Config.GetSettings();
            cfg.CheckforLauncherUpdates = false;
        }
    }
}
