using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for CreateFolderDesigner.xaml
	public partial class CreateFolderDesigner
	{
		public CreateFolderDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.CreateFolder modelItem = (Activities.CreateFolder)ModelItem.GetCurrentValue();

			bool? defaultValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "ShowOutput");

			if (modelItem.ShowOutput == null && defaultValue != null)
				chkShowOutput.IsChecked = defaultValue.Value;
		}
	}
}
