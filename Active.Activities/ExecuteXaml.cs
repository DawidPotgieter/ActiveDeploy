using System;
using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using Active.Activities.XamlProviders;

namespace Active.Activities
{
	public class ExecuteXaml : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the xaml workflow file to execute.")]
		public InArgument<string> XamlFileName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The workflow input variables.(New Dictionary(Of String, Object) From {{ \"Var1\", \"val1\" }, { \"Var2\", 1 }})")]
		public InArgument<Dictionary<string, object>> Inputs { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Optional password for encrypted XAML file.")]
		public InArgument<string> Password { get; set; }

		private ActivityConsole console = null;
		private bool isBusy;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string xamlFile = XamlFileName.Get(context);
			Dictionary<string, object> inputs = Inputs.Get(context);

			try
			{
				Activity workflow = null;
				string xamlFileName = XamlFileName.Get(context);

				string password = null;
				string errorMessage = null;
				if (XamlFileProviderFactory.IsXamlFileEncrypted(xamlFileName))
				{
					password = Password.Get(context);
					if (string.IsNullOrEmpty(password))
					{
						errorMessage = string.Format("The file '{0}' is protected and requires the correct password to open.", xamlFileName);
					}
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					try
					{
						IXamlFileProvider provider = XamlFileProviderFactory.GetXamlFileProvider(xamlFileName, password);
						provider.LoadXamlFile(xamlFileName, password);
						workflow = provider.XamlDocument;
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						if (errorMessage == "The encrypted string was not in a valid format.")
							errorMessage = "The password you specified was incorrect.";
						errorMessage = string.Format("The following error occurred while trying to open '{0}' : \n\n{1}", xamlFileName, errorMessage);
					}
				}
				if (!string.IsNullOrEmpty(errorMessage))
				{
					throw new ArgumentException(errorMessage);
				}

				WorkflowApplication wa;
				if (inputs != null && inputs.Count > 0)
				{
					wa = new WorkflowApplication(workflow, Inputs.Get(context));
				}
				else
				{
					wa = new WorkflowApplication(workflow);
				}
				wa.Extensions.Add(console);
				wa.Completed = WorkflowCompleted;
				wa.OnUnhandledException = WorkflowUnhandledException;
				wa.Aborted = WorkflowAborted;
				isBusy = true;
				console.WriteLine(Environment.NewLine + xamlFile + " Executing..." + Environment.NewLine);
				wa.Run();
				while (isBusy)
				{
					System.Threading.Thread.Sleep(200);
				}
				console.WriteLine(Environment.NewLine + xamlFile + " Execution Complete." + Environment.NewLine);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
		{
			switch (e.CompletionState)
			{
				case ActivityInstanceState.Canceled:
					console.WriteLine(Environment.NewLine + "== Execution Cancelled ==");
					break;
				case ActivityInstanceState.Closed:
					//console.WriteLine(Environment.NewLine + "== Execution Complete ==");
					break;
				case ActivityInstanceState.Faulted:
					console.WriteLine(Environment.NewLine + "** Terminated **");
					console.WriteLine(Environment.NewLine + (e.TerminationException != null ? ExceptionManager.GetExceptionMessage(e.TerminationException) : string.Empty));
					break;
			}
			isBusy = false;
		}

		private UnhandledExceptionAction WorkflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
		{
			console.WriteLine(Environment.NewLine + "** Unhandled Exception **");
			console.WriteLine(Environment.NewLine + (e.UnhandledException != null ? ExceptionManager.GetExceptionMessage(e.UnhandledException) : string.Empty));
			isBusy = false;
			return UnhandledExceptionAction.Cancel;
		}

		private void WorkflowAborted(WorkflowApplicationAbortedEventArgs e)
		{
			console.WriteLine(Environment.NewLine + "** Aborted **");
			console.WriteLine(Environment.NewLine + (e.Reason != null ? ExceptionManager.GetExceptionMessage(e.Reason) : string.Empty));
			isBusy = false;
		}
	}
}
