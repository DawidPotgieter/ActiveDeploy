using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Active.Activities.ActivityDesigners
{
	public partial class AzureCreateBlobContainerDesigner
	{
		public AzureCreateBlobContainerDesigner()
		{
			InitializeComponent();
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Azure.CreateBlobContainerIfNotExists modelItem = (Azure.CreateBlobContainerIfNotExists)ModelItem.GetCurrentValue();

			AttributeCollection attributes = TypeDescriptor.GetProperties(modelItem)["ThrowOnError"].Attributes;
			DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
			defaultValueAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];

			if (modelItem.ThrowOnError == null && defaultValueAttribute != null)
				chkThrowOnError.IsChecked = (bool)defaultValueAttribute.Value;
		}
	}
}
