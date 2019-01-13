﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AGKCore
{
    public static class StaticInvoke
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

        public static object Call(string rCallable, string rArgs)
        {
            return _FunctionList.FirstOrDefault(el => el.Name == rCallable).Function.DynamicInvoke(rArgs);
        }
    }

    public class Invokable
    {
        public Delegate Function;
        public string Name;
    }
}