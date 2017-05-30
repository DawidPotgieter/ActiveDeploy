using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Active.Activities.Helpers
{
	public class FrameworkHelper
	{
		[Flags]
		enum RuntimeInfo
		{
			UPGRADE_VERSION = 0x01,
			REQUEST_IA64 = 0x02,
			REQUEST_AMD64 = 0x04,
			REQUEST_X86 = 0x08,
			DONT_RETURN_DIRECTORY = 0x10,
			DONT_RETURN_VERSION = 0x20,
			DONT_SHOW_ERROR_DIALOG = 0x40
		}

		[DllImport("mscoree.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern int GetRequestedRuntimeInfo(
				string pExe,
				string pwszVersion,
				string pConfigurationFile,
				uint startupFlags,
				RuntimeInfo runtimeInfoFlags,
				StringBuilder pDirectory,
				uint dwDirectory,
				out uint dwDirectoryLength,
				StringBuilder pVersion,
				uint cchBuffer,
				out uint dwLength);

		public static string GetDotNetInstallationFolder()
		{
			Version env_version = Environment.Version;
			StringBuilder directory = new StringBuilder(0x200);
			StringBuilder version = new StringBuilder(0x20);
			uint directoryLength;
			uint versionLength;
			int hr = GetRequestedRuntimeInfo(
					null,
					"v" + env_version.ToString(3),
					null,
					0,
					RuntimeInfo.DONT_SHOW_ERROR_DIALOG | RuntimeInfo.UPGRADE_VERSION,
					directory,
					(uint)directory.Capacity,
					out directoryLength,
					version,
					(uint)version.Capacity,
					out versionLength);
			Marshal.ThrowExceptionForHR(hr);
			return Path.Combine(directory.ToString(), version.ToString());
		}
	}
}
