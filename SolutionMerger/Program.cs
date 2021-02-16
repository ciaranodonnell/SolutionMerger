using System;
using System.Collections.Generic;
using System.IO;

namespace SolutionMerger
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");


			DirectoryInfo di = new DirectoryInfo(AppContext.BaseDirectory);
			var solutionFiles = di.GetFiles("*.sln", SearchOption.AllDirectories);
			List<Solution> solutions = new List<Solution>();
			string targetsln = Path.Combine(di.FullName, "Test.sln");
			foreach (var sol in solutionFiles)
			{
				if (sol.FullName.ToUpper() == targetsln.ToUpper()) continue;
				solutions.Add(new Solution(sol.FullName));
			}

			Combine c = new Combine(solutions.ToArray());
			c.Write(targetsln);

		//	Console.ReadLine();
		}
	}
}
