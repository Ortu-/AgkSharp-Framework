using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace AGKCore
{
    public static class Scheduler
    {
        public static List<TimerState> Scheduled = new List<TimerState>();

        public static void SetInterval(TimerCallback rCallback, string rArgs, int rTicks, int rInterval, int rDelay, string rTarget)
        {
            TimerState ts = new TimerState
            {
                Callback = rCallback,
                Name = rCallback.GetMethodInfo().DeclaringType.Name + "." + rCallback.GetMethodInfo().Name,
                Args = rArgs,
                Target = rTarget,
                MaxTicks = rTicks,
                DoneTicks = 0
            };

            Timer t = new Timer(TickInterval, ts, rDelay, rInterval);
            ts.Timer = t;
            Scheduled.Add(ts);
        }

        public static void TickInterval(object sender)
        {
            TimerState ts = (TimerState)sender;
            var i = Scheduled.IndexOf(ts);
            ++Scheduled[i].DoneTicks;

            bool isDone = Scheduled[i].MaxTicks > -1 && Scheduled[i].DoneTicks >= Scheduled[i].MaxTicks;
            if (isDone)
            {
                Scheduled[i].Timer.Dispose();
            }

            Scheduled[i].Callback.DynamicInvoke(sender);

            if (isDone)
            {
                i = Scheduled.IndexOf(ts); //list may have changed, have to relocate this timer before removing it.
                Scheduled.RemoveAt(i);
            }
        }

        public class TimerState
        {
            public Timer Timer;
            public Delegate Callback;
            public string Name;
            public string Args;
            public string Target;
            public int MaxTicks;
            public int DoneTicks;
        }

    }

}
