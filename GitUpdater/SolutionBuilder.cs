using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitUpdater.Properties;
using Microsoft.Build.Evaluation;

namespace GitUpdater
{
    class SolutionBuilder
    {
        public StringBuilder buildLog = new StringBuilder();
        public List<string> FindSolutions()
        {
            List<string> solutionFiles = new List<string>();
            foreach (string repoLocation in Settings.Default.FullRepoLocation)
            {
                solutionFiles.AddRange(Directory.GetFiles(repoLocation, "*.csproj", SearchOption.AllDirectories));
            }
            return solutionFiles;
        }

        public void BuildSolutions(List<string> solutionFiles)
        {
            foreach (string solution in solutionFiles)
            {
                buildLog.AppendLine("Preparing to build " + solution);
                Project project = new Project(solution);
                try
                {
                    project.Build();
                    buildLog.AppendLine("Solution built.");
                }
                catch (Exception e)
                {
                    buildLog.AppendLine(e.Message);
                }
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            }
        }
    }
}