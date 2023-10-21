using System.Diagnostics;

namespace UDPManager.Services
{
    internal class WmicService
    {
        static string ExecutaShell(string arg)
        {
            Process shell = new Process();
            shell.StartInfo.FileName = "wmic";
            shell.StartInfo.Arguments = $"{arg} /format:list";
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.RedirectStandardOutput = true;
            shell.StartInfo.Verb = "runas";
            shell.StartInfo.CreateNoWindow = true;
            shell.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                shell.Start();
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

        public static string ObtemValorUnico(string arg)
        {
            var retorno = ExecutaShell(arg);
            return retorno.Substring(retorno.IndexOf('=') + 1, retorno.Length - retorno.IndexOf('=') - 1).Trim();
        }

        public static List<string> ObtemVariosValores(string arg)
        {
            var retorno = ExecutaShell(arg);
            return retorno.Replace("\n", "").Split('\r').Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
