using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionMerger
{
	public class Project
	{

		public static class ProjectTypes
		{
			public const string CSharpCoreClassLibrary = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
			public const string SolutionFolderTypeGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
			public const string SqlDbProjectTypeGuid = "{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}";
		}
		public Guid TypeGuid { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public Guid ProjectGuid { get; private set; }
		public string SubItems => subItems.ToString();

		public List<Guid> ChildProjectGuids { get; set; } = new List<Guid>();
		public Guid? ParentGuid { get; set; }
		public string SolutionName { get; set; }

		internal static Project Parse(string line, string solutionName)
		{
			Project p = new Project();
			p.SolutionName = solutionName;
			var quotedParts = line.Split("\"");
			p.TypeGuid = Guid.Parse(quotedParts[1]);
			p.Name = quotedParts[3];
			p.Path = quotedParts[5];
			p.ProjectGuid = Guid.Parse(quotedParts[7]);

			return p;
		}

		StringBuilder subItems = new StringBuilder();

		public void AddSubItem(string line)
		{
			subItems.AppendLine(line);
		}

	}
}