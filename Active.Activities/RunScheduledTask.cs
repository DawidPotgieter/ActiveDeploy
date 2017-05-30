using System.Activities;
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;

namespace Active.Activities
{
	public sealed class RunScheduledTask : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the task to create.")]
		[Category("Task")]
		public InArgument<string> TaskName { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			using (TaskService ts = new TaskService())
			{
				var task = ts.FindTask(TaskName.Get(context));
				task.Run();
			}
		}
	}
}
