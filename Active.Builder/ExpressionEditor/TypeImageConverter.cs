using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Active.Builder.ExpressionEditor
{
	public class TypeImageConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is TreeNodes.NodeTypes))
				return null;

			DrawingBrush result = null;
			ImageSource resImage = null;
			TreeNodes.NodeTypes inputValue = (TreeNodes.NodeTypes)Enum.Parse(typeof(TreeNodes.NodeTypes), value.ToString());
			switch (inputValue)
			{
				case TreeNodes.NodeTypes.Namespace:
					resImage = (ImageSource)Application.Current.Resources["ISNamespace"];
					break;
				case TreeNodes.NodeTypes.Interface:
					resImage = (ImageSource)Application.Current.Resources["ISInterface"];
					break;
				case TreeNodes.NodeTypes.Class:
					resImage = (ImageSource)Application.Current.Resources["ISClass"];
					break;
				case TreeNodes.NodeTypes.Method:
					resImage = (ImageSource)Application.Current.Resources["ISMethod"];
					break;
				case TreeNodes.NodeTypes.Property:
					resImage = (ImageSource)Application.Current.Resources["ISProperty"];
					break;
				case TreeNodes.NodeTypes.Field:
					resImage = (ImageSource)Application.Current.Resources["ISField"];
					break;
				case TreeNodes.NodeTypes.Enum:
					resImage = (ImageSource)Application.Current.Resources["ISEnum"];
					break;
				case TreeNodes.NodeTypes.ValueType:
					resImage = (ImageSource)Application.Current.Resources["ISStructure"];
					break;
				case TreeNodes.NodeTypes.Event:
					resImage = (ImageSource)Application.Current.Resources["ISEvent"];
					break;
				case TreeNodes.NodeTypes.Primitive:
					break;
				default:
					break;
			}

			if (resImage != null)
			{
				result = new DrawingBrush { Drawing = new ImageDrawing(resImage, new Rect(0, 0, 16, 16)) };
			}
			return result;
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}