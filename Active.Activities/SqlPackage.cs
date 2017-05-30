using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using System.IO;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(SqlPackageDesigner))]
	public class SqlPackage : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description(@"The path where sqlpackage.exe is located. Defaults to 'C:\Program Files (x86)\Microsoft SQL Server\110\DAC\bin\SqlPackage.exe'.")]
		[Category("Process")]
		[DefaultValue(@"C:\Program Files (x86)\Microsoft SQL Server\110\DAC\bin\SqlPackage.exe")] //NB : This only works because it's manually read in the designer
		public InArgument<string> SqlPackagePath { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The connectionstring to the database. Must include the database name.")]
		[Category("Arguments")]
		public InArgument<string> ConnectionString { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("This is the name of .dacpac file for the deployment package.")]
		[Category("Arguments")]
		public InArgument<string> DacpacFilename { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Any additional command line params you want to pass through.  See 'http://msdn.microsoft.com/en-us/library/hh550080(v=VS.103).aspx' for more details.")]
		[Category("Arguments")]
		public InArgument<string> AdditionalArguments { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Set to true to force a full backup before deploying.")]
		[Category("Arguments")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> BackupBeforeDeploy { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Specifies whether the target database should be updated or whether it should be dropped and re-created when you publish to a database..  Caution, setting this to true will overwrite all your dataz.")]
		[Category("Arguments")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> AlwaysCreateNewDatabase { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will show the command line being executed. Defaults to 'false'.")]
		[Category("Process")]
		public InArgument<bool> ShowCommandLine { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string backupArguments = string.Format("/p:BackupDatabaseBeforeChanges={0}", BackupBeforeDeploy.Get(context));
			string alwaysCreateNewArguments = string.Format("/p:CreateNewDatabase={0}", AlwaysCreateNewDatabase.Get(context));
			string sqlPackageArguments = "/a:Publish /tcs:\"{0}\" /sf:\"{1}\" {2} {3} {4}";
			string output = string.Empty;
			string sqlPackagePath = SqlPackagePath.Get(context);

			if (string.IsNullOrEmpty(sqlPackagePath))
			{
				sqlPackagePath = @"C:\Program Files (x86)\Microsoft SQL Server\110\DAC\bin\SqlPackage.exe";
			}

			if (sqlPackagePath.EndsWith("SqlPackage.exe", StringComparison.OrdinalIgnoreCase))
			{
				sqlPackagePath = sqlPackagePath.Substring(0, sqlPackagePath.Length - "SqlPackage.exe".Length);
			}

			sqlPackagePath = Path.Combine(sqlPackagePath, "SqlPackage.exe");

			if (!File.Exists(sqlPackagePath))
				throw new ArgumentException(string.Format("SqlPackage missing : The file '{0}' could not be found.", sqlPackagePath));

			sqlPackageArguments = string.Format(sqlPackageArguments,
																			 ConnectionString.Get(context),
																			 DacpacFilename.Get(context),
																			 (BackupBeforeDeploy.Get(context) ? backupArguments : ""),
																			 (AlwaysCreateNewDatabase.Get(context) ? alwaysCreateNewArguments : ""),
																			 AdditionalArguments.Get(context));

			console.WriteLine("Executing SqlPackage.exe..." + Environment.NewLine);

			if (ShowCommandLine.Get(context))
			{
				console.WriteLine(string.Format("\"{0}\" {1}", sqlPackagePath, sqlPackageArguments));
			}

			CommandLine commandLineHelper = new CommandLine();
			commandLineHelper.ReportProgress += new EventHandler<CommandLineProgressEventArgs>(commandLineHelper_ReportProgress);
			int exitCode = commandLineHelper.Execute(sqlPackagePath, sqlPackageArguments, out output);
			if (exitCode != 0)
			{
				throw new InvalidOperationException(string.Format("SqlPackage returned a exit code : '{0}'.", exitCode));
			}
		}

		void commandLineHelper_ReportProgress(object sender, CommandLineProgressEventArgs e)
		{
			console.WriteLine(e.Output);
		}
	}
}
