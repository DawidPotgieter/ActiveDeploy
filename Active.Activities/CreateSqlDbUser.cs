using System;
using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using System.Data.SqlClient;

namespace Active.Activities
{
	public class CreateSqlDbUser : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The database connection string to use.")]
		[Category("Input")]
		public InArgument<string> ConnectionString { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The login name to create a user for. The db username will be the same as the login name.")]
		[Category("Input")]
		public InArgument<string> LoginName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If the login is a windows login, the domain/computer will also be used. Defaults to false.")]
		[Category("Input")]
		public InArgument<bool> IsWindowsLogin { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Set to true if this is a windows account on the local sql machine. Defaults to false.")]
		[Category("Input")]
		public InArgument<bool> IsLocalSQLMachineAccount { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If this is a windows domain account, the domain name must be specified.")]
		[Category("Input")]
		public InArgument<string> DomainName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Defaults to 'dbo'.")]
		[Category("Input")]
		public InArgument<string> DefaultSchema { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			var console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string sqlStatement =
				"USE [{0}]\n" +
				"DECLARE @Username nvarchar(max);\n" +
				"SET @Username = '{1}' + '{2}';\n" +
				"SET NOCOUNT ON;\n" +
				"DECLARE @SQL NVARCHAR(4000);\n" +
				"SET @SQL = 'CREATE USER [' + @Username + '] FOR LOGIN [' + @Username + '] WITH DEFAULT_SCHEMA=[{3}]';\n" +
				"EXECUTE(@SQL);";

			SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString.Get(context));

			string databaseName = connectionStringBuilder.InitialCatalog;
			string loginName = LoginName.Get(context);
			string message = string.Format("Successfully created (or exists) SQL Server database user account '{0}'.", loginName);
			string defaultSchema = DefaultSchema.Get(context);
			if (string.IsNullOrEmpty(defaultSchema))
			{
				defaultSchema = "dbo";
			}

			if (IsWindowsLogin.Get(context))
			{
				if (IsLocalSQLMachineAccount.Get(context))
				{
					sqlStatement = string.Format(sqlStatement, databaseName, "HOST_NAME()", "\\" + loginName, defaultSchema);
				}
				else
				{
					if (string.IsNullOrEmpty(DomainName.Get(context)))
					{
						throw new ArgumentException("Domain name must be specified for windows domain logins.");
					}
					sqlStatement = string.Format(sqlStatement, databaseName, "'" + DomainName.Get(context) + "'", "\\" + loginName, defaultSchema);
				}
			}
			else
			{
				sqlStatement = string.Format(sqlStatement, databaseName,  "", loginName, defaultSchema);
			}

			using (SqlConnection connection = new SqlConnection(ConnectionString.Get(context)))
			{
				using (SqlCommand command = new SqlCommand(sqlStatement, connection))
				{
					connection.Open();
					try
					{
						command.ExecuteNonQuery();
					}
					catch (SqlException ex)
					{
						if (ex.Number != 15023) //Already exists, ignore this error.
						{
							throw;
						}
					}
					console.WriteLine(message);
				}
			}
		}
	}
}
