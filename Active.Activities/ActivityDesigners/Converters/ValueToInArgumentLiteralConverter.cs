using System;
using System.Windows.Data;
using System.Globalization;
using System.Activities.Presentation.Model;
using System.Activities;


namespace Active.Activities.ActivityDesigners.Converters
{
	public class ValueToInArgumentLiteralConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var modelItem = value as ModelItem;
				if (modelItem != null)
				{
					var expr = modelItem.Properties["Expression"];
					if (expr != null)
					{
						return expr.Value;
					}
				}
			}
			
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;

			var type = typeof(InArgument<>);
			var genericArgument = type.MakeGenericType(value.GetType());
			var argument = Activator.CreateInstance(genericArgument, value);

			return argument;
		}
	}
}
