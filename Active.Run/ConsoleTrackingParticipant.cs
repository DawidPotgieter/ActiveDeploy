using System;
using System.Activities.Tracking;

namespace Active.Run
{
	public class ConsoleTrackingParticipant : TrackingParticipant
	{
		protected override void Track(TrackingRecord record, TimeSpan timeout)
		{
			ActivityStateRecord activityRecord = record as ActivityStateRecord;
			if (activityRecord != null)
			{
				WriteToConsole("DEBUG -- " + activityRecord.Activity.Name + " : " + activityRecord.State + " --");
			}
		}

		private void WriteToConsole(object param)
		{
			App.Current.Dispatcher.BeginInvoke((Action)(() => 
			{
				ExecuteWindow executeWindow = (ExecuteWindow)App.Current.MainWindow;
			  executeWindow.AddConsoleText((string)param);
			}));
		}
	}
}
