using System.Collections.Generic;

namespace Active.Builder.DesignerDataTypes
{
	internal class ActivityCategoryDefinition
	{
		public string Name { get; set; }
		public List<ActivityDefinition> ActivityTypes { get; set; }
	}
}
