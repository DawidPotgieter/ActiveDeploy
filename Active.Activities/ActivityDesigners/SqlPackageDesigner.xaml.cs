using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for SqlPackageDesigner.xaml
	public partial class SqlPackageDesigner
	{
		public SqlPackageDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.SqlPackage modelItem = (Activities.SqlPackage)ModelItem.GetCurrentValue();

			bool? defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "AlwaysCreateNewDatabase");

			if (modelItem.AlwaysCreateNewDatabase == null && defaultValue != null)
				chkAlwaysCreateNewDatabase.IsChecked = defaultValue.Value;

			defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "BackupBeforeDeploy");

			if (modelItem.BackupBeforeDeploy == null && defaultValue != null)
				chkBackupBeforeDeploy.IsChecked = defaultValue.Value;

			string defaultSqlPackagePath = (string)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "SqlPackagePath");

			if (modelItem.SqlPackagePath == null && defaultSqlPackagePath != null)
			{
				DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "SqlPackagePath", "\"" + defaultSqlPackagePath + "\"");
			}
		}

		private void btnPickSqlPackagePath_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "SqlPackage.exe|SqlPackage.exe|All Files|*.*", "SqlPackagePath", false);
		}

		private void btnPickDacpackFile_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Dacpac Files (*.dacpac)|*.dacpac|All Files|*.*", "DacpacFilename");
		}
	}
}
