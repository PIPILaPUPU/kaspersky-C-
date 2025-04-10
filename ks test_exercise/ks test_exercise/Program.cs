using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Security;
using System.Security.Cryptography;

namespace ks_test_exercise
{
    internal class Program
    {
        const string Path = @"D:\fot_test_exersice\new_test_files\files";//путь к файлам
        const string PatternPASS = @"(password\s*[:=]\s*)[^ \n\r]+";//паттерны для поиска паролей и лицензии
        const string PatternLICENSE = @"\b([A-Z0-9]{5}-){3}[A-Z0-9]{5}\b";

        static readonly Dictionary<string, string> ForbidenWords = new Dictionary<string, string>
        {
                {"master", "primary" },
                {"slave", "secondary" },
                {"whitelist", "allowlist" },
                {"blacklist", "denylist" },
                {"manhours", "personhours" },
                {"grandfathered", "legacy" },
                {"dummy", "stub" },
                {"sanitycheck", "integritycheck" },
                {"masterpassword", "mainpassword" }
        };
//------------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            string difficulty = "00000";

            if (PoW_Check(difficulty, out int nonce, out string hash, out double timeSec))
            {
                string logPath = @"d:\C# non University\kasp\ks test_exercise\ks test_exercise\log\log_data.log";//путь к логу для записи данных
                string logContent = $"[POW check] Date: {DateTime.UtcNow} \n" +
                                    $"[POW check] User: {Environment.UserName} \n" +
                                    $"[POW check] Difficult: {difficulty} \n" +
                                    $"[POW check] Founded nonce: {nonce} \n" +
                                    $"[POW check] Current hash: {hash} \n" +
                                    $"[POW check] Time to complete: {timeSec:F2} seconds \n" +
                                    "\n";
                File.AppendAllText(logPath, logContent);
                Console.WriteLine($"Data is wrote to log {logPath}");

                RunFileReplacing();
            }
        }
//------------------------------------------------------------------------------------------------------------------
        public static void RunFileReplacing()
        {
            var logFiles = Directory.GetFiles(Path, "*.log", SearchOption.AllDirectories);
            var txtFiles = Directory.GetFiles(Path, "*.txt", SearchOption.AllDirectories);

            foreach (var logFile in logFiles)
            {
                var lines = File.ReadAllLines(logFile);
                var proccesedLines = new List<string>();

                foreach (var line in lines)
                {
                    var newLine = replaceLine(line);
                    proccesedLines.Add(newLine);
                }

                File.WriteAllLines(logFile, proccesedLines);
            }

            foreach (var txtFile in txtFiles)
            {
                var lines = File.ReadAllLines(txtFile);
                var proccesedLines = new List<string>();

                foreach (var line in lines)
                {
                    var newLine = replaceLine(line);
                    proccesedLines.Add(newLine);
                }

                File.WriteAllLines(txtFile, proccesedLines);
            }

            Console.WriteLine("Work with files is over");
        }
//------------------------------------------------------------------------------------------------------------------
        public static string replaceLine(string line)
        {
            line = Regex.Replace(line, PatternPASS, "$1***PASWORD***", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, PatternLICENSE, "***LICENCE KEY***");

            foreach (var word in ForbidenWords)
            {
                var pattern = $@"\b{word.Key}\b";
                line = Regex.Replace(line, pattern, word.Value, RegexOptions.IgnoreCase);
            }

            return line;
        }
//------------------------------------------------------------------------------------------------------------------
        public static bool PoW_Check(string difficult, out int nonceOut, out string hashOut, out double timeOutSeconds)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            var timesTamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var baseString = Environment.UserName + timesTamp;

            Console.WriteLine("Work with PoW started");
            int nonce = 0;
            string hash;

            do
            {
                string data = baseString + nonce;
                hash = GetSha256(data);
                nonce++;

                if (nonce % 100000 == 0)
                {
                    Console.WriteLine($"Tried {nonce}... Current hash {hash}");
                }
            } while (!hash.StartsWith(difficult));

            stopWatch.Stop();

            Console.WriteLine($"Nonce: {nonce - 1}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine($"Lead time: {stopWatch.Elapsed.TotalSeconds:F2} seconds");

            nonceOut = nonce - 1;
            hashOut = hash;
            timeOutSeconds = stopWatch.Elapsed.TotalSeconds;

            return true;
        }
//------------------------------------------------------------------------------------------------------------------
        public static string GetSha256(string data)
        {
            using(var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
