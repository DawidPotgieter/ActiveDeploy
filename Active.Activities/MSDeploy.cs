using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using Active.Activities.ActivityDesigners;
using System.IO;

namespace Active.Activities
{
	public enum MSDeployExecutionType
	{
		Exe,
		Cmd,
	}
	[Designer(typeof(MSDeployDesigner))]
	public sealed class MSDeploy : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(false)]
		[Description("If set to true, MSDeploy.exe will be called directly with package and param files.  If set to false, the generated cmd file will be used.")]
		[Category("Process")]
		[DefaultValue(MSDeployExecutionType.Cmd)] //NB : This only works because it's manually read in the designer
		public InArgument<MSDeployExecutionType> ExecutionType { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name and path of the zip package file, only used when ExecutionType = Exe.")]
		[Category("ExeFile")]
		public InArgument<string> ZipPackageFilename { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name and path of the params file for the package, only used when ExecutionType = Exe.")]
		[Category("ExeFile")]
		public InArgument<string> ParamFilename { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name and path of the cmd file generated for the package, only used when ExecutionType = Cmd.")]
		[Category("CmdFile")]
		public InArgument<string> CmdFileName { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(false)]
		[Description("If set to true, will try to use MSDeploy version specified.")]
		[Category("ExeFile")]
		[DefaultValue(1)] //NB : This only works because it's manually read in the designer
		public InArgument<int> UseVersion { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("[deprecated] If set to true, a workaround will be used to allow the vs2010 generated cmd file to work with version 2.x of msdeploy. Note that this setting overrides UseVersion if set to True.")]
		[Category("CmdFile")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		[Obsolete("This is used for backwards compatibility, use UseVersion instead.")]
		public InArgument<bool> UseVersion2x { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the IIS site/virtual directory to deploy to.  To deploy to the root of a site, use 'demosite.com/'.")]
		[Category("Deploy")]
		public InArgument<string> IISWebApplication { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, files that were added manually to the site will not be deleted. By default, the deploy is a clean/re-deploy of the whole folder structure.")]
		[Category("Deploy")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> DoNotDeleteFiles { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, the deployment is allowed over an untrusted SSL connection.")]
		[Category("Deploy")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> AllowUntrustedSSLConnection { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will run with -whatif flag which doesn't do the sync.")]
		[Category("Deploy")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> Test { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If the deployment is to a remove server, specify the remote machine name here.  Leave blank if deploying locally.")]
		[Category("Deploy")]
		public InArgument<string> ComputerName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If the deployment is to a remove server, specify the remote username here.  Leave blank if deploying locally.")]
		[Category("Deploy")]
		public InArgument<string> Username { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If the deployment is to a remove server, specify the remote user account password here.  Leave blank if deploying locally.")]
		[Category("Deploy")]
		public InArgument<string> Password { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Use this to manually specify command line params to pass to the .cmd file. i.e. -skip:objectName=dirPath,absolutePath=_vti_bin")]
		[Category("Deploy")]
		public InArgument<string> CommandLineParams { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			int version = UseVersion.Get(context);

			if (UseVersion2x.Get(context))
			{
				UseVersion.Set(context, 2);
			}

			string platformKey = "InstallPath";
			string keyName = string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\IIS Extensions\MSDeploy\{0}", version);
			string msdeployExePath = (string)Microsoft.Win32.Registry.GetValue(keyName, platformKey, "");
			if (string.IsNullOrEmpty(msdeployExePath))
			{
				throw new ArgumentException(string.Format("Could not find msdeploy.exe for version '{0}'.", version));
			}
			msdeployExePath = string.Format("\"{0}\"", System.IO.Path.Combine(msdeployExePath, "msdeploy.exe"));

			if (version == 2 && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSDeployPath")))
			{
				Environment.SetEnvironmentVariable("MSDeployPath", msdeployExePath);
			}

			string executablePath;
			string arguments;
			if (ExecutionType.Get(context) == MSDeployExecutionType.Exe)
			{
				executablePath = msdeployExePath;
				arguments = GetExeFileCommandLineArguments(context);
			}
			else
			{
				executablePath = CmdFileName.Get(context);
				arguments = GetCmdFileCommandLineArguments(context);
			}

			string output = string.Empty;

			CommandLine commandLineHelper = new CommandLine();
			commandLineHelper.ReportProgress += new EventHandler<CommandLineProgressEventArgs>(commandLineHelper_ReportProgress);
			console.WriteLine(string.Format("{0} {1}\r\n", executablePath, arguments));
			int returnValue = commandLineHelper.Execute(executablePath, arguments, out output);

			if (output.Contains("All arguments must begin with \"-\"") && ExecutionType.Get(context) == MSDeployExecutionType.Cmd && !string.IsNullOrEmpty(IISWebApplication.Get(context)))
			{
				console.WriteLine("\n");
				console.WriteLine("**********************************************************************************************************************************************\n");
				console.WriteLine("There is a bug with 2012 versions of the .cmd files generated, which does not allow '=' to be passed on the command line.  Try the Exe method.\n");
				console.WriteLine("**********************************************************************************************************************************************\n\n");
			}

			if (output.Contains("Attempted to perform an unauthorized operation.") || output.Contains("ERROR_USER_UNAUTHORIZED") || output.Contains("ERROR_INSUFFICIENT_ACCESS_TO_SITE_FOLDER"))
			{
				console.WriteLine("\n");
				console.WriteLine("***********************************************************************************************************************\n");
				console.WriteLine("It seems the user account is not allowed to do this.  Try running as administrator or specifying username and password.\n");
				console.WriteLine("***********************************************************************************************************************\n\n");
			}

			if (output.Contains("ERROR_CERTIFICATE_VALIDATION_FAILED"))
			{
				console.WriteLine("\n");
				console.WriteLine("**********************************************************************************************************\n");
				console.WriteLine("The SSL certificate is self signed and untrusted. Tick the 'Allow Untrusted Connection' box and try again. \n");
				console.WriteLine("**********************************************************************************************************\n\n");
			}

			if (returnValue != 0 || (!output.Contains("Total changes:")))
			{
				throw new InvalidOperationException(output);
			}
		}

		private string GetCmdFileCommandLineArguments(CodeActivityContext context)
		{
			string doNotDeleteArgument = "-enableRule:DoNotDeleteRule";
			string computerNameArgument = string.Format("/M:{0}", ComputerName.Get(context));
			string userNameArgument = string.Format("/U:{0}", Username.Get(context));
			string passwordArgument = string.Format("/P:{0}", Password.Get(context));
			string webApplicationArgument = string.Format("\"-setParam:'IIS Web Application Name'='{0}'\"", IISWebApplication.Get(context));

			string msdeployArguments = (Test.Get(context) ? "/t" : "/y");
			msdeployArguments += " {0} {1} {2} {3} {4} {5} {6}";

			msdeployArguments = string.Format(msdeployArguments,
																				(!string.IsNullOrEmpty(IISWebApplication.Get(context)) ? webApplicationArgument : ""),
																				(DoNotDeleteFiles.Get(context) ? doNotDeleteArgument : ""),
																				(!string.IsNullOrEmpty(ComputerName.Get(context)) ? computerNameArgument : ""),
																				(!string.IsNullOrEmpty(Username.Get(context)) ? userNameArgument : ""),
																				(!string.IsNullOrEmpty(Password.Get(context)) ? passwordArgument : ""),
																				(AllowUntrustedSSLConnection.Get(context) ? "-allowUntrusted" : ""),
																				(!string.IsNullOrEmpty(CommandLineParams.Get(context)) ? CommandLineParams.Get(context) : "")).Trim();

			return msdeployArguments;
		}

		private string GetExeFileCommandLineArguments(CodeActivityContext context)
		{
			string doNotDeleteArgument = "-enableRule:DoNotDeleteRule";
			string computerNameArgument = string.Format("computerName='{0}',", ComputerName.Get(context));
			string userNameArgument = string.Format("userName='{0}',", Username.Get(context));
			string passwordArgument = string.Format("password='{0}',", Password.Get(context));
			string webApplicationArgument = string.Format("-setParam:'IIS Web Application Name'='{0}'", IISWebApplication.Get(context));

			string msdeployArguments = (Test.Get(context) ? "-whatif" : "");
			msdeployArguments += " -source:package='{0}' -dest:auto,{1}{2}{3}authtype='Basic',includeAcls='False' -verb:sync -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension -setParamFile:\"{4}\" {5} {6} {7} {8}";

			msdeployArguments = string.Format(msdeployArguments,
																				Path.GetFullPath(ZipPackageFilename.Get(context)),
																				(string.IsNullOrEmpty(ComputerName.Get(context)) ? "" : computerNameArgument),
																				(string.IsNullOrEmpty(Username.Get(context)) ? "" : userNameArgument),
																				(string.IsNullOrEmpty(Password.Get(context)) ? "" : passwordArgument),
																				Path.GetFullPath(ParamFilename.Get(context)),
																				(string.IsNullOrEmpty(IISWebApplication.Get(context)) ? "" : webApplicationArgument),
																				(DoNotDeleteFiles.Get(context) ? doNotDeleteArgument : ""),
																				(AllowUntrustedSSLConnection.Get(context) ? "-allowUntrusted" : ""),
																				(!string.IsNullOrEmpty(CommandLineParams.Get(context)) ? CommandLineParams.Get(context) : "")).Trim();

			return msdeployArguments;
		}

		void commandLineHelper_ReportProgress(object sender, CommandLineProgressEventArgs e)
		{
			console.WriteLine(e.Output);
		}
	}
}
