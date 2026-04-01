using System;
using System.Collections.Generic;
using System.Text;

namespace Argus
{
    public class ArgusConfig
    {
        public int RestartThreshold { get; set; } = 3;
        public int RestartWindowSeconds { get; set; } = 10;
        public List<string> KillList { get; set; } = new() { "Windhawk" };
    }
}
