using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for MSBuildCmdDesigner.xaml
	public partial class MSBuildCmdDesigner
	{
		public MSBuildCmdDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void btnPickProjectPath_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Project Files (*.csproj, *.sln)|*.csproj;*.sln|All Files|*.*", "Project", false);
		}

		private void btnConvertProjectPathToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtProject);
		}
	}
}
