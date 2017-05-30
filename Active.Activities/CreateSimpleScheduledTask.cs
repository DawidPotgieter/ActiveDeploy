using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;
using System.Security.Principal;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(CreateSimpleScheduledTaskDesigner))]
	public sealed class CreateSimpleScheduledTask : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the executable file to execute when the schedule triggers.")]
		[Category("Process")]
		public InArgument<string> FileName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Optional arguments to pass on the command line.")]
		[Category("Process")]
		public InArgument<string> Arguments { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The working directory of the executable file.")]
		[Category("Process")]
		public InArgument<string> WorkingDirectory { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the task to create.")]
		[Category("Task")]
		public InArgument<string> TaskName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("A description of the task to create.")]
		[Category("Task")]
		public InArgument<string> TaskDescription { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account name to use when launching the tasks.  If not specified, the logged in account will be used.")]
		[Category("Task")]
		public InArgument<string> Username { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account password to use when launching the tasks.  If not specified, the logged in account will be used.")]
		[Category("Task")]
		public InArgument<string> Password { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description(
			"A list of triggers that sets when this task will execute. " +
			"\nExample : New List(Of Microsoft.Win32.TaskScheduler.Trigger) From { New Microsoft.Win32.TaskScheduler.DailyTrigger With {.DaysInterval = 2} }." +
			"\nSee http://taskscheduler.codeplex.com for more information.")]
		[Category("Task")]
		public InArgument<IList<Microsoft.Win32.TaskScheduler.Trigger>> Triggers { get; set; }

		/// <summary>
		/// See http://taskscheduler.codeplex.com/ for help on using the TaskService wrapper library.
		/// </summary>
		/// <param name="context"></param>
		protected override void Execute(CodeActivityContext context)
		{
			try
			{
				using (TaskService ts = new TaskService())
				{
					TaskDefinition td = ts.NewTask();
					td.RegistrationInfo.Description = TaskDescription.Get(context);
					var triggers = Triggers.Get(context);
					if (triggers != null) triggers.ToList().ForEach(t => td.Triggers.Add(t));
					td.Actions.Add(new ExecAction(FileName.Get(context), Arguments.Get(context), WorkingDirectory.Get(context)));

					string username = Username.Get(context);
					string password = Password.Get(context);
					if (string.IsNullOrEmpty(username)) username = WindowsIdentity.GetCurrent().Name;

					ts.RootFolder.RegisterTaskDefinition(
						TaskName.Get(context),
						td,
						TaskCreation.CreateOrUpdate,
						username,
						password);
				}
			}
			catch
			{
				throw;
			}
		}
	}
}
