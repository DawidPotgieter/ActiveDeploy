using System.Activities;
using System.ComponentModel;
using System.Data.SqlClient;

namespace Active.Activities
{
	public class ExecuteSqlNonQuery : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The SQL statement to execute.")]
		public InArgument<string> SQLStatement { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The database connection string to use.")]
		public InArgument<string> ConnectionString { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString.Get(context)))
			{
				using (SqlCommand command = new SqlCommand(SQLStatement.Get(context), connection))
				{
					connection.Open();
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
