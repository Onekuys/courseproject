using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject.Helpers
{
    public static class Logger
    {
        public static void Log(string action, string description)
        {
            string message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {action}: {description}{Environment.NewLine}";
            string logPath = System.IO.Path.Combine("Data", "logs.txt");
            File.AppendAllText(logPath, message);
        }
    }
}
