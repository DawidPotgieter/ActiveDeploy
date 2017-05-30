using System;
using System.IO;
using System.Windows.Resources;
using System.Windows;
using Active.Activities.Helpers;

namespace Active.Activities.Resources
{
	public static class ResourceManager
	{
		public static Stream GetActivityDesignerIcon(string activityName)
		{
			try
			{
				StreamResourceInfo sri = Application.GetResourceStream(new Uri("pack://application:,,,/Active.Activities;component/Resources/" + activityName + ".png"));
				return sri.Stream;
			}
			catch
			{
				return null;
			}
		}
	}
}
