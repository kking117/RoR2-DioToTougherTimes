﻿using System;
using System.Collections.Generic;

namespace ZoeaRework.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();
        public static void RegisterState(Type thistype)
        {
            States.entityStates.Add(thistype);
        }
    }
}
