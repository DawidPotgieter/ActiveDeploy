using System.Activities;
using System.ComponentModel;
using System.Net.Mail;

namespace Active.Activities
{
	/// <summary>
	/// A simple SMTPMail wrapper activity.  The system settings can be specified in the app.config file
	/// of the executing process (i.e. app.config for Active.Deploy)
	/// </summary>
	public sealed class SimpleSendMail : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The email addresses to send to.  Multiple addresses must be seperated by comma ',' character.")]
		[Category("Recipient")]
		[RequiredArgument]
		public InArgument<string> To { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The email address to send from.")]
		[Category("Sender")]
		public InArgument<string> From { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The email subject.")]
		[Category("Content")]
		public InArgument<string> Subject { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The email body.")]
		[Category("Content")]
		public InArgument<string> Body { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Whether the email body should be sent as embedded html or not.")]
		[Category("Content")]
		public InArgument<bool> IsBodyHtml { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The smtp host name or address.")]
		[Category("System")]
		public InArgument<string> SmtpHost { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The smtp server port to use.")]
		[Category("System")]
		public InArgument<int?> SmtpPort { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If the smtp server requires authentication, the username can be specified.")]
		[Category("System")]
		public InArgument<string> Username { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Smtp server authentication password.  Ignored if Username is not set.")]
		[Category("System")]
		public InArgument<string> Password { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Determines whether the mail is sent via SSL or in clear text.")]
		[Category("System")]
		public InArgument<bool> EnableSsl { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, the email will be sent without blocking to wait for a response.")]
		[Category("System")]
		public InArgument<bool> SendAsync { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			SmtpClient smtpClient = new SmtpClient();
			if (!string.IsNullOrEmpty(SmtpHost.Get(context)))
			{
				smtpClient.Host = SmtpHost.Get(context);
			}
			if (SmtpPort.Get(context).HasValue)
			{
				smtpClient.Port = SmtpPort.Get(context).Value;
			}
			if (!string.IsNullOrEmpty(Username.Get(context)))
			{
				smtpClient.Credentials = new System.Net.NetworkCredential(Username.Get(context), Password.Get(context));
			}
			smtpClient.EnableSsl = EnableSsl.Get(context);

			MailMessage mailMessage = new MailMessage();
			mailMessage.To.Add(To.Get(context));
			mailMessage.From = new MailAddress(From.Get(context));
			mailMessage.Subject = Subject.Get(context);
			mailMessage.Body = Body.Get(context);
			mailMessage.IsBodyHtml = IsBodyHtml.Get(context);

			if (SendAsync.Get(context))
			{
				smtpClient.SendAsync(mailMessage, null);
			}
			else
			{
				smtpClient.Send(mailMessage);
			}
		}
	}
}
