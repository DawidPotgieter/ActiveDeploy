using System.Xml.Linq;
using System.Xml;

namespace Active.Activities.XamlProviders
{
	public class XamlFileProviderFactory
	{
		public static IXamlFileProvider GetNewXamlFileProvider(bool isProtected)
		{
			if (isProtected) return new EncryptedXamlFileProvider();
			return new DefaultXamlFileProvider();
		}

		public static IXamlFileProvider GetXamlFileProvider(string filename, string password = null)
		{
			IXamlFileProvider provider;
			if (!string.IsNullOrEmpty(password))
			{
				provider = new EncryptedXamlFileProvider();
			}
			else
			{
				provider = new DefaultXamlFileProvider();
			}
			return provider;
		}

		public static bool IsXamlFileEncrypted(string filename)
		{
			try
			{
				using (XmlReader reader = XmlReader.Create(filename))
				{
					XDocument xDocument = XDocument.Load(reader);
					if (xDocument.Root.Name == "Encrypted")
					{
						return true;
					}
				}
			}
			catch { }
			return false;
		}
	}
}
