using System.Activities;
using System.ComponentModel;

namespace Active.Activities
{
	public class ReadRegistryValue : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The registry hive to query.")]
		[Category("Registry")]
		public InArgument<Microsoft.Win32.RegistryHive> Hive { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the key to query e.g 'SOFTWARE\\Microsoft\\Internet Explorer'")]
		[Category("Registry")]
		public InArgument<string> KeyPath { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the key to query e.g 'Build'")]
		[Category("Registry")]
		public InArgument<string> ValueName { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The default value that will be returned if the registry key is not found.")]
		[Category("Registry")]
		public InArgument<object> DefaultValue { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The registry value or the default value if it could not be read.")]
		[Category("Output")]
		public OutArgument<object> OutputValue { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			string hive = null;
			switch (Hive.Get(context))
			{
				case Microsoft.Win32.RegistryHive.ClassesRoot:
					hive = "HKEY_CLASSES_ROOT";
					break;
				case Microsoft.Win32.RegistryHive.CurrentConfig:
					hive = "HKEY_CURRENT_CONFIG";
					break;
				case Microsoft.Win32.RegistryHive.CurrentUser:
					hive = "HKEY_CURRENT_USER";
					break;
				case Microsoft.Win32.RegistryHive.DynData:
					hive = "HKEY_DYN_DATA";
					break;
				case Microsoft.Win32.RegistryHive.LocalMachine:
					hive = "HKEY_LOCAL_MACHINE";
					break;
				case Microsoft.Win32.RegistryHive.PerformanceData:
					hive = "HKEY_PERFORMANCE_DATA";
					break;
				case Microsoft.Win32.RegistryHive.Users:
					hive = "HKEY_USERS";
					break;
			}

			string keyName = string.Format("{0}\\{1}", hive, KeyPath.Get(context).TrimEnd('\\'));

			object returnValue = Microsoft.Win32.Registry.GetValue(keyName, ValueName.Get(context), DefaultValue.Get(context));

			OutputValue.Set(context, returnValue);
		}
	}
}
