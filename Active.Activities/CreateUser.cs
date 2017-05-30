using System.Collections.Generic;
using System.Activities;
using System.DirectoryServices.AccountManagement;
using System.ComponentModel;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(CreateUserDesigner))]
	public sealed class CreateUser : CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Whether the account is local or a domain account. If set to false, will target the local computer and Domain and DomainContainer is ignored. If set to true, the domain and domain container must be specified.")]
		[Category("System")]
		public InArgument<bool> IsLocalAccount { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The domain to create this user account in. i.e. 'MyDomain'.")]
		[Category("System")]
		public InArgument<string> Domain { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The domain container. i.e. DC=MyDomain, DC=com")]
		[Category("System")]
		public InArgument<string> DomainContainer { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account username.")]
		[Category("User")]
		public InArgument<string> Username { get; set; }

		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user password.")]
		[Category("User")]
		public InArgument<string> Password { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account firstname.")]
		[Category("User")]
		public InArgument<string> Firstname { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account lastname.")]
		[Category("User")]
		public InArgument<string> Lastname { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The user account group memberships that are the end state.  Extra groups will be removed.(New List(Of String) From { \"Group\" })")]
		[Category("User")]
		public InArgument<IList<string>> GroupMemberships { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, an existing user account will be updated if found.")]
		[Category("User")]
		public InArgument<bool> UpdateExistingUser { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			PrincipalContext principalContext;
			if (IsLocalAccount.Get(context))
			{
				principalContext = new PrincipalContext(ContextType.Machine);
			}
			else
			{
				principalContext = new PrincipalContext(ContextType.Domain, Domain.Get(context), DomainContainer.Get(context));
			}

			UserPrincipal principal = UserPrincipal.FindByIdentity(principalContext, Username.Get(context));
			if (principal == null)
			{
				principal = new UserPrincipal(principalContext, Username.Get(context), Password.Get(context), true);
				principal.DisplayName = Firstname.Get(context) + " " + Lastname.Get(context);
				principal.PasswordNeverExpires = true;
				principal.Save();

				IList<string> groups = GroupMemberships.Get(context) ?? new List<string>();

				foreach (var group in groups)
				{
					GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(principalContext, group);
					if (!groupPrincipal.Members.Contains(principal))
					{
						groupPrincipal.Members.Add(principal);
					}
					groupPrincipal.Save();
				}
			}
			else if (UpdateExistingUser.Get(context))
			{
				principal.SetPassword(Password.Get(context));
				principal.DisplayName = Firstname.Get(context) + " " + Lastname.Get(context);
				principal.PasswordNeverExpires = true;
				principal.Save();

				IList<string> groups = GroupMemberships.Get(context) ?? new List<string>();

				foreach (var group in groups)
				{
					GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(principalContext, group);
					if (!groupPrincipal.Members.Contains(principal))
					{
						groupPrincipal.Members.Add(principal);
					}
					groupPrincipal.Save();
				}

				GroupPrincipal allGroups = new GroupPrincipal(principalContext);
				allGroups.Name = "*";
				PrincipalSearcher searcher = new PrincipalSearcher(allGroups);
				var allGroupList = searcher.FindAll();
				foreach (GroupPrincipal group in allGroupList)
				{
					if (!groups.Contains(group.Name) && group.Members.Contains(principal))
					{
						group.Members.Remove(principal);
						group.Save();
					}
				}
			}
		}
	}
}
