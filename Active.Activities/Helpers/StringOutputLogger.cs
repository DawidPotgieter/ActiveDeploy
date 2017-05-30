using System;
using System.Text;
using Microsoft.Build.Framework;

namespace Active.Activities.Helpers
{
	public class StringOutputLogger : ILogger
	{
		StringBuilder output = new StringBuilder();

		public void Initialize(IEventSource eventSource)
		{
			eventSource.AnyEventRaised += new AnyEventHandler(eventSource_AnyEventRaised);
			eventSource.BuildFinished += new BuildFinishedEventHandler(eventSource_BuildFinished);
			eventSource.BuildStarted += new BuildStartedEventHandler(eventSource_BuildStarted);
			eventSource.CustomEventRaised += new CustomBuildEventHandler(eventSource_CustomEventRaised);
			eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
			eventSource.MessageRaised += new BuildMessageEventHandler(eventSource_MessageRaised);
			eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
			eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
			eventSource.StatusEventRaised += new BuildStatusEventHandler(eventSource_StatusEventRaised);
			eventSource.TargetFinished += new TargetFinishedEventHandler(eventSource_TargetFinished);
			eventSource.TargetStarted += new TargetStartedEventHandler(eventSource_TargetStarted);
			eventSource.TaskFinished += new TaskFinishedEventHandler(eventSource_TaskFinished);
			eventSource.TaskStarted += new TaskStartedEventHandler(eventSource_TaskStarted);
			eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
		}

		void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
		{
			output.AppendFormat("Warning[{1}] - {0} Line {2} : {3}\n" + Environment.NewLine, e.File, e.Code, e.LineNumber, e.Message);
		}

		void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_TaskFinished(object sender, TaskFinishedEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_TargetStarted(object sender, TargetStartedEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_TargetFinished(object sender, TargetFinishedEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_StatusEventRaised(object sender, BuildStatusEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
		{
			output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
		{
			output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
		{
			output.AppendFormat("Error " + "[{1}]" + " - {0} Line {2} : {3}\n" + Environment.NewLine, e.File, e.Code, e.LineNumber, e.Message);
		}

		void eventSource_CustomEventRaised(object sender, CustomBuildEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_BuildStarted(object sender, BuildStartedEventArgs e)
		{
			output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_BuildFinished(object sender, BuildFinishedEventArgs e)
		{
			output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		void eventSource_AnyEventRaised(object sender, BuildEventArgs e)
		{
			if (Verbosity > LoggerVerbosity.Normal)
				output.AppendFormat("{0}" + Environment.NewLine, e.Message);
		}

		public string Parameters { get; set; }

		public void Shutdown()
		{
		}

		public LoggerVerbosity Verbosity { get; set; }

		public string GetOutput()
		{
			return output.ToString();
		}
	}
}
