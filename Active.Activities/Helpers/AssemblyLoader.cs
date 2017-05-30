using Microsoft.VisualBasic.Activities;
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Services;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Active.Activities.Helpers
{
	public static class AssemblyLoader
	{
		private static Assembly loadedAssembly = null;
		public static Assembly LoadActivitiesAssembly()
		{
			loadedAssembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + @"\Active.Activities.dll");
			return loadedAssembly;
		}

		private static long Decompress(Stream inp, Stream outp)
		{
			byte[] buf = new byte[65535];
			long nBytes = 0;

			// Decompress the contents of the input file
			using (inp = new DeflateStream(inp, CompressionMode.Decompress))
			{
				int len;
				while ((len = inp.Read(buf, 0, buf.Length)) > 0)
				{
					// Write the data block to the decompressed output stream
					outp.Write(buf, 0, len);
					nBytes += len;
				}
			}
			// Done
			return nBytes;
		}

		//The following section comes from https://social.msdn.microsoft.com/Forums/vstudio/en-US/6784d84b-1b63-424c-8f12-261e59231337/add-assembly-references-runtime-in-rehosted-wf-designer?forum=wfprerelease
		//Might be useful later to be able to add assembly references to workflow designer

		//private static void DynamicAssemblyMonitor(WorkflowDesigner wd, string fullname, Assembly asm, bool toadd)
		//{
		//	//Get the designers acci
		//	var acci = wd.Context.Items.GetValue<AssemblyContextControlItem>() ?? new AssemblyContextControlItem();
		//	if (acci.ReferencedAssemblyNames == null)
		//		acci.ReferencedAssemblyNames = new List<AssemblyName>();
		//	if (toadd)
		//		AddDynamicAssembly(wd, acci, asm);
		//	else
		//		RemoveDynamicAssembly(wd, acci, asm);
		//}

		//private static void RemoveDynamicAssembly(WorkflowDesigner wd, AssemblyContextControlItem acci, Assembly asm)
		//{
		//	if (acci.ReferencedAssemblyNames.Contains(asm.GetName()))
		//	{
		//		acci.ReferencedAssemblyNames.Remove(asm.GetName());
		//		wd.Context.Items.SetValue(acci);
		//	}
		//	var root = GetRootElement(wd);
		//	if (null == root) return;
		//	VisualBasicSettings vbs = VisualBasic.GetSettings(root) ?? new VisualBasicSettings();

		//	var namespaces = (from type in asm.GetTypes() select type.Namespace).Distinct();
		//	var fullname = asm.FullName;
		//	foreach (var name in namespaces)
		//	{
		//		var theimport = (from importname in vbs.ImportReferences where importname.Assembly == fullname where importname.Import == name select importname).FirstOrDefault();
		//		if (theimport != null)
		//			vbs.ImportReferences.Remove(theimport);
		//	}
		//	VisualBasic.SetSettings(root, vbs);
		//}

		//private static void AddDynamicAssembly(WorkflowDesigner wd, AssemblyContextControlItem acci, Assembly asm)
		//{
		//	if (!acci.ReferencedAssemblyNames.Contains(asm.GetName()))
		//	{
		//		acci.ReferencedAssemblyNames.Add(asm.GetName());
		//		wd.Context.Items.SetValue(acci);
		//	}
		//	var root = GetRootElement(wd);
		//	var fullname = asm.FullName;
		//	if (null == root) return;
		//	VisualBasicSettings vbs = VisualBasic.GetSettings(root) ?? new VisualBasicSettings();

		//	var namespaces = (from type in asm.GetTypes() select type.Namespace).Distinct();
		//	foreach (var name in namespaces)
		//	{
		//		var import = new VisualBasicImportReference() { Assembly = fullname, Import = name };
		//		vbs.ImportReferences.Add(import);
		//	}
		//	VisualBasic.SetSettings(root, vbs);
		//}

		//private static object GetRootElement(WorkflowDesigner wd)
		//{
		//	var modelservice = wd.Context.Services.GetService<ModelService>();
		//	if (modelservice == null) return null;
		//	var rootmodel = modelservice.Root.GetCurrentValue();
		//	return rootmodel;
		//}
	}
}
