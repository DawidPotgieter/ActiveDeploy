using System;
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

namespace Active.Builder.ExpressionEditor
{
	/// <summary>
	/// Interaction logic for Popup.xaml
	/// </summary>
	public partial class Popup
	{
		public Popup()
		{
			InitializeComponent();
		}

		internal TreeNodes SelectedItem
		{
			get { return (TreeNodes)lblIntellisense.SelectedItem; }
			set { lblIntellisense.SelectedItem = value; }
		}

		internal int SelectedIndex
		{
			get { return lblIntellisense.SelectedIndex; }
			set
			{
				if ((value >= lblIntellisense.Items.Count) || (value < -1))
					return;
				lblIntellisense.SelectedIndex = value;
				lblIntellisense.ScrollIntoView(lblIntellisense.SelectedItem);
			}
		}

		internal int ItemsCount
		{
			get { return lblIntellisense.Items.Count; }
		}

		protected virtual void OnListBoxItemDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ListBoxItemDoubleClick?.Invoke(sender, e);
		}

		internal event ListBoxKeyDownEventHandler ListBoxKeyDown;
		internal delegate void ListBoxKeyDownEventHandler(object sender, KeyEventArgs e);
		internal event ListBoxItemDoubleClickEventHandler ListBoxItemDoubleClick;
		internal delegate void ListBoxItemDoubleClickEventHandler(object sender, MouseButtonEventArgs e);
	}
}
