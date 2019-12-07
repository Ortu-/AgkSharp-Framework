using System;
using System.Collections.Generic;
using System.Linq;
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
                Interval = rInterval,
                DoneTicks = 0
            };

            Timer t = new Timer(TickInterval, ts, rDelay, rInterval);
            ts.Timer = t;
            Scheduled.Add(ts);
        }

        public static void TickInterval(object sender)
        {

            if (App.Timing.PauseState == 1)
            {
                return;
            }

            TimerState ts = (TimerState)sender;
   
            ++ts.DoneTicks;
            ts.LastTick = App.Timing.Timer;

            if(App.Timing.PauseState == 2)
            {
                if(App.Timing.Timer < ts.LastTick + ts.Interval + App.Timing.PauseElapsed)
                {
                    return;
                }
            }

            bool isDone = ts.MaxTicks > -1 && ts.DoneTicks >= ts.MaxTicks;
            if (isDone)
            {
                Scheduled.Remove(ts);
                ts.Timer.Dispose();
            }

            ts.Callback.DynamicInvoke(sender);

            /* TODO: we shouldn't need to maintain a reference in the list during the invoke call. we should be able to get anything we need out of sender
            if (isDone && ts != null)
            {
                Scheduled.Remove(ts);
            }
            */
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
            public int Interval;
            public uint LastTick;
        }

    }

}
