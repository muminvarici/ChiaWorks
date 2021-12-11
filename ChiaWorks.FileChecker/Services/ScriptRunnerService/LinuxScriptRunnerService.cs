using System.Diagnostics;

namespace ChiaWorks.FileChecker.Services.ScriptRunnerService
{
    public class LinuxScriptRunnerService : ScriptRunnerServiceBase
    {
        public override string RunCommand(string command)
        {
            var result = "";
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                result += proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();

                proc.WaitForExit();
            }

            return result;
        }
    }
}