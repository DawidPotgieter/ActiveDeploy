using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
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

namespace Active.Activities.ActivityDesigners.UserControls
{
	public partial class AzureBlobStorageSettings : UserControl
	{
		public AzureBlobStorageSettings()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		public System.Activities.Presentation.Model.ModelItem ModelItem
		{
			get { return (System.Activities.Presentation.Model.ModelItem)GetValue(ModelItemProperty); }
			set { SetValue(ModelItemProperty, value); }
		}

		public System.Activities.Presentation.Model.ModelItem Container
		{
			get { return (System.Activities.Presentation.Model.ModelItem)GetValue(ContainerProperty); }
			set { SetValue(ContainerProperty, value); }
		}

		public static readonly DependencyProperty ModelItemProperty =
			DependencyProperty.Register("ModelItem", typeof(System.Activities.Presentation.Model.ModelItem), typeof(AzureBlobStorageSettings));

		public static readonly DependencyProperty ContainerProperty =
			DependencyProperty.Register("Container", typeof(System.Activities.Presentation.Model.ModelItem), typeof(AzureBlobStorageSettings));
	}
}
