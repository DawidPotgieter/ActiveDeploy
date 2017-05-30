using System.Collections.Generic;

namespace Microsoft.Web.Administration.Wrapper
{
	public class Application : NamedObject
	{
		private Administration.Application application;

		internal Application(Administration.Application application, string name)
			: base(name)
		{
			this.application = application;
		}

		public string ApplicationPoolName { get { return application.ApplicationPoolName; } }
		public string EnabledProtocols { get { return application.EnabledProtocols; } }
		public string Path { get { return application.Path; } }

		private List<VirtualDirectory> virtualDirectories = null;
		public ReadOnlyStringIndexedCollection<VirtualDirectory> VirtualDirectories
		{
			get
			{
				if (virtualDirectories == null)
				{
					virtualDirectories = new List<VirtualDirectory>();
					foreach (Administration.VirtualDirectory virtualDirectory in application.VirtualDirectories)
					{
						virtualDirectories.Add(new VirtualDirectory(virtualDirectory, virtualDirectory.Path));
					}
				}
				return new ReadOnlyStringIndexedCollection<VirtualDirectory>(virtualDirectories);
			}
		}
	}
}
