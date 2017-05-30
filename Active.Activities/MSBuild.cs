using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.ComponentModel;
using Microsoft.Build.Evaluation;
using Active.Activities.Helpers;
using Microsoft.Build.Framework;

namespace Active.Activities
{
	public sealed class MSBuild : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public InArgument<string> CommandLineArguments { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public InArgument<string> Configuration { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public InArgument<string> OutDir { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public InArgument<string> Platform { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		[RequiredArgument]
		public InArgument<string> Project { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("(New List(Of String) From { \"Build\" })")]
		[RequiredArgument]
		public InArgument<IEnumerable<string>> Targets { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public OutArgument<string> BuildOutput { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("")]
		public OutArgument<bool> BuildSuccess { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			if (!string.IsNullOrEmpty(CommandLineArguments.Get(context)))
			{
				console.WriteLine("!!!Warning - This activity currently ignores 'CommandLineArguments' argument. !!!");
			}

			var project = new Project(Project.Get(context));
			StringOutputLogger logger = new StringOutputLogger();
			project.SetGlobalProperty("Configuration", Configuration.Get(context) ?? "");
			project.SetGlobalProperty("Platform", Platform.Get(context) ?? "");
			project.SetProperty("OutDir", OutDir.Get(context) ?? "");
			bool buildResult = project.Build(Targets.Get(context).ToArray(), new ILogger[] { logger });
			string buildOutput = string.Format(
				"MSBUILD - {0}\nConfiguration : {1}\nPlatform : {2}\nOutput Directory : {3}\n{4}", 
				project.FullPath, 
				project.GetProperty("Configuration").EvaluatedValue, 
				project.GetProperty("Platform").EvaluatedValue,
				project.GetProperty("OutDir").EvaluatedValue,
				logger.GetOutput());
			BuildOutput.Set(context, buildOutput);
			BuildSuccess.Set(context, buildResult);
			ProjectCollection.GlobalProjectCollection.UnloadProject(project);

			console.WriteLine(buildOutput);
		}
	}

}
