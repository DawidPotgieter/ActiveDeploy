using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for CopyFolderDesigner.xaml
	public partial class CopyFolderDesigner
	{
		public CopyFolderDesigner()
		{
			InitializeComponent();
		}

		private void btnPickSourceFolder_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.PickFolder(ModelItem, "Source", false);
		}

		private void btnPickTargetFolder_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.PickFolder(ModelItem, "Target", false);
		}

		private void btnConvertSourceToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtSource);
		}

		private void btnConvertTargetToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtTarget);
		}
	}
}
