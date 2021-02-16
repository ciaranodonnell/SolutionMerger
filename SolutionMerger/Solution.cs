using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SolutionMerger
{
	public class Solution
	{

		public Solution(string solutionPath)
		{

			Parse(solutionPath);

		}

		public Version VisualStudioVersion { get; private set; }
		public Version MinimumVisualStudioVersion { get; private set; }

		public Dictionary<string, GlobalSection> GlobalSections { get; set; } = new Dictionary<string, GlobalSection>();
		public List<Project> Projects { get; set; } = new List<Project>();
		public Dictionary<Guid, Project> ProjectsById = new Dictionary<Guid, Project>();
		public string Path { get; private set; }
		public string Name { get; internal set; }
		public Guid SolutionGuid { get; internal set; }

		void Parse(string slnFile)
		{
			this.Name = System.IO.Path.GetFileName(slnFile);
			this.Path = slnFile;
			bool isParsingProject = false;
			bool isParsingGlobal = false;
			var lines = File.ReadAllLines(slnFile);
			Project currentProject = null;

			GlobalSection currentGlobalSection = null;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line)) continue;
				if (IsComment(line)) continue;

				if (isParsingGlobal)
				{
					if (line.TrimStart().StartsWith("GlobalSection"))
					{
						currentGlobalSection = GlobalSection.Parse(line);
					}else if (line.TrimStart().StartsWith("EndGlobalSection"))
					{
						GlobalSections.Add(currentGlobalSection.SectionType, currentGlobalSection);
						currentGlobalSection = null;
					}
					else if (line == "EndGlobal")
					{
						isParsingGlobal = false;
					}
					else
					{
						currentGlobalSection.AddSubLine(line);
					}
				}
				else if (isParsingProject)
				{
					if (line.StartsWith("EndProject"))
					{
						isParsingProject = false;
						this.Projects.Add(currentProject);
					}
					else
					{
						currentProject.AddSubItem(line);
					}
				}
				else
				{
					if (line.StartsWith("VisualStudioVersion = "))
					{
						this.VisualStudioVersion = Version.Parse(line.Substring("VisualStudioVersion = ".Length));
					}
					if (line.StartsWith("MinimumVisualStudioVersion ="))
					{
						this.MinimumVisualStudioVersion = Version.Parse(line.Substring("MinimumVisualStudioVersion =".Length));
					}

					if (line.StartsWith("Project("))
					{
						isParsingProject = true;
						currentProject = Project.Parse(line, Name);
					}

					if (line.Trim() == "Global")
					{
						isParsingGlobal = true;
					}

				}
			}


			foreach (var p in Projects)
				ProjectsById[p.ProjectGuid] = p;

			if (GlobalSections.TryGetValue("NestedProjects", out var section))
			{
				foreach (var subItem in section.SubItems)
				{
					var guids = subItem.Split("=");
					var child = Guid.Parse(guids[0].Trim());
					var parent = Guid.Parse(guids[1].Trim());
					ProjectsById[child].ParentGuid = parent;
					ProjectsById[parent].ChildProjectGuids.Add(child);
				}
			}
			else
			{
				GlobalSections.Add("NestProjects", new GlobalSection { Location = "preSolution", SectionType = "NestedProjects" });
			}

			this.SolutionGuid = GetSolutionGuid();

		}

		private Guid GetSolutionGuid()
		{
			GlobalSections.TryGetValue("ExtensibilityGlobals", out var section);
			if (section != null && section.SubItems.Count == 1)
			{
				var line = section.SubItems[0];
				Guid g = Guid.Parse(line.Substring(line.IndexOf("{")));
				return g;
			}
			else
			{
				return Guid.NewGuid();
			}

		}

		private bool IsComment(string line) => line.StartsWith("#");

	}
}

