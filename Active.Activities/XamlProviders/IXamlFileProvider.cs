using System.Activities;

namespace Active.Activities.XamlProviders
{
	public interface IXamlFileProvider
	{
		Activity XamlDocument { get; }
		void LoadXamlFile(string filename, string password = null);
		void WriteXamlFile(string filename, string data, string password = null);
	}
}
