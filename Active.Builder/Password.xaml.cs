using System.Windows;

namespace Active.Builder
{
	/// <summary>
	/// Interaction logic for Password.xaml
	/// </summary>
	public partial class Password : Window
	{
		public Password()
		{
			InitializeComponent();
		}

		public string PasswordValue { get; set; }

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			PasswordValue = txtPassword.Password;
			DialogResult = true;
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			txtPassword.Focus();
		}
	}
}
