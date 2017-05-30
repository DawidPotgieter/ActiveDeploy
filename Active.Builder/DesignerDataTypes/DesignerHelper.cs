using System;
using System.Linq;
using System.Activities.Presentation.Metadata;
using System.Reflection;

namespace Active.Builder.DesignerDataTypes
{
	/// <summary>
	/// This is no longer used - been replaced by local icon resources.  Keeping it here for future reference only
	/// </summary>
	internal class DesignerHelper
	{
		public static void LoadToolboxIconsForBuiltInActivities()
		{
			try
			{
				AttributeTableBuilder builder = new AttributeTableBuilder();
				Assembly sourceAssembly = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + @"\Microsoft.VisualStudio.Activities.dll");
				System.Resources.ResourceReader resourceReader = new System.Resources.ResourceReader(sourceAssembly.GetManifestResourceStream("Microsoft.VisualStudio.Activities.Resources.resources"));
				foreach (Type type in typeof(System.Activities.Activity).Assembly.GetTypes().Where(t => t.Namespace == "System.Activities.Statements"))
				{
					CreateToolboxBitmapAttributeForActivity(builder, resourceReader, type);
				}
				MetadataStore.AddAttributeTable(builder.CreateTable());
			}
			catch { }
		}

		public static void AddToolboxIcon(Type activityType, System.Drawing.Bitmap bitmap)
		{
			try
			{
				AttributeTableBuilder builder = new AttributeTableBuilder();
				if (bitmap != null)
				{
					Type tbaType = typeof(System.Drawing.ToolboxBitmapAttribute);
					Type imageType = typeof(System.Drawing.Image);
					ConstructorInfo constructor = tbaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { imageType, imageType }, null);
					System.Drawing.ToolboxBitmapAttribute tba = constructor.Invoke(new object[] { bitmap, bitmap }) as System.Drawing.ToolboxBitmapAttribute;
					builder.AddCustomAttributes(activityType, tba);
				}
				MetadataStore.AddAttributeTable(builder.CreateTable());
			}
			catch { }
		}

		private static void CreateToolboxBitmapAttributeForActivity(AttributeTableBuilder builder, System.Resources.ResourceReader resourceReader, Type builtInActivityType)
		{
			try
			{
				System.Drawing.Bitmap bitmap = ExtractBitmapResource(resourceReader, builtInActivityType.IsGenericType ? builtInActivityType.Name.Split('`')[0] : builtInActivityType.Name);

				if (bitmap != null)
				{
					Type tbaType = typeof(System.Drawing.ToolboxBitmapAttribute);
					Type imageType = typeof(System.Drawing.Image);
					ConstructorInfo constructor = tbaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { imageType, imageType }, null);
					System.Drawing.ToolboxBitmapAttribute tba = constructor.Invoke(new object[] { bitmap, bitmap }) as System.Drawing.ToolboxBitmapAttribute;
					builder.AddCustomAttributes(builtInActivityType, tba);
				}
			}
			catch { }
		}

		private static System.Drawing.Bitmap ExtractBitmapResource(System.Resources.ResourceReader resourceReader, string bitmapName)
		{
			try
			{
				System.Collections.IDictionaryEnumerator dictEnum = resourceReader.GetEnumerator();
				System.Drawing.Bitmap bitmap = null;
				while (dictEnum.MoveNext())
				{
					if (String.Equals(dictEnum.Key, bitmapName))
					{
						bitmap = dictEnum.Value as System.Drawing.Bitmap;
						System.Drawing.Color pixel = System.Drawing.Color.FromArgb(255, 0, 255);
						bitmap.MakeTransparent(pixel);
						break;
					}
				}
				return bitmap;
			}
			catch { return null; }
		}
	}
}
