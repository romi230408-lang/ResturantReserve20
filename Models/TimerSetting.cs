using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResturantReserve.Models
{
    public class TimerSetting(long totalTimeInMilliseconds, long intervalInMilliseconds)
    {
        public long MillisInFuture { get; set; } = totalTimeInMilliseconds;
        public long CountDownInterval { get; set; } = intervalInMilliseconds;
    }
}
