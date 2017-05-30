using System.Activities;
using System.ComponentModel;
using System.Configuration;

namespace Active.Activities
{
	public class ReadAppConnectionString : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the connection string of the current running application to read.")]
		[Category("Input")]
		public InArgument<string> ConnectionStringName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The value of the specified connection string entry.")]
		[Category("Output")]
		public OutArgument<string> Value { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			Value.Set(context, ConfigurationManager.ConnectionStrings[ConnectionStringName.Get(context)].ConnectionString);
		}
	}
}
