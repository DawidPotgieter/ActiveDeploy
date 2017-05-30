using System;
using System.Activities;
using System.Diagnostics;
using System.ComponentModel;
using Active.Activities.Helpers;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(CreateEventLogDesigner))]
	public class CreateEventLog : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the event log to create or add the eventsource for (i.e. Application).  Will be created if it doesn't exist.")]
		[RequiredArgument]
		public InArgument<string> EventLogName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The source used to log events to the event log (i.e ASP.NET 4.0.30319.0).  Will be created if it doesn't exist.")]
		[RequiredArgument]
		public InArgument<string> EventLogSource { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will treat the existance of the eventlog and source as success. Defaults to true.")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> TreatExistAsSuccess { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will throw an exception if an error occurs. Defaults to true.")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ThrowOnError { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The target machine name.  Defaults to current machine.")]
		public InArgument<string> MachineName { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = ActivityConsole.GetDefaultOrNew(context);

			string machineName = MachineName.Get(context);
			machineName = (string.IsNullOrEmpty(machineName) ? "." : machineName);

			bool throwOnError = ThrowOnError.Get(context);
			bool treatExistsAsSuccess = TreatExistAsSuccess.Get(context);

			string errorMessage = "An unknown error has occurred.";
			bool created = CreateLogSource(machineName, EventLogName.Get(context), EventLogSource.Get(context), out errorMessage, treatExistsAsSuccess);

			if (created)
			{
				console.WriteLine(string.Format("Successfully created or verified event log and source : {0} - {1}", EventLogName.Get(context), EventLogSource.Get(context)));
			}
			else
			{
				if (throwOnError)
				{
					throw new ArgumentException(errorMessage);
				}
				else
				{
					console.WriteLine(string.Format("Error : {0}", errorMessage));
				}
			}
		}

		internal static bool CreateLogSource(string machineName, string eventLogName, string eventLogSource, out string errorMessage, bool treatExistsAsSuccess = false)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrEmpty(eventLogName))
			{
				errorMessage = "Event Log Name cannot be empty.";
				return false;
			}
			if (string.IsNullOrEmpty(eventLogSource))
			{
				errorMessage = "Log Source cannot be empty.";
				return false;
			}
			if (eventLogName == eventLogSource)
			{
				errorMessage = "The event log name and event source cannot be the same.";
				return false;
			}

			try
			{
				if (treatExistsAsSuccess &&
						EventLog.Exists(eventLogName, ".") &&
						EventLog.SourceExists(eventLogSource) &&
						(EventLog.LogNameFromSourceName(eventLogSource, ".") == eventLogName))
				{
					return true;
				}
				EventLog.CreateEventSource(eventLogSource, eventLogName);
				return true;
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}
		}
	}
}
