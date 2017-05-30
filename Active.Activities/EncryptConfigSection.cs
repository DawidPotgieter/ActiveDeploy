using System;
using System.Activities;
using System.ComponentModel;
using System.Configuration;
using System.Web.Configuration;
using System.IO;
using Active.Activities.ActivityDesigners;
using Active.Activities.Helpers;

namespace Active.Activities
{
	[Designer(typeof(EncryptConfigSectionDesigner))]
	public class EncryptConfigSection : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path and filename of the config file to encrypt sections in.")]
		[Category("Config")]
		public InArgument<string> ConfigFilename { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The section name to encrypt.")]
		[Category("Config")]
		public InArgument<string> Section { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Set this to true if the config file is web.config (website) or leave empty/set to false for exe.")]
		[Category("Config")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> IsWebsite { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("This is the user account that will be used to access the specified encryption string." + 
			"\nIf specified, the user account will be given permission to access the RSA keys on the current machine." +
			"\nIf this is not done at least once, the account will not be able to decrypt the sections." + 
			"\nYou can use 'aspnet_regiis -pa \"NetFrameworkConfigurationKey\" \"Domain\\Username\"' to set this manually.")]
		[Category("Security")]
		public InArgument<string> Username { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			Configuration config = null;
			if (IsWebsite.Get(context))
			{
				var configFile = new FileInfo(ConfigFilename.Get(context));
				var vdm = new VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
				var wcfm = new WebConfigurationFileMap();
				wcfm.VirtualDirectories.Add("/", vdm);
				config = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
			}
			else
			{
				config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = ConfigFilename.Get(context) }, ConfigurationUserLevel.None);
			}

			ConfigurationSection section = config.GetSection(Section.Get(context));
			section.SectionInformation.ProtectSection("RSAProtectedConfigurationProvider");
			config.Save();

			var console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			console.WriteLine(string.Format("Encrypted section '{0}' in '{1}'", Section.Get(context), ConfigFilename.Get(context)));
			console.WriteLine("");

			string username = Username.Get(context);
			if (!string.IsNullOrEmpty(username))
			{
				Helpers.CommandLine cmdExecutor = new CommandLine();
				string output = string.Empty;
				int exitCode = cmdExecutor.Execute(string.Format("{0}\\{1}", Helpers.FrameworkHelper.GetDotNetInstallationFolder(), "aspnet_regiis"), string.Format("-pa \"NetFrameworkConfigurationKey\" \"{0}\"", username), out output);
				if (exitCode != 0 || !output.Contains("Succeeded!"))
				{
					throw new ArgumentException(string.Format("Failed to set Encryption key security privileges for user '{0}'\n{1}", username, output));
				}
				console.WriteLine(string.Format("Successfully set read access to Encryption key for user '{0}'.", username));
			}
		}
	}
}
