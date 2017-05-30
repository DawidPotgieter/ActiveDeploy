namespace Microsoft.Web.Administration.Wrapper
{
	public class VirtualDirectory : NamedObject
	{
		private Administration.VirtualDirectory virtualDirectory;

		internal VirtualDirectory(Administration.VirtualDirectory virtualDirectory, string name)
			:base(name)
		{
			this.virtualDirectory = virtualDirectory;
		}

		public AuthenticationLogonMethod LogonMethod { get { return (AuthenticationLogonMethod)virtualDirectory.LogonMethod; } }
		public string Path { get { return virtualDirectory.Path; } }
		public string PhysicalPath { get { return virtualDirectory.PhysicalPath; } }
	}
}
