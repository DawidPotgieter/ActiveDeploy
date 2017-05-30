using System;
using System.Activities.XamlIntegration;
using System.Activities;
using Active.Activities.Helpers;

namespace Active.Run
{
	public class WorkflowHelper
	{
		public static WorkflowApplication ExecuteWorkflow(string xamlFile, 
			Action<WorkflowApplicationCompletedEventArgs> workflowCompleted = null,
			Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> workflowUnhandledException = null,
			Action<WorkflowApplicationAbortedEventArgs> workflowAborted = null,
			bool debug = false)
		{
			var workflow = ActivityXamlServices.Load(xamlFile);
			return ExecuteWorkflow(
				workflow,
				workflowCompleted,
				workflowUnhandledException,
				workflowAborted,
				debug);
		}

		public static WorkflowApplication ExecuteWorkflow(Activity workflow,
			Action<WorkflowApplicationCompletedEventArgs> workflowCompleted = null,
			Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> workflowUnhandledException = null,
			Action<WorkflowApplicationAbortedEventArgs> workflowAborted = null,
			bool debug = false)
		{
			var console = new ActivityConsole();
			console.dataArrived += new EventHandler<ConsoleDataArrivedEventArgs>(console_dataArrived);
			var wa = new WorkflowApplication(workflow);
			wa.Extensions.Add(console);
			if (debug)
			{
				wa.Extensions.Add(new ConsoleTrackingParticipant());
			}
			wa.Completed = workflowCompleted;
			wa.OnUnhandledException = workflowUnhandledException;
			wa.Aborted = workflowAborted;
			wa.Run();
			return wa;
		}

		static void console_dataArrived(object sender, ConsoleDataArrivedEventArgs e)
		{
			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				ExecuteWindow executeWindow = (ExecuteWindow)App.Current.MainWindow;
				executeWindow.AddConsoleText(e.Data);
			}));
		}
	}
}
