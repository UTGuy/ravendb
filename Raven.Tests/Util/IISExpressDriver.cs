﻿using System;

namespace Raven.Tests.Util
{
    class IISExpressDriver : ProcessDriver
    {
        public string Url { get; private set;  }

        protected override void Shutdown()
        {
            _process.StandardInput.Write("q");
            _process.StandardInput.Flush();

            if (!_process.WaitForExit(10000))
                throw new Exception("IISExpress did not halt within 10 seconds.");
        }

        public void Start(string physicalPath, int port)
        {
            var sitePhysicalDirectory = physicalPath;
            StartProcess(@"c:\program files (x86)\IIS Express\IISExpress.exe",
                @"/port:" + port + @" /path:" + sitePhysicalDirectory);

            var match = WaitForConsoleOutputMatching(@"Successfully registered URL ""([^""]*)""");

            Url = match.Groups[1].Value;
        }
    }
}