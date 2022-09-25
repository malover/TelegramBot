using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxminded.Task11
{
    public static class MessageHolder
    {
        public static ConcurrentDictionary<string, List<string>> Dictionary = new ConcurrentDictionary<string, List<string>>();
    }
}
