using System;
using System.Windows.Data;
using System.Activities.Presentation.Model;
using System.Activities;
using System.Activities.Expressions;

namespace Active.Activities.ActivityDesigners.Converters
{
	public class InArgumentBoolConverter : IValueConverter
	{
		public object Convert(
				object value,
				Type targetType,
				object parameter,
				System.Globalization.CultureInfo culture)
		{
			if (value is ModelItem)
			{
				if (((ModelItem)value).GetCurrentValue() is InArgument<bool>)
				{
					Activity<bool> expression = ((InArgument<bool>)((ModelItem)value).GetCurrentValue()).Expression;
					if (expression is Literal<bool>)
					{
						return ((Literal<bool>)expression).Value;
					}
				}
			}

			return null;
		}

		public object ConvertBack(
				object value,
				Type targetType,
				object parameter,
				System.Globalization.CultureInfo culture)
		{
			if (value is bool)
			{
				return new InArgument<bool>(new Literal<bool>((bool)value));
			}
			else
			{
				return null;
			}
		}
	}
}
