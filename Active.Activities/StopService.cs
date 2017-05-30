using System;
using System.Activities;
using System.ServiceProcess;
using System.ComponentModel;

namespace Active.Activities
{
	public sealed class StopService : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the service to stop.")]
		[Category("Service")]
		[RequiredArgument]
		public InArgument<string> Name { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the machine on which to stop the service. Use a dot '.' or leave empty for local machine.")]
		[Category("Service")]
		public InArgument<string> MachineName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The time to wait for the service to stop.")]
		[Category("Service")]
		public InArgument<TimeSpan> Timeout { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			TimeSpan timeOut = Timeout.Get(context);
			if (timeOut.TotalSeconds <= 0) timeOut = new TimeSpan(0, 0, 30);
			try
			{
				ServiceController serviceController = new ServiceController(Name.Get(context), (string.IsNullOrEmpty(MachineName.Get(context)) ? "." : MachineName.Get(context)));
				if (serviceController.Status == ServiceControllerStatus.Running)
				{
					serviceController.Stop();
					serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeOut);
				}
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null && ex.InnerException.Message == "The service has not been started")
				{
					return;
				}
				throw;
			}
		}
	}
}
