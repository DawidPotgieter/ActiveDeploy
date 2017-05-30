using Microsoft.Win32;

namespace Microsoft.Web.Administration.Wrapper
{
	public class IIS
	{
		private static int? majorVersion = null;
		public static int? MajorVersion
		{
			get
			{
				if (majorVersion == null)
				{
					using (RegistryKey iisKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp"))
					{
						majorVersion = (int)iisKey.GetValue("MajorVersion");
					}
				}

				return majorVersion;
			}
		}

		public static bool? IsIIS7
		{
			get
			{
				return (int)MajorVersion >= 7;
			}
		}
	}
}
