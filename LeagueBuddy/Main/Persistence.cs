using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.Main
{
    internal class Persistence
    {
        internal static readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueBuddy");
        internal static void CreateDataDir()
        {
            if (Directory.Exists(DataDir)) return;
            Directory.CreateDirectory(DataDir);
        }
    }
}
