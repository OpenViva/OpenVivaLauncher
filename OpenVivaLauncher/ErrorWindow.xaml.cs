using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	/// Interaction logic for ErrorWindow.xaml
	/// </summary>
	public partial class ErrorWindow : Window
	{
		private Exception Exception { get; set; }
		private string Location { get; set; }
		public ErrorWindow(Exception e, string location = null)
		{
			this.Exception = e;
			this.Location = location;
			InitializeComponent();
		}
		private void ErrorWindow_OnLoad(object sender, RoutedEventArgs e)
		{
			this.Title = $"Error";
			//StringBuilder builder = new StringBuilder();
			//builder.AppendLine($"An exception has occurred:");
			//builder.AppendLine($"{Exception.Message}");
			//builder.AppendLine($"=====================");
			//builder.AppendLine($"{Exception.StackTrace}");
			//builder.AppendLine($"=====================");
			//builder.AppendLine($"{Exception.Data}");
			//this.MainBlock.Text = builder.ToString();
			this.MainBlock.Text = this.Exception.ToString();
			if (string.IsNullOrEmpty(this.Location))
			{
				this.LocationTextBlock.Visibility = Visibility.Hidden;
				this.Location = "Location not provided";
			}
			else
			{
				this.LocationTextBlock.Visibility = Visibility.Visible;
				this.LocationTextBlock.Text = this.Location;
			}
		}
		private void CloseButton_OnClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void CopyButton_OnClick(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText($"Throw Location: {this.Location}\n============================\n{this.MainBlock.Text}");
		}
		private void IssueTrackerButton_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://github.com/OpenViva/OpenVivaLauncher/issues/new") { UseShellExecute = true });
		}
		public void LogException(System.Exception ex)
		{
			var sb = new System.Text.StringBuilder();
			while (ex != null)
			{
				sb.AppendLine(ex.Message);
				ex = ex.InnerException;
			}
			//Log(sb.ToString());
		}
	}
}
