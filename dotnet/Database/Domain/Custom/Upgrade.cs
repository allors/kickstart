using Allors.Database.Derivations;
using Allors.Database.Meta;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DateTime = System.DateTime;

namespace Allors.Database.Domain
{
    public class Upgrade
    {
        private readonly ITransaction transaction;

        private DirectoryInfo DataPath;

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public Upgrade(ITransaction transaction, DirectoryInfo dataPath)
        {
            this.transaction = transaction;
            this.DataPath = dataPath;
        }

        public void Execute()
        {
        }
    }
}