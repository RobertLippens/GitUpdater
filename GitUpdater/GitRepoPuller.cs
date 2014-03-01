using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Mono.Posix;
using NGit;
using NGit.Api;
using NGit.Transport;
using GitUpdater.Properties;
using NGit.Errors;
using NGit.Treewalk;

namespace GitUpdater
{
    class GitRepoPuller
    {
        private WorkingTreeIterator workingTreeIt;
        readonly UsernamePasswordCredentialsProvider credentials = new UsernamePasswordCredentialsProvider(Properties.Settings.Default.Username,Properties.Settings.Default.Password);
        public StringBuilder gitLog = new StringBuilder();

        public GitRepoPuller() {

        }

        public void UpdateRepository(string repoLocation)
        {
            gitLog.Clear();
            try
            {
                gitLog.AppendLine("Attemping to pull from " + repoLocation + "...");
                var repository = Git.Open(repoLocation);
                //if (Settings.Default.EveryBranch)
                //{
                //    UpdateRepositoryAllBranches(repository);
                //}
                //else
                //{
                    UpdateSingleRepository(repository);
                    LogRepository(repository);
                //}
            }
            catch (Exception e)
            {
                gitLog.AppendLine(e.Message);
            }
        }

        private void UpdateSingleRepository(Git repository)
        {
            repository.Pull().SetCredentialsProvider(credentials).Call();
            gitLog.AppendLine("Pull completed.");
        }

        //private void UpdateRepositoryAllBranches(Git repository)
        //{
        //    string currentBranch = repository.GetRepository().GetBranch();
        //    gitLog.AppendLine("Currently on branch " + currentBranch + "...");
        //    var branches = repository.BranchList().Call().Select(x => x.GetName()).ToList();
        //    foreach (string branch in branches)
        //    {
        //        gitLog.AppendLine("Currently on branch " + branch + "...");
        //        //repository.Checkout().SetName(branch).Call();
        //        //repository.Pull().SetCredentialsProvider(credentials).Call();
        //    }
        //    //repository.Checkout().SetName(currentBranch).Call();
        //}

        public void LogRepository(Git repository)
        {            
            gitLog.AppendLine("Recent changes:");
            var commits = repository.Pull().SetCredentialsProvider(credentials).Call().GetMergeResult().GetMergedCommits().Select(x=>x.Name).ToList();
            foreach (var commit in commits)
            {
                gitLog.AppendLine(commit);
            }
        }
    }
}
