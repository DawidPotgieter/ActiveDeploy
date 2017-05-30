using System;
using System.Activities.XamlIntegration;
using System.Activities;
using System.IO;

namespace Active.Activities.XamlProviders
{
	public class DefaultXamlFileProvider : IXamlFileProvider
	{
		public void LoadXamlFile(string filename, string password = null)
		{
			try
			{
				var workflow = ActivityXamlServices.Load(filename);
				XamlDocument = workflow;
			}
			catch (Exception)
			{
				throw;
			}
		}


		public void WriteXamlFile(string filename, string data, string password = null)
		{
			try
			{
				File.WriteAllText(filename, data);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public Activity XamlDocument
		{
			get;
			private set;
		}
	}
}
