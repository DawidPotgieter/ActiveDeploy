using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;

namespace Active.Activities
{
	[Description("Writes the specified message to the ActivityConsole.")]
	public sealed class WriteLine : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[RequiredArgument]
		public InArgument<string> Message { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string message = Message.Get(context);
			if (message != null)
			{
				console.WriteLine(message);
			}
		}
	}
}
