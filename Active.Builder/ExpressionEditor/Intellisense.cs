using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Active.Builder.ExpressionEditor
{
	public class Intellisense
	{
		private TreeNodes intellisenseList = new TreeNodes();

		public TreeNodes IntellisenseList
		{
			get
			{
				return intellisenseList;
			}
		}

		public Intellisense()
		{
			var wfAsm = System.Reflection.Assembly.GetEntryAssembly();
			var refAsmList = (from x in wfAsm.GetReferencedAssemblies() select System.Reflection.Assembly.Load(x)).ToList();

			for(int i = 0; i < refAsmList.Count; i++)
			{
				try
				{
					var subAssemblyList = (from x in refAsmList[i].GetReferencedAssemblies() select Assembly.Load(x)).ToList();
					refAsmList.AddRange(subAssemblyList.Where(a => !refAsmList.Contains(a)));
				}
				catch { }
			}
			var typeList = refAsmList.SelectMany(a => (from x in a.GetTypes() where x.IsPublic && x.IsVisible && ((x.Namespace != null) && !x.Namespace.StartsWith("Active.")) select x)).ToList();
			intellisenseList.Nodes.Clear();
			foreach (var asmType in typeList)
			{
				this.AddNode(intellisenseList, asmType.Namespace);
				this.AddTypeNode(intellisenseList, asmType);
			}
			this.AddNode(intellisenseList, "New", false);

			this.SortNodes(intellisenseList);
		}

		private void AddNode(TreeNodes targetNodes, string namePath)
		{
			var targetPath = namePath.Split('.');
			bool validPath = false;
			TreeNodes existsNodes = null;

			var validNode = targetNodes.Nodes.Where(x => x.Name.ToLower() == targetPath[0].ToLower());

			if ((validNode != null) && (validNode.Count() > 0))
			{
				existsNodes = validNode.FirstOrDefault();
				validPath = true;
			}

			if (!validPath)
			{
				TreeNodes childNodes = new TreeNodes
				{
					Name = targetPath[0],
					AddStrings = targetPath[0],
					ItemType = TreeNodes.NodeTypes.Namespace,
					Parent = targetNodes,
					Description = string.Format("Namespace {0}", targetPath[0])
				};
				targetNodes.AddNode(childNodes);

				string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
				if (nextPath.StartsWith("."))
					nextPath = nextPath.Substring(1, nextPath.Length - 1);
				if (nextPath.Trim() != "")
					this.AddNode(childNodes, nextPath);
			}
			else
			{
				string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
				if (nextPath.StartsWith("."))
					nextPath = nextPath.Substring(1, nextPath.Length - 1);
				if (nextPath.Trim() != "")
					this.AddNode(existsNodes, nextPath);
			}
		}

		private void AddNode(TreeNodes targetNodes, string namePath, bool isNamespace)
		{
			var targetPath = namePath.Split('.');
			bool validPath = false;
			TreeNodes existsNodes = null;

			var validNode = targetNodes.Nodes.Where(x => x.Name.ToLower() == targetPath[0].ToLower());

			if ((validNode != null) && (validNode.Count() > 0))
			{
				existsNodes = validNode.FirstOrDefault();
				validPath = true;
			}

			if (!validPath)
			{
				TreeNodes childNodes = new TreeNodes
				{
					Name = targetPath[0],
					AddStrings = targetPath[0],
					ItemType = isNamespace ? TreeNodes.NodeTypes.Namespace : TreeNodes.NodeTypes.Primitive,
					Parent = targetNodes,
					Description = isNamespace ? string.Format("Namespace {0}", targetPath[0]) : ""
				};
				targetNodes.AddNode(childNodes);

				if (isNamespace)
				{
					string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
					if (nextPath.StartsWith("."))
						nextPath = nextPath.Substring(1, nextPath.Length - 1);
					if (nextPath.Trim() != "")
						this.AddNode(childNodes, nextPath);
				}
			}
			else
			{
				if (isNamespace)
				{
					string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
					if (nextPath.StartsWith("."))
						nextPath = nextPath.Substring(1, nextPath.Length - 1);
					if (nextPath.Trim() != "")
						this.AddNode(existsNodes, nextPath);
				}
			}
		}

		private void AddTypeNode(TreeNodes targetNodes, Type target)
		{
			if (target.IsAbstract || !target.IsVisible)
				return;

			var typeNamespace = target.Namespace;
			var typeName = target.Name;

			var parentNode = this.SearchNodes(targetNodes, typeNamespace);
			TreeNodes newNodes = new TreeNodes
			{
				Name = typeName,
				AddStrings = typeName,
				Parent = parentNode,
				SystemType = target
			};
			string nodesName = typeName;
			if (target.IsGenericType)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.Class;
				if (typeName.Contains("`"))
				{
					nodesName = typeName.Substring(0, typeName.LastIndexOf("`"));
					newNodes.AddStrings = nodesName;
				}
				System.Text.StringBuilder paramStrings = new System.Text.StringBuilder();
				int count = 0;
				foreach (var childArg in target.GetGenericArguments())
				{
					if (count > 0)
						paramStrings.Append(", ");
					paramStrings.Append(childArg.Name);
					count += 1;
				}

				nodesName += "(" + paramStrings.ToString() + ")";
				newNodes.Name = nodesName;
				newNodes.Description = string.Format("Class {0}", newNodes.AddStrings);
			}
			else if (target.IsClass)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.Class;
				newNodes.Description = string.Format("Class {0}", newNodes.AddStrings);
			}
			else if (target.IsEnum)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.Enum;
				newNodes.Description = string.Format("Enum {0}", newNodes.AddStrings);
			}
			else if (target.IsInterface)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.Interface;
				newNodes.Description = string.Format("Interface {0}", newNodes.AddStrings);
			}
			else if (target.IsPrimitive)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.Primitive;
				newNodes.Description = string.Format("{0}", newNodes.AddStrings);
			}
			else if (target.IsValueType)
			{
				newNodes.ItemType = TreeNodes.NodeTypes.ValueType;
				newNodes.Description = string.Format("{0}", newNodes.AddStrings);
			}
			else
			{
				return;
			}
			if (parentNode == null)
			{
				targetNodes.AddNode(newNodes);
			}
			else
			{
				parentNode.AddNode(newNodes);
			}
			this.AddMethodNode(newNodes, target);
			this.AddPropertyNode(newNodes, target);
			this.AddFieldNode(newNodes, target);
			this.AddEventNode(newNodes, target);
			this.AddNestedTypeNode(newNodes, target);
		}

		private void AddMethodNode(TreeNodes targetNodes, Type target)
		{
			System.Threading.Tasks.Parallel.ForEach(target.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance), targetmember =>
			{
				TreeNodes memberNodes = new TreeNodes
				{
					Name = targetmember.Name,
					AddStrings = targetmember.Name,
					ItemType = TreeNodes.NodeTypes.Method,
					Parent = targetNodes,
					Description = CreateMethodDescription(targetmember)
				};

				targetNodes.AddNode(memberNodes);
			});
		}

		private void AddPropertyNode(TreeNodes targetNodes, Type target)
		{
			System.Threading.Tasks.Parallel.ForEach(target.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance), targetmember =>
			{
				TreeNodes memberNodes = new TreeNodes
				{
					Name = targetmember.Name,
					AddStrings = targetmember.Name,
					ItemType = TreeNodes.NodeTypes.Property,
					Parent = targetNodes,
					Description = CreatePropertyDescription(targetmember)
				};

				targetNodes.AddNode(memberNodes);
			});
		}

		private void AddFieldNode(TreeNodes targetNodes, Type target)
		{
			System.Threading.Tasks.Parallel.ForEach(target.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance), targetmember =>
			{
				TreeNodes memberNodes = new TreeNodes
				{
					Name = targetmember.Name,
					AddStrings = targetmember.Name,
					ItemType = TreeNodes.NodeTypes.Field,
					Parent = targetNodes,
					Description = CreateFieldDescription(targetmember)
				};

				targetNodes.AddNode(memberNodes);
			});
		}

		private void AddEventNode(TreeNodes targetNodes, Type target)
		{
			System.Threading.Tasks.Parallel.ForEach(target.GetEvents(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance), targetmember =>
			{
				TreeNodes memberNodes = new TreeNodes
				{
					Name = targetmember.Name,
					AddStrings = targetmember.Name,
					ItemType = TreeNodes.NodeTypes.Event,
					Parent = targetNodes,
					Description = CreateEventDescription(targetmember)
				};

				targetNodes.AddNode(memberNodes);
			});
		}

		private void AddNestedTypeNode(TreeNodes targetNodes, Type target)
		{
			System.Threading.Tasks.Parallel.ForEach(target.GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance), targetmember =>
			{
				TreeNodes memberNodes = new TreeNodes
				{
					Name = targetmember.Name,
					AddStrings = targetmember.Name,
					ItemType = TreeNodes.NodeTypes.Method,
					Parent = targetNodes
				};

				targetNodes.AddNode(memberNodes);
			});
		}

		private TreeNodes SearchNodes(TreeNodes targetNodes, string namePath)
		{
			var targetPath = namePath.Split('.');
			bool validPath = false;
			TreeNodes existsNodes = null;

			var validNode = targetNodes.Nodes.Where(x => x.Name.ToLower() == targetPath[0].ToLower());

			if ((validNode != null) && (validNode.Count() > 0))
			{
				existsNodes = validNode.FirstOrDefault();
				validPath = true;
			}

			if (!validPath)
				return targetNodes;

			var nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
			if (nextPath.StartsWith("."))
				nextPath = nextPath.Substring(1, nextPath.Length - 1);
			if (nextPath.Trim() == "")
				return existsNodes;
			return this.SearchNodes(existsNodes, nextPath);
		}

		private void SortNodes(TreeNodes targetNodes)
		{
			targetNodes.Nodes.Sort(new ComparerName());
			foreach (var childNode in targetNodes.Nodes)
			{
				this.SortNodes(childNode);
			}
		}

		private string CreateMethodDescription(MethodInfo target)
		{
			StringBuilder desc = new StringBuilder();
			if (target.IsPublic)
				desc.Append("Public ");
			if (target.IsFamily)
				desc.Append("Protected ");
			if (target.IsAssembly)
				desc.Append("Friend ");
			if (target.IsPrivate)
				desc.Append("Private ");
			if (target.IsAbstract)
				desc.Append("MustOverride ");
			if (target.IsVirtual && !target.IsFinal)
				desc.Append("Overridable ");
			if (target.IsStatic)
				desc.Append("Shared ");

			if ((!object.ReferenceEquals(target.ReturnType, typeof(void))))
			{
				desc.Append("Function ");
			}
			else
			{
				desc.Append("Sub ");
			}

			desc.Append(target.Name);
			desc.Append(CreateGenericParameter(target));

			desc.Append("(");
			int paramIndex = 0;
			foreach (var param in target.GetParameters())
			{
				if (paramIndex > 0)
					desc.Append(", ");
				if (param.IsOptional)
					desc.Append("Optional ");
				if (param.IsOut)
				{
					desc.Append("ByRef ");
				}
				else
				{
					desc.Append("ByVal ");
				}
				desc.Append(param.Name + " As " + param.ParameterType.Name);
				desc.Append(CreateGenericParameter(param.ParameterType));
				if (!Convert.IsDBNull(param.DefaultValue))
				{
					if (param.DefaultValue == null)
					{
						desc.Append(" = Nothing");
					}
					else
					{
						desc.Append(" = " + param.DefaultValue.ToString());
					}
				}
				paramIndex += 1;
			}
			desc.Append(") ");
			if (target.ReturnType != null)
			{
				desc.Append("As " + target.ReturnType.Name);
				desc.Append(CreateGenericParameter(target.ReturnType));
			}
			return desc.ToString();
		}

		private string CreatePropertyDescription(PropertyInfo target)
		{
			StringBuilder desc = new StringBuilder();

			if (target.CanRead && target.CanWrite)
			{
			}
			else if (target.CanRead)
			{
				desc.Append("ReadOnly ");
			}
			else
			{
				desc.Append("WriteOnly ");
			}
			desc.Append("Property " + target.Name + " As " + target.PropertyType.Name);
			desc.Append(CreateGenericParameter(target.PropertyType));

			return desc.ToString();
		}

		private string CreateFieldDescription(FieldInfo target)
		{
			StringBuilder desc = new StringBuilder();
			if (target.IsPublic)
				desc.Append("Public ");
			if (target.IsPrivate)
				desc.Append("Private ");
			if (target.IsStatic)
				desc.Append("Shared ");

			desc.Append(target.Name);
			desc.Append("() ");
			if (target.FieldType != null)
			{
				desc.Append("As " + target.FieldType.Name);
				desc.Append(CreateGenericParameter(target.FieldType));
			}
			return desc.ToString();
		}

		private string CreateEventDescription(EventInfo target)
		{
			StringBuilder desc = new StringBuilder();

			desc.Append(target.Name);
			if (target.EventHandlerType != null)
			{
				desc.Append("As " + target.EventHandlerType.Name);
				if (target.EventHandlerType.IsGenericType)
				{
					desc.Append(CreateGenericParameter(target.EventHandlerType));
				}
			}
			return desc.ToString();
		}

		private string CreateGenericParameter(MethodInfo target)
		{
			StringBuilder result = new StringBuilder();
			if (target.IsGenericMethod)
			{
				result.Append("(Of ");
				int genIndex = 0;
				foreach (var genParam in target.GetGenericArguments())
				{
					if (genIndex > 0)
						result.Append(", ");
					result.Append(genParam.Name);
					genIndex += 1;
				}
				result.Append(")");
			}
			return result.ToString();
		}

		private string CreateGenericParameter(Type target)
		{
			StringBuilder result = new StringBuilder();
			if (target.IsGenericType)
			{
				result.Append("(Of ");
				int genIndex = 0;
				foreach (var genParam in target.GetGenericArguments())
				{
					if (genIndex > 0)
						result.Append(", ");
					result.Append(genParam.Name);
					genIndex += 1;
				}
				result.Append(")");
			}
			return result.ToString();
		}

		public string CreateKeywords()
		{
			StringBuilder words = new StringBuilder();
			{
				words.Append("AddHandler|");
				words.Append("AddressOf|");
				words.Append("Alias|");
				words.Append("And|");
				words.Append("AndAlso|");
				words.Append("As|");
				words.Append("Boolean|");
				words.Append("ByRef|");
				words.Append("Byte|");
				words.Append("ByVal|");
				words.Append("Call|");
				words.Append("Case|");
				words.Append("Catch|");
				words.Append("CBool|");
				words.Append("CByte|");
				words.Append("CChar|");
				words.Append("CDate|");
				words.Append("CDbl|");
				words.Append("CDec|");
				words.Append("Char|");
				words.Append("CInt|");
				words.Append("Class|");
				words.Append("CLng|");
				words.Append("CObj|");
				words.Append("Const|");
				words.Append("Continue|");
				words.Append("CSByte|");
				words.Append("CShort|");
				words.Append("CSng|");
				words.Append("CStr|");
				words.Append("CType|");
				words.Append("CUInt|");
				words.Append("CULng|");
				words.Append("CUShort|");
				words.Append("Date|");
				words.Append("Decimal|");
				words.Append("Declare|");
				words.Append("Default|");
				words.Append("Delegate|");
				words.Append("Dim|");
				words.Append("DirectCast|");
				words.Append("Do|");
				words.Append("Double|");
				words.Append("Each|");
				words.Append("Else|");
				words.Append("ElseIf|");
				words.Append("End|");
				words.Append("EndIf|");
				words.Append("Enum|");
				words.Append("Erase|");
				words.Append("Error|");
				words.Append("Event|");
				words.Append("Exit|");
				words.Append("False|");
				words.Append("Finally|");
				words.Append("For|");
				words.Append("Friend|");
				words.Append("Function|");
				words.Append("Get|");
				words.Append("GetType|");
				words.Append("GetXMLNamespace|");
				words.Append("Global|");
				words.Append("GoSub|");
				words.Append("GoTo|");
				words.Append("Handles|");
				words.Append("If|");
				words.Append("Implements|");
				words.Append("Imports|");
				words.Append("In|");
				words.Append("Inherits|");
				words.Append("Integer|");
				words.Append("Interface|");
				words.Append("Is|");
				words.Append("IsNot|");
				words.Append("Let|");
				words.Append("Lib|");
				words.Append("Like|");
				words.Append("Long|");
				words.Append("Loop|");
				words.Append("Me|");
				words.Append("Mod|");
				words.Append("Module|");
				words.Append("MustInherit|");
				words.Append("MustOverride|");
				words.Append("MyBase|");
				words.Append("MyClass|");
				words.Append("Namespace|");
				words.Append("Narrowing|");
				words.Append("New|");
				words.Append("Next|");
				words.Append("Not|");
				words.Append("Nothing|");
				words.Append("NotInheritable|");
				words.Append("NotOverridable|");
				words.Append("Object|");
				words.Append("Of|");
				words.Append("On|");
				words.Append("Operator|");
				words.Append("Option|");
				words.Append("Optional|");
				words.Append("Or|");
				words.Append("OrElse|");
				words.Append("Out|");
				words.Append("Overloads|");
				words.Append("Overridable|");
				words.Append("Overrides|");
				words.Append("ParamArray|");
				words.Append("Partial|");
				words.Append("Private|");
				words.Append("Property|");
				words.Append("Protected|");
				words.Append("Public|");
				words.Append("RaiseEvent|");
				words.Append("ReadOnly|");
				words.Append("ReDim|");
				words.Append("REM|");
				words.Append("RemoveHandler|");
				words.Append("Resume|");
				words.Append("Return|");
				words.Append("SByte|");
				words.Append("Select|");
				words.Append("Set|");
				words.Append("Shadows|");
				words.Append("Shared|");
				words.Append("Short|");
				words.Append("Single|");
				words.Append("Static|");
				words.Append("Step|");
				words.Append("Stop|");
				words.Append("String|");
				words.Append("Structure|");
				words.Append("Sub|");
				words.Append("SyncLock|");
				words.Append("Then|");
				words.Append("Throw|");
				words.Append("To|");
				words.Append("True|");
				words.Append("Try|");
				words.Append("TryCast|");
				words.Append("TypeOf|");
				words.Append("UInteger|");
				words.Append("ULong|");
				words.Append("UShort|");
				words.Append("Using|");
				words.Append("Variant|");
				words.Append("Wend|");
				words.Append("While|");
				words.Append("Widening|");
				words.Append("With|");
				words.Append("WithEvents|");
				words.Append("WriteOnly|");
				words.Append("Xor|");
				words.Append("#Const|");
				words.Append("#Else|");
				words.Append("#ElseIf|");
				words.Append("#End|");
				words.Append("#If");
			}
			return words.ToString();
		}
	}
}
