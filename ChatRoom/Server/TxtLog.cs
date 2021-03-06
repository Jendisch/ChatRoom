﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TxtLog : ILogger
    {
        private string logFileName;

        public TxtLog(string fileName)
        {
            logFileName = "..\\ChatLog.txt";
        }

        public void Log(string message)
        {
            try
            {
                File.AppendAllText(logFileName, $"{DateTime.Now.ToString("h:mm:ss tt")}: {message}\n" + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
