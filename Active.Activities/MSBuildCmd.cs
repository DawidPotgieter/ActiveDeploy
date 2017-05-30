using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(MSBuildCmdDesigner))]
	public sealed class MSBuildCmd : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Any additional MSBuild command line arguments.")]
		public InArgument<string> AdditionalCommandLineArguments { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Project/Solution configuration to build (i.e. Debug, Release)")]
		public InArgument<string> Configuration { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Platform to build (i.e. AnyCPU, x86, x64)")]
		public InArgument<string> Platform { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The project or solution to build.")]
		[RequiredArgument]
		public InArgument<string> Project { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Configuration target (i.e. Clean, Build, Package)")]
		public InArgument<string> Target { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string targetArgument = string.Format("/t:{0}", Target.Get(context));
			string platformArgument = string.Format("/p:platform={0}", Platform.Get(context));
			string configurationArgument = string.Format("/p:configuration={0}", Configuration.Get(context));
			string msBuildArguments = "\"{0}\" {1} {2} {3} {4}";
			string msBuildPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0", "MSBuildToolsPath", "") + @"\msbuild.exe";
			string output = string.Empty;

			msBuildArguments = string.Format(msBuildArguments,
																			 Project.Get(context),
																			 (!string.IsNullOrWhiteSpace(Target.Get(context)) ? targetArgument : ""),
																			 (!string.IsNullOrWhiteSpace(Platform.Get(context)) ? platformArgument : ""),
																			 (!string.IsNullOrWhiteSpace(Configuration.Get(context)) ? configurationArgument : ""),
																			 AdditionalCommandLineArguments.Get(context));

			console.WriteLine(string.Format("\"{0}\" {1}", msBuildPath, msBuildArguments) + Environment.NewLine);

			CommandLine commandLineHelper = new CommandLine();
			commandLineHelper.ReportProgress += new EventHandler<CommandLineProgressEventArgs>(commandLineHelper_ReportProgress);
			int exitCode = commandLineHelper.Execute(msBuildPath, msBuildArguments, out output);
			if (exitCode != 0)
			{
				throw new InvalidOperationException(string.Format("MsBuild.exe returned an exit code : '{0}'.", exitCode));
			}
		}

		void commandLineHelper_ReportProgress(object sender, CommandLineProgressEventArgs e)
		{
			console.WriteLine(e.Output);
		}
	}
}
