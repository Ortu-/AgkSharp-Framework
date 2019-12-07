using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AGKCore
{
    public static class Dispatcher
    {
        private static List<Invokable> _FunctionList = new List<Invokable>();

        public static void Add(TimerCallback rCallable)
        {
            Invokable inv = new Invokable
            {
                Function = rCallable,
                Name = rCallable.GetMethodInfo().DeclaringType.Name + "." + rCallable.GetMethodInfo().Name,
            };
            _FunctionList.Add(inv);
        }

        public static object Invoke(string rCallable, string rArgs)
        {
            App.Log("Dispatcher.cs", 1, "main", "Dispatcher Try Invoke: " + rCallable);

            var fn = _FunctionList.FirstOrDefault(el => el.Name == rCallable);
            
            if(fn != null)
            {
                return _FunctionList.FirstOrDefault(el => el.Name == rCallable).Function.DynamicInvoke(rArgs);
            }

            return null;
        }
    }

    public class Invokable
    {
        public Delegate Function;
        public string Name;
    }
}
