using System;
using System.Activities;
using System.ComponentModel;
using System.Data.SqlClient;
using Active.Activities.Helpers;

namespace Active.Activities
{
	public class CreateSqlLogin : CodeActivity
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
		[Description("The login name to create. Do not specify the domain/computer account as part of the login name.")]
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
		[Description("If this is a sql account, the password must be specified.")]
		[Category("Input")]
		public InArgument<string> Password { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Defaults to 'master'.")]
		[Category("Input")]
		public InArgument<string> DefaultDatabase { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			var console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			string sqlStatement =
				"DECLARE @Username nvarchar(max);\n" +
				"SET @Username = '{0}' + '{1}';\n" +
				"SET NOCOUNT ON;\n" +
				"DECLARE @SQL NVARCHAR(4000);\n" +
				"SET @SQL = 'CREATE LOGIN [' + @Username + '] {2} WITH {3} DEFAULT_DATABASE=[{4}]';\n" +
				"EXECUTE(@SQL);";

			string loginName = LoginName.Get(context);
			string message = string.Format("Successfully created (or exists) SQL Server user login '{0}'.", loginName);
			string defaultDatabase = DefaultDatabase.Get(context);
			if (string.IsNullOrEmpty(defaultDatabase))
			{
				defaultDatabase = "master";
			}

			if (IsWindowsLogin.Get(context))
			{
				if (IsLocalSQLMachineAccount.Get(context))
				{
					sqlStatement = string.Format(sqlStatement, "HOST_NAME()", "\\" + loginName, "FROM WINDOWS", "", defaultDatabase);
				}
				else
				{
					if (string.IsNullOrEmpty(DomainName.Get(context)))
					{
						throw new ArgumentException("Domain name must be specified for windows domain logins.");
					}
					sqlStatement = string.Format(sqlStatement, "'" + DomainName.Get(context) + "'", "\\" + loginName, "FROM WINDOWS", "", defaultDatabase);
				}
			}
			else
			{
				string password = Password.Get(context);
				sqlStatement = string.Format(sqlStatement, "", loginName, "", "PASSWORD=''" + password + "'',", defaultDatabase);
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
						if (ex.Number != 15025) //Already exists, ignore error
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
