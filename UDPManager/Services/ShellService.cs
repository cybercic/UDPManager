using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UDPManager
{
    internal class ShellService
    {
        public static string ExecutaShell(string file, string args = null, string input = null)
        {
            Process shell = new Process();
            shell.StartInfo.FileName = file;
            if (!string.IsNullOrEmpty(args))
                shell.StartInfo.Arguments = args;
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
            shell.StartInfo.RedirectStandardOutput = true;
            shell.StartInfo.Verb = "runas";
            shell.StartInfo.CreateNoWindow = true;
            shell.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (!string.IsNullOrEmpty(input))
                shell.StartInfo.RedirectStandardInput = true;

            try
            {
                shell.Start();
                if (!string.IsNullOrEmpty(input))
                {
                    shell.StandardInput.WriteLine(input);
                    shell.StandardInput.Close();
                }
                string output = shell.StandardOutput.ReadToEnd();
                shell.WaitForExit();
                shell.Close();

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return String.Empty;
        }

        public static List<string> ObtemVariosValores(string file, string args = null, string input = null)
        {
            var retorno = ExecutaShell(file, args, input);
            return retorno.Replace("\n", "").Split('\r').Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
