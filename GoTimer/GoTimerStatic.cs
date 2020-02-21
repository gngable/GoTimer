using System;
using System.Collections.Generic;
using System.Text;

namespace GoTimer
{
    public static class GoTimerStatic
    {
        public static Action<string, object> SaveProperty { get; set; }
        public static Func<string, object> GetProperty { get; set; }

        public static Func<string, bool> HasProperty { get; set; }
    }
}
