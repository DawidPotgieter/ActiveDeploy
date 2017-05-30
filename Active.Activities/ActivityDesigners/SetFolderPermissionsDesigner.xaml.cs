using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for SetFolderPermissionsDesigner.xaml
	public partial class SetFolderPermissionsDesigner
	{
		public SetFolderPermissionsDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.SetFolderPermissions modelItem = (Activities.SetFolderPermissions)ModelItem.GetCurrentValue();

			bool? defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "Read");

			if (modelItem.Read == null && defaultValue != null)
				chkRead.IsChecked = defaultValue.Value;

			defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "Write");

			if (modelItem.Write == null && defaultValue != null)
				chkWrite.IsChecked = defaultValue.Value;

			defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "ReadAndExecute");

			if (modelItem.ReadAndExecute == null && defaultValue != null)
				chkExecute.IsChecked = defaultValue.Value;

			defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "ShowOutput");

			if (modelItem.ShowOutput == null && defaultValue != null)
				chkShowOutput.IsChecked = defaultValue.Value;
		}

		private void btnPickIdentityName_Click(object sender, RoutedEventArgs e)
		{
			CustomDialogs.FindIdentities findIdentitiesWindow = new CustomDialogs.FindIdentities(true);
			var result = findIdentitiesWindow.ShowDialog();
			if (result.HasValue && result.Value)
			{
				Helpers.DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "UserOrGroup", "\"" + findIdentitiesWindow.IdentityName + "\"");
			}
		}

		private void btnPickFileSystemPath_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFolder(ModelItem, "FolderPath", false);
		}
	}
}
