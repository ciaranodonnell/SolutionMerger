using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SolutionMerger
{
	public class Combine
	{
		private Solution[] solutions;

		public Combine(Solution[] solutions)
		{
			this.solutions = solutions;
		}



		public void Write(string fileName)
		{

			using (var filestream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
			using (var file = new StreamWriter(filestream))
			{
				file.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
				file.WriteLine("# Visual Studio Version 16");
				file.WriteLine("VisualStudioVersion = " + solutions.Max(s => s.VisualStudioVersion).ToString());
				file.WriteLine("MinimumVisualStudioVersion = " + solutions.Max(s => s.MinimumVisualStudioVersion).ToString());

				PrefixDuplicatedProjectNames(solutions);

				foreach (var s in solutions)
				{

					foreach (var p in s.Projects)
					{
						file.WriteLine($"Project(\"{p.TypeGuid.ToUpperString()}\") = \"{p.Name}\", \"{ MakeRelative(p.Path, s.Path, fileName)}\", \"{p.ProjectGuid.ToUpperString()}\"");
						file.Write(p.SubItems);
						file.WriteLine("EndProject");
					}

					file.WriteLine($"Project(\"{Project.ProjectTypes.SolutionFolderTypeGuid}\") = \"{s.Name}\", \"{ MakeRelative(s.Name, s.Path, fileName)}\", \"{s.SolutionGuid.ToUpperString()}\"");
					file.WriteLine("EndProject");

				}

				var sectionNames = new HashSet<(string SectionType, string Location)>(solutions.SelectMany(s => s.GlobalSections.Select(g => (g.Value.SectionType, g.Value.Location))));

				var uniqueLines = new HashSet<string>();

				foreach (var gs in sectionNames)
				{
					if (gs.SectionType == "ExtensibilityGlobals")
					{
						file.Write(@"	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = " + Guid.NewGuid().ToString() + @"
	EndGlobalSection
");
						continue;
					}
					else
					{


						file.WriteLine($"	GlobalSection({gs.SectionType}) = {gs.Location}");
						foreach (var s in solutions)
						{
							if (s.GlobalSections.TryGetValue(gs.SectionType, out var section))
							{
								foreach (var line in section.SubItems)
								{
									if (!uniqueLines.Contains(line))
									{
										file.WriteLine(line);
										uniqueLines.Add(line);

									}
								}

							}

						}

						if (gs.SectionType == "NestedProjects")
						{
							foreach (var s in solutions)
							{
								foreach (var p in s.Projects)
								{
									if (p.ParentGuid == null)
									{
										file.WriteLine($"		{p.ProjectGuid.ToUpperString()} = {s.SolutionGuid.ToUpperString()}");
									}
								}

							}
						}

						file.WriteLine("	EndGlobalSection");
					}


				}


			}
		}

		private void PrefixDuplicatedProjectNames(Solution[] solutions)
		{

			Dictionary<string, Project> projects = new Dictionary<string, Project>();
			List<Project> projectsToUpdate = new List<Project>();
			
			
			foreach (var s in solutions)
			{

				foreach (var p in s.Projects)
				{
					if (projects.ContainsKey(p.Name))
					{
						projectsToUpdate.Add(projects[p.Name]);
						p.Name = s.Name + "." + p.Name;
					}
					else
					{
						projects.Add(p.Name, p);
					}
				}
			}
			foreach(var ptu in projectsToUpdate)
			{
				ptu.Name = ptu.SolutionName + "." + ptu.Name;
			}

		}

		public static string MakeRelative(string projectPath, string originalSlnPath, string newSolutionPath)
		{
			string relPath = Path.GetRelativePath(Path.GetDirectoryName(newSolutionPath), Path.GetDirectoryName(originalSlnPath));
			return Path.Combine(relPath, projectPath);
		}
	}

	static class GuidExtn
	{
		public static string ToUpperString(this Guid g) => g.ToString("B").ToUpper();
		public static string ToUpperString(this Guid? g) => g?.ToString("B").ToUpper();
	}
}
