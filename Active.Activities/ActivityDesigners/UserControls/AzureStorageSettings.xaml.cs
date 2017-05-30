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
	public partial class AzureStorageSettings : UserControl
	{
		public AzureStorageSettings()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		public System.Activities.Presentation.Model.ModelItem ModelItem
		{
			get { return (System.Activities.Presentation.Model.ModelItem)GetValue(ModelItemProperty); }
			set { SetValue(ModelItemProperty, value); }
		}

		public System.Activities.Presentation.Model.ModelItem Account
		{
			get { return (System.Activities.Presentation.Model.ModelItem)GetValue(AccountProperty); }
			set { SetValue(AccountProperty, value); }
		}

		public System.Activities.Presentation.Model.ModelItem AccountKey
		{
			get { return (System.Activities.Presentation.Model.ModelItem)GetValue(AccountKeyProperty); }
			set { SetValue(AccountKeyProperty, value); }
		}

		public static readonly DependencyProperty ModelItemProperty =
			DependencyProperty.Register("ModelItem", typeof(System.Activities.Presentation.Model.ModelItem), typeof(AzureStorageSettings));

		public static readonly DependencyProperty AccountProperty =
			DependencyProperty.Register("Account", typeof(System.Activities.Presentation.Model.ModelItem), typeof(AzureStorageSettings));

		public static readonly DependencyProperty AccountKeyProperty =
			DependencyProperty.Register("AccountKey", typeof(System.Activities.Presentation.Model.ModelItem), typeof(AzureStorageSettings));
	}
}
