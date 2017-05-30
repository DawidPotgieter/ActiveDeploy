using System;
using System.Activities;
using System.ComponentModel;
using System.ServiceProcess;
using System.Runtime.InteropServices;

namespace Active.Activities
{
	public sealed class SetServiceStartupMode : CodeActivity
	{
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern Boolean ChangeServiceConfig(
				IntPtr hService,
				UInt32 nServiceType,
				UInt32 nStartType,
				UInt32 nErrorControl,
				String lpBinaryPathName,
				String lpLoadOrderGroup,
				IntPtr lpdwTagId,
				[In] char[] lpDependencies,
				String lpServiceStartName,
				String lpPassword,
				String lpDisplayName);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr OpenService(
				IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

		[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenSCManager(
				string machineName, string databaseName, uint dwAccess);

		private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
		private const uint SERVICE_QUERY_CONFIG = 0x00000001;
		private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
		private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the service to manage.")]
		[Category("Service")]
		[RequiredArgument]
		public InArgument<string> Name { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the machine on which to manage the service. Use a dot '.' or leave empty for local machine.")]
		[Category("Service")]
		public InArgument<string> MachineName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the machine on which to manage the service. Use a dot '.' or leave empty for local machine.")]
		[Category("Service")]
		[RequiredArgument]
		public InArgument<ServiceStartMode> StartMode { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			try
			{
				ServiceController serviceController = new ServiceController(Name.Get(context), (string.IsNullOrEmpty(MachineName.Get(context)) ? "." : MachineName.Get(context)));
				ChangeStartMode(serviceController, StartMode.Get(context));
			}
			catch (Exception)
			{
				throw;
			}
		}

		private static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
		{
			var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
			if (scManagerHandle == IntPtr.Zero)
			{
				throw new ExternalException("Open Service Manager Error");
			}

			var serviceHandle = OpenService(
					scManagerHandle,
					svc.ServiceName,
					SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

			if (serviceHandle == IntPtr.Zero)
			{
				throw new ExternalException("Open Service Error");
			}

			var result = ChangeServiceConfig(
					serviceHandle,
					SERVICE_NO_CHANGE,
					(uint)mode,
					SERVICE_NO_CHANGE,
					null,
					null,
					IntPtr.Zero,
					null,
					null,
					null,
					null);

			if (result == false)
			{
				int nError = Marshal.GetLastWin32Error();
				var win32Exception = new Win32Exception(nError);
				throw new ExternalException("Could not change service start type: "
						+ win32Exception.Message);
			}
		}  
	}
}
