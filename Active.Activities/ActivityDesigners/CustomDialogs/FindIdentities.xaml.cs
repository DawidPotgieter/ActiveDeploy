using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Active.Activities.Helpers;
using System.Windows.Threading;

namespace Active.Activities.ActivityDesigners.CustomDialogs
{
	/// <summary>
	/// Interaction logic for FindIdentities.xaml
	/// </summary>
	public partial class FindIdentities : Window
	{
		private bool isDomainsLoaded = false;
		private bool showGroups = false;

		public FindIdentities(bool showGroups)
		{
			InitializeComponent();
			this.showGroups = showGroups;
		}

		public string IdentityName { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cmbDomain.ItemsSource = new string[] { System.Environment.MachineName };
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
				{
					cmbDomain.SelectedIndex = 0;
				}));			
		}

		private List<string> GetDomainNames()
		{
			List<string> domainNames = SystemManagementObjects.GetDomainNames().ToList();
			domainNames.Insert(0, System.Environment.MachineName);
			if (System.Environment.UserDomainName != System.Environment.MachineName)
			{
				domainNames.Insert(1, System.Environment.UserDomainName);
			}
			return domainNames;
		}

		private Dictionary<string, string> GetIdentities(string domain)
		{
			Dictionary<string, string> identities = new Dictionary<string, string>();
			SystemManagementObjects.GetUsernames(domain).ToList().ForEach(u => identities.Add(u, "(u) " + u));
			if (showGroups)
			{
				SystemManagementObjects.GetGroupNames(domain).ToList().ForEach(g => identities.Add(g, "(g) " + g));
			}
			return identities;
		}

		private void cmbDomain_DropDownOpened(object sender, EventArgs e)
		{
			if (!isDomainsLoaded)
			{
				Mouse.OverrideCursor = Cursors.Wait;
				cmbDomain.ItemsSource = GetDomainNames();
				Mouse.OverrideCursor = null;
				isDomainsLoaded = true;
			}
		}

		private void cmbDomain_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			cmbIdentity.ItemsSource = GetIdentities((string)cmbDomain.SelectedValue);
			Mouse.OverrideCursor = null;
		}

		private void cmbIdentity_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IdentityName = cmbIdentity.SelectedValue as string;
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
	}
}
