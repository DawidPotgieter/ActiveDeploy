using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;

namespace Active.Activities
{
	[Description("Writes all the details of the provided exception to the ActivityConsole.")]
	public sealed class WriteExceptionTrace : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The exception that will be written to the console and Output variable.")]
		[RequiredArgument]
		public InArgument<Exception> Exception { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Contains the full exception trace generated from the exception.")]
		public OutArgument<string> Output { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			if (Exception.Get(context) != null)
			{
				string exceptionMessage = ExceptionManager.GetExceptionMessage(Exception.Get(context));
				Output.Set(context, exceptionMessage);
				console.WriteLine(exceptionMessage);
			}
		}
	}
}
