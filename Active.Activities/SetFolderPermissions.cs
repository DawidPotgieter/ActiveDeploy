using System;
using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using System.Security.AccessControl;
using System.IO;
using Active.Activities.Helpers;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	/// <summary>
	/// This is a simple version, as the most often used permissions are read,write and execute.
	/// </summary>
	[Designer(typeof(SetFolderPermissionsDesigner))]
	public class SetFolderPermissions : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the folder to set permissions for.")]
		[RequiredArgument]
		public InArgument<string> FolderPath { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the user or group to set permissions for.  Note, this needs to include the domain (or current machine name) i.e. MACHINE\\Bob")]
		[RequiredArgument]
		public InArgument<string> UserOrGroup { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Read access to the file or folder.")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> Read { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Write access to the file or folder.")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> Write { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Execute access to the file or folder.")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ReadAndExecute { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Determines whether any output is sent to the ActivityConsole.")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ShowOutput { get; set; }

		private ActivityConsole console = null;
		private bool showOutput = false;

		private void WriteLineConsole(string text)
		{
			if (showOutput)
			{
				console.WriteLine(text);
			}
		}

		protected override void Execute(CodeActivityContext context)
		{
			console = ActivityConsole.GetDefaultOrNew(context);

			showOutput = ShowOutput.Get(context);
			string folder = FolderPath.Get(context);

			DirectoryInfo di = new DirectoryInfo(folder);
			if (!di.Exists)
			{
				throw new ArgumentException(string.Format("The folder '{0}' does not exist.", folder));
			}

			InheritanceFlags inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

			List<FileSystemAccessRule> accessRules = new List<FileSystemAccessRule>();
			accessRules.Add(new FileSystemAccessRule(UserOrGroup.Get(context), FileSystemRights.Read, inheritance, PropagationFlags.None, (Read.Get(context) ? AccessControlType.Allow : AccessControlType.Deny)));
			accessRules.Add(new FileSystemAccessRule(UserOrGroup.Get(context), FileSystemRights.Write, inheritance, PropagationFlags.None, (Write.Get(context) ? AccessControlType.Allow : AccessControlType.Deny)));
			accessRules.Add(new FileSystemAccessRule(UserOrGroup.Get(context), FileSystemRights.ExecuteFile, inheritance, PropagationFlags.None, (ReadAndExecute.Get(context) ? AccessControlType.Allow : AccessControlType.Deny)));

			DirectorySecurity ds = di.GetAccessControl();

			foreach (FileSystemAccessRule rule in accessRules)
			{
				ds.AddAccessRule(rule);
				WriteLineConsole(string.Format("Adding {0} {1} permission for identity {2} to folder {3}", rule.AccessControlType, rule.FileSystemRights, rule.IdentityReference.Value, folder));
			}

			di.SetAccessControl(ds);
		}
	}
}
