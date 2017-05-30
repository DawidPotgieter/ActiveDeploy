using System;
using System.Activities;
using System.ComponentModel;
using System.ServiceProcess;

namespace Active.Activities
{
	public sealed class StartService : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the service to start.")]
		[Category("Service")]
		[RequiredArgument]
		public InArgument<string> Name { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the machine on which to start the service. Use a dot '.' or leave empty for local machine.")]
		[Category("Service")]
		public InArgument<string> MachineName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The time to wait for the service to start.")]
		[Category("Service")]
		public InArgument<TimeSpan> Timeout { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			TimeSpan timeOut = Timeout.Get(context);
			if (timeOut.TotalSeconds <= 0) timeOut = new TimeSpan(0, 0, 30);
			try
			{
				ServiceController serviceController = new ServiceController(Name.Get(context), (string.IsNullOrEmpty(MachineName.Get(context)) ? "." : MachineName.Get(context)));
				switch (serviceController.Status)
				{
					case ServiceControllerStatus.Stopped:
					case ServiceControllerStatus.Paused:
						serviceController.Start();
						serviceController.WaitForStatus(ServiceControllerStatus.Running, timeOut);
						break;
				}
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null && ex.InnerException.Message == "An instance of the service is already running")
				{
					return;
				}
				throw;
			}
		}
	}
}
