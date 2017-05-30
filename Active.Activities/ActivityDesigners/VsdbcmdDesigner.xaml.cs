using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for VsdbcmdDesigner.xaml
	public partial class VsdbcmdDesigner
	{
		public VsdbcmdDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.Vsdbcmd modelItem = (Activities.Vsdbcmd)ModelItem.GetCurrentValue();

			bool? defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "AlwaysCreateNewDatabase");

			if (modelItem.AlwaysCreateNewDatabase == null && defaultValue != null)
				chkAlwaysCreateNewDatabase.IsChecked = defaultValue.Value;

			defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "BackupBeforeDeploy");

			if (modelItem.BackupBeforeDeploy == null && defaultValue != null)
				chkBackupBeforeDeploy.IsChecked = defaultValue.Value;
		}

		private void btnPickVsdbcmdPath_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Vsdbcmd.exe|Vsdbcmd.exe|All Files|*.*", "VsdbcmdPath", false);
		}

		private void btnConvertVsdbcmdPathToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtVsdbcmd);
		}

		private void btnPickManifestFile_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Database Manifest Files (*.deploymanifest)|*.deploymanifest|All Files|*.*", "ManifestFilename");
		}
	}
}
