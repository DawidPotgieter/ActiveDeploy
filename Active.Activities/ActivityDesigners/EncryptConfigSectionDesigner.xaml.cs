using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for EncryptConfigSectionDesigner.xaml
	public partial class EncryptConfigSectionDesigner
	{
		public EncryptConfigSectionDesigner()
		{
			InitializeComponent();
		}

		string username;

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			UsernameRow.Height = new GridLength(0);
			username = DesignerHelper.GetModelItemExpressionTextValue(ModelItem, "Username");
			chkSetUserPermission.IsChecked = !string.IsNullOrEmpty(username);

			Activities.EncryptConfigSection modelItem = (Activities.EncryptConfigSection)ModelItem.GetCurrentValue();

			bool? defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "IsWebsite");

			if (modelItem.IsWebsite == null && defaultValue != null)
				chkIsWebsite.IsChecked = defaultValue.Value;
		}
		
		private void btnPickConfigFilename_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Config Files (*.config)|*.config|All Files|*.*", "ConfigFilename", false);
		}

		private void chkSetUserPermission_Checked(object sender, RoutedEventArgs e)
		{
			UsernameRow.Height = new GridLength(1, GridUnitType.Star);
			if (!string.IsNullOrEmpty(username))
			{
				DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "Username", username);
			}
		}

		private void chkSetUserPermission_Unchecked(object sender, RoutedEventArgs e)
		{
			UsernameRow.Height = new GridLength(0);
			DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "Username", null);
		}

		private void ExpressionTextBox_EditorLostLogicalFocus(object sender, RoutedEventArgs e)
		{
			username = DesignerHelper.GetModelItemExpressionTextValue(ModelItem, "Username");
		}

		private void btnPickUserName_Click(object sender, RoutedEventArgs e)
		{
			CustomDialogs.FindIdentities findIdentitiesWindow = new CustomDialogs.FindIdentities(false);
			var result = findIdentitiesWindow.ShowDialog();
			if (result.HasValue && result.Value)
			{
				Helpers.DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "Username", "\"" + findIdentitiesWindow.IdentityName + "\"");
			}
		}
	}
}
