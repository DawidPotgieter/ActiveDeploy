using System.Activities;
using System.ComponentModel;
using Active.Activities.ActivityDesigners;

namespace Active.Builder.Activities
{
	[Designer(typeof(MessageBoxDesigner))]
	public sealed class MessageBox : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The title bar text of the message box.")]
		[Category("Display")]
		public InArgument<string> Caption { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The text to display on the message box.")]
		[Category("Display")]
		[RequiredArgument]
		public InArgument<string> Text { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The buttons to display on the message box.")]
		[Category("Display")]
		public InArgument<System.Windows.MessageBoxButton> Buttons { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The icon to display on the message box.")]
		[Category("Display")]
		public InArgument<System.Windows.MessageBoxImage> Icon { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Visual options for the message box.")]
		[Category("Display")]
		public InArgument<System.Windows.MessageBoxOptions> Options { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Contains the MessageBoxResult : which button was pressed by the user.")]
		[Category("Output")]
		public OutArgument<System.Windows.MessageBoxResult> Result { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			string caption = this.Caption.Get(context) ?? string.Empty;
			string text = this.Text.Get(context) ?? string.Empty;
			System.Windows.MessageBoxButton buttons = this.Buttons.Get(context);
			System.Windows.MessageBoxImage icon = this.Icon.Get(context);
			System.Windows.MessageBoxOptions options = this.Options.Get(context);

			Result.Set(context,System.Windows.MessageBox.Show(text, caption, buttons, icon, System.Windows.MessageBoxResult.None, options));
		}
	}
}
