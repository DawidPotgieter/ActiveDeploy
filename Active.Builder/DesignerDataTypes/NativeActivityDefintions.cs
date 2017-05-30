using System.Collections.Generic;
using System.Activities.Statements;

namespace Active.Builder.DesignerDataTypes
{
	internal class NativeActivityDefintions
	{
		private static List<ActivityCategoryDefinition> categories =
			new List<ActivityCategoryDefinition>
				{
					new ActivityCategoryDefinition
					{
						Name = "Control Flow",
						ActivityTypes = new List<ActivityDefinition>
						{
							new ActivityDefinition { ActivityType = typeof(DoWhile) },
							new ActivityDefinition { ActivityType = typeof(ForEach<>), DisplayName = "ForEach<T>" },
							new ActivityDefinition { ActivityType = typeof(If) },
							new ActivityDefinition { ActivityType = typeof(Parallel) },
							new ActivityDefinition { ActivityType = typeof(ParallelForEach<>), DisplayName = "ParallelForEach<T>" },
							new ActivityDefinition { ActivityType = typeof(Pick) },
							new ActivityDefinition { ActivityType = typeof(PickBranch) },
							new ActivityDefinition { ActivityType = typeof(Sequence) },
							new ActivityDefinition { ActivityType = typeof(Switch<>), DisplayName = "Switch<T>" },
							new ActivityDefinition { ActivityType = typeof(While) },
						}
					},
					new ActivityCategoryDefinition
					{
						Name = "FlowChart",
						ActivityTypes = new List<ActivityDefinition>
						{
							new ActivityDefinition { ActivityType = typeof(Flowchart) },
							new ActivityDefinition { ActivityType = typeof(FlowDecision) },
							new ActivityDefinition { ActivityType = typeof(FlowSwitch<>) , DisplayName= "FlowSwitch<T>" },
						}
					},
					new ActivityCategoryDefinition
					{
						Name = "Runtime",
						ActivityTypes = new List<ActivityDefinition>
						{
							//new ActivityDefinition { ActivityType = typeof(Persist) },
							new ActivityDefinition { ActivityType = typeof(TerminateWorkflow) },
						}
					},
					new ActivityCategoryDefinition
					{
						Name = "Primitives",
						ActivityTypes = new List<ActivityDefinition>
						{
							new ActivityDefinition { ActivityType = typeof(Assign) },
							new ActivityDefinition { ActivityType = typeof(Assign<>) , DisplayName= "Assign<T>" },
							new ActivityDefinition { ActivityType = typeof(Delay) },
							new ActivityDefinition { ActivityType = typeof(InvokeMethod) },
							new ActivityDefinition { ActivityType = typeof(WriteLine) },
						}
					},
					new ActivityCategoryDefinition
					{
						Name = "Transaction",
						ActivityTypes = new List<ActivityDefinition>
						{
							new ActivityDefinition { ActivityType = typeof(CancellationScope) },
							new ActivityDefinition { ActivityType = typeof(CompensableActivity)},
							new ActivityDefinition { ActivityType = typeof(Compensate) },
							new ActivityDefinition { ActivityType = typeof(Confirm) },
							new ActivityDefinition { ActivityType = typeof(TransactionScope) },
						}
					},
					new ActivityCategoryDefinition
					{
						Name = "Error Handling",
						ActivityTypes = new List<ActivityDefinition>
						{
							new ActivityDefinition { ActivityType = typeof(Rethrow) },
							new ActivityDefinition { ActivityType = typeof(Throw)},
							new ActivityDefinition { ActivityType = typeof(TryCatch) },
						}
					},				
				};

		public static IList<ActivityCategoryDefinition> Categories
		{
			get
			{
				return categories.AsReadOnly();
			}
		}
	}
}
