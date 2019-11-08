using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using SimpleLyricsMaker.Logs.Models;

namespace SimpleLyricsMaker.Logs
{
    public static class LogExtension
    {
        private static readonly Dictionary<Assembly, Logger> AllLoggers = new Dictionary<Assembly, Logger>();

        public static void SetupLogger(Assembly assembly, LoggerMembers member)
        {
            if (!AllLoggers.ContainsKey(assembly))
                AllLoggers.Add(assembly, LoggerService.GetLogger(member));
        }

        public static void LogByObject(this object obj, string message, [CallerMemberName] string methodName = null) => obj.GetType().LogByType(message, methodName);
        public static void LogByObject(this object obj, Exception exception, [CallerMemberName] string methodName = null) => obj.GetType().LogByType(exception, methodName);
        public static void LogByObject(this object obj, Exception exception, string extraMessage, [CallerMemberName] string methodName = null) => obj.GetType().LogByType(exception, extraMessage, methodName);

        public static void LogByType(this Type type, string message, [CallerMemberName] string methodName = null)
        {
            var logger = AllLoggers[GetAssembly(type)];
            logger.Info($"{message} | {type.Name}.{methodName}");
        }

        public static void LogByType(this Type type, Exception exception, [CallerMemberName] string methodName = null)
        {
            var logger = AllLoggers[GetAssembly(type)];
            logger.Error(exception, $" | {type.Name}.{methodName}");
        }

        public static void LogByType(this Type type, Exception exception, string extraMessage, [CallerMemberName] string methodName = null)
        {
            var logger = AllLoggers[GetAssembly(type)];
            logger.Error(exception, $"{extraMessage} | {type.Name}.{methodName}");
        }

        private static Assembly GetAssembly(Type type)
        {
            return type.Assembly;
        }
    }
}