using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Activities;
using System.IO;
using System.IO.Compression;
using Active.Activities.Helpers;

namespace Active.Builder.DesignerDataTypes
{
	internal class CustomActivityDefinitions
	{
		private static List<ActivityCategoryDefinition> categories = null;

		public static IList<ActivityCategoryDefinition> Categories
		{
			get
			{
				if (categories == null)
				{
					categories = new List<ActivityCategoryDefinition>
					{
						new ActivityCategoryDefinition { Name = "Active", ActivityTypes = new List<ActivityDefinition>() },
						new ActivityCategoryDefinition { Name = "Active.Azure", ActivityTypes = new List<ActivityDefinition>() },
					};
					List<Type> types = GetTypesImplementingInterface(AssemblyLoader.LoadActivitiesAssembly(), typeof(CodeActivity));

					foreach (var type in types.Where(t => t.Namespace != "Active.Activities.Azure"))
					{
						categories[0].ActivityTypes.Add(new ActivityDefinition
						{
							DisplayName = type.Name,
							ActivityType = type,
						});
					}
					foreach (var type in types.Where(t => t.Namespace == "Active.Activities.Azure"))
					{
						categories[1].ActivityTypes.Add(new ActivityDefinition
						{
							DisplayName = type.Name,
							ActivityType = type,
						});
					}
				}
				categories[0].ActivityTypes = categories[0].ActivityTypes.OrderBy(f => f.DisplayName).ToList();
				categories[1].ActivityTypes = categories[1].ActivityTypes.OrderBy(f => f.DisplayName).ToList();
				return categories.AsReadOnly();
			}
		}

		internal static List<Type> GetTypesImplementingInterface(Assembly assembly, Type interfaceType)
		{
			var types = assembly
				.GetTypes()
				.Where(p => interfaceType.IsAssignableFrom(p) && p != interfaceType && !p.IsAbstract);

			return types.ToList();
		}
	}
}
