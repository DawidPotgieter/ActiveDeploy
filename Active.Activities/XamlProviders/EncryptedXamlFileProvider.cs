using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Active.Activities.XamlProviders
{
	public class EncryptedXamlFileProvider : IXamlFileProvider
	{
		public void LoadXamlFile(string filename, string password = null)
		{
			try
			{
				using (XmlReader xmlReader = XmlReader.Create(filename))
				{
					XDocument xDocument = XDocument.Load(xmlReader);
					if (xDocument.Root.Name != "Encrypted")
					{
						throw new ArgumentException("The file is not in the correct format. Expected 'Encrypted' as root node.");
					}
					var fileData = xDocument.Root.Value;
					var decryptedData = RijndaelAES.Decrypt(password, fileData, Encoding.CharacterEncoding.Base64, true);
					using (TextReader reader = new StringReader(decryptedData))
					{
						var workflow = ActivityXamlServices.Load(reader);
						XamlDocument = workflow;
					}
				}
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
				string encryptedData = RijndaelAES.Encrypt(password, data, Encoding.CharacterEncoding.Base64, true);
				XDocument xDocument = new XDocument();
				xDocument.AddFirst(new XElement("Encrypted", encryptedData));
				xDocument.Save(filename, SaveOptions.None);
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
