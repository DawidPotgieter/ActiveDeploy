using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using System.IO;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(VsdbcmdDesigner))]
	public class Vsdbcmd : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The path where vsdbcmd.exe is located. Defaults to current folder.")]
		[Category("Process")]
		public InArgument<string> VsdbcmdPath { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The connectionstring to the database. Does not need to include the database name.")]
		[Category("Arguments")]
		public InArgument<string> ConnectionString { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("This is the name of .manifest file for the deployment package.")]
		[Category("Arguments")]
		public InArgument<string> ManifestFilename { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("This is the name of the database that will be deployed to.")]
		[Category("Arguments")]
		public InArgument<string> TargetDatabase { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Set to true to force a full backup before deploying.")]
		[Category("Arguments")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> BackupBeforeDeploy { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Set to true to force the database to be re-created.  Caution, setting this to true will overwrite all your dataz.")]
		[Category("Arguments")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> AlwaysCreateNewDatabase { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string backupArguments = string.Format("/p:PerformDatabaseBackup={0}", BackupBeforeDeploy.Get(context));
			string alwaysCreateNewArguments = string.Format("/p:AlwaysCreateNewDatabase={0}", AlwaysCreateNewDatabase.Get(context));
			string vsdbcmdArguments = "/a:Deploy /dd+ /cs:\"{0}\" /p:TargetDatabase={1}  \"/manifest:{2}\" {3} {4}";
			string output = string.Empty;
			string vsdbcmd = VsdbcmdPath.Get(context);

			if (string.IsNullOrEmpty(vsdbcmd))
			{
				vsdbcmd = ".\\";
			}

			if (vsdbcmd.EndsWith("vsdbcmd.exe", StringComparison.OrdinalIgnoreCase))
			{
				vsdbcmd = vsdbcmd.Substring(0, vsdbcmd.Length - "vsdbcmd.exe".Length);
			}

			vsdbcmd = Path.Combine(vsdbcmd, "vsdbcmd.exe");

			if (!File.Exists(vsdbcmd))
				throw new ArgumentException(string.Format("Vsdbcmd missing : The file '{0}' could not be found.", vsdbcmd));

			vsdbcmdArguments = string.Format(vsdbcmdArguments,
																			 ConnectionString.Get(context),
																			 TargetDatabase.Get(context),
																			 ManifestFilename.Get(context),
																			 (BackupBeforeDeploy.Get(context) ? backupArguments : ""),
																			 (AlwaysCreateNewDatabase.Get(context) ? alwaysCreateNewArguments : ""));

			console.WriteLine("Executing Vsdbcmd.exe..." + Environment.NewLine);

			CommandLine commandLineHelper = new CommandLine();
			commandLineHelper.ReportProgress += new EventHandler<CommandLineProgressEventArgs>(commandLineHelper_ReportProgress);
			int exitCode = commandLineHelper.Execute(vsdbcmd, vsdbcmdArguments, out output);
			if (exitCode != 0)
			{
				throw new InvalidOperationException(string.Format("Vsdbcmd returned a exit code : '{0}'.", exitCode));
			}
		}

		void commandLineHelper_ReportProgress(object sender, CommandLineProgressEventArgs e)
		{
			console.WriteLine(e.Output);
		}
	}
}
