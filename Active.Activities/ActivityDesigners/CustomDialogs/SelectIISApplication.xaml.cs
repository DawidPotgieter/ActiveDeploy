using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Web.Administration.Wrapper;

namespace Active.Activities.ActivityDesigners.CustomDialogs
{
	/// <summary>
	/// Interaction logic for SelectIISVirtualDirectory.xaml
	/// </summary>
	public partial class SelectIISApplication : Window
	{
		public SelectIISApplication()
		{
			InitializeComponent();
		}

		public string VirtualDirectoryPath { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				cmbSite.ItemsSource = GetIISSiteNames();
				this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
				{
					cmbSite.SelectedIndex = 0;
				}));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Could not load IIS Sites.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				this.DialogResult = false;
				this.Close();
			}
		}

		private List<string> GetIISSiteNames()
		{
			List<string> siteNames = new List<string>();
			try
			{
				using (ServerManager serverManager = new ServerManager())
				{
					siteNames = serverManager.Sites.Select(s => s.Name).ToList();
				}
			}
			catch (System.IO.FileNotFoundException ex)
			{
				if (ex.Message.Contains("Microsoft.Web.Administration"))
				{
					MessageBox.Show("This dialog requires that the IIS Management objects are fully installed on the current machine.  Ensure that you can find the assembly 'Microsoft.Web.Administration.dll' at the path '%WinDir%\\System32\\InetSrv' to use this dialog.", "Could not load IIS Sites.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				this.DialogResult = false;
				this.Close();
			}
			return siteNames;
		}

		private List<string> GetApplicationPaths(string siteName)
		{
			List<string> applicationPaths = new List<string>();
			using (ServerManager serverManager = new ServerManager())
			{
				Site site = serverManager.Sites[siteName];
				foreach (Microsoft.Web.Administration.Wrapper.Application application in site.Applications)
				{
					applicationPaths.Add(site.Name + application.Path);
				}
				return applicationPaths;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			this.Close();
		}

		private void cmbSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			cmbApplicationPath.ItemsSource = GetApplicationPaths((string)cmbSite.SelectedValue);
			cmbApplicationPath.SelectedIndex = 0;
			Mouse.OverrideCursor = null;
		}

		private void cmbVirtualDirectory_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			VirtualDirectoryPath = cmbApplicationPath.SelectedValue as string;
		}
	}
}
