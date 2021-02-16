using System.Collections.Generic;
using System.Text;

namespace SolutionMerger
{
	public class GlobalSection
	{
		public string SectionType { get; set; }
		public string Location { get; set; }
		public List<string> SubItems { get; internal set; } = new List<string>();

		public static GlobalSection Parse(string line)
		{
			GlobalSection g = new GlobalSection();

			var halves = line.Split(") = ");

			g.SectionType = halves[0].Substring(halves[0].IndexOf("(") + 1);
			g.Location = halves[1];

			return g;
		}


		public void AddSubLine(string line)
		{
			SubItems.Add(line);
		}

		

	}
}