using System.Windows;
using System.ComponentModel;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for CreateEventLogDesigner.xaml
	public partial class CreateEventLogDesigner
	{
		public CreateEventLogDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.CreateEventLog modelItem = (Activities.CreateEventLog)ModelItem.GetCurrentValue();

			AttributeCollection attributes = TypeDescriptor.GetProperties(modelItem)["TreatExistAsSuccess"].Attributes;
			DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];

			if (modelItem.TreatExistAsSuccess == null && defaultValueAttribute != null)
				chkTreatExistsAsSuccess.IsChecked = (bool)defaultValueAttribute.Value;

			attributes = TypeDescriptor.GetProperties(modelItem)["ThrowOnError"].Attributes;
			defaultValueAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];

			if (modelItem.ThrowOnError == null && defaultValueAttribute != null)
				chkThrowOnError.IsChecked = (bool)defaultValueAttribute.Value;
		}
	}
}
