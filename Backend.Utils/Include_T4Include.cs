
// ############################################################################
// #                                                                          #
// #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
// #                                                                          #
// # This means that any edits to the .cs file will be lost when its          #
// # regenerated. Changes should instead be applied to the corresponding      #
// # text template file (.tt)                                                      #
// ############################################################################



// ############################################################################
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
// @@@ INCLUDE_FOUND: ../Common/Array.cs
// @@@ INCLUDE_FOUND: ../Common/Config.cs
// @@@ INCLUDE_FOUND: ../Common/Log.cs
// @@@ INCLUDING: https://raw.github.com/chgeuer/WA.Tomcat.Sample/master/WebRole/RestartingProcessHost.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
// @@@ INCLUDE_FOUND: Config.cs
// @@@ INCLUDE_FOUND: Generated_Log.cs
// @@@ SKIPPING (Already seen): https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
// ############################################################################
// Certains directives such as #define and // Resharper comments has to be 
// moved to top in order to work properly    
// ############################################################################
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
namespace Backend.Utils
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    
    namespace Source.Extensions
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.IO;
        using System.Reflection;
    
        using Source.Common;
    
        static partial class BasicExtensions
        {
            public static bool IsNullOrWhiteSpace (this string v)
            {
                return string.IsNullOrWhiteSpace (v);
            }
    
            public static bool IsNullOrEmpty (this string v)
            {
                return string.IsNullOrEmpty (v);
            }
    
            public static T FirstOrReturn<T>(this T[] values, T defaultValue)
            {
                if (values == null)
                {
                    return defaultValue;
                }
    
                if (values.Length == 0)
                {
                    return defaultValue;
                }
    
                return values[0];
            }
    
            public static T FirstOrReturn<T>(this IEnumerable<T> values, T defaultValue)
            {
                if (values == null)
                {
                    return defaultValue;
                }
    
                foreach (var value in values)
                {
                    return value;
                }
    
                return defaultValue;
            }
    
            public static void Shuffle<T>(this T[] values, Random random)
            {
                if (values == null)
                {
                    return;
                }
    
                if (random == null)
                {
                    return;
                }
    
                for (var iter = 0; iter < values.Length; ++iter)
                {
                    var swapWith = random.Next (iter, values.Length);
    
                    var tmp = values[iter];
    
                    values[iter] = values[swapWith];
                    values[swapWith] = tmp;
                }
    
            }
    
            public static string DefaultTo (this string v, string defaultValue = null)
            {
                return !v.IsNullOrEmpty () ? v : (defaultValue ?? "");
            }
    
            public static IEnumerable<T> DefaultTo<T>(
                this IEnumerable<T> values, 
                IEnumerable<T> defaultValue = null
                )
            {
                return values ?? defaultValue ?? Array<T>.Empty;
            }
    
            public static T[] DefaultTo<T>(this T[] values, T[] defaultValue = null)
            {
                return values ?? defaultValue ?? Array<T>.Empty;
            }
    
            public static T DefaultTo<T>(this T v, T defaultValue = default (T))
                where T : struct, IEquatable<T>
            {
                return !v.Equals (default (T)) ? v : defaultValue;
            }
    
            public static string FormatWith (this string format, CultureInfo cultureInfo, params object[] args)
            {
                return string.Format (cultureInfo, format ?? "", args.DefaultTo ());
            }
    
            public static string FormatWith (this string format, params object[] args)
            {
                return format.FormatWith (Config.DefaultCulture, args);
            }
    
            public static TValue Lookup<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary, 
                TKey key, 
                TValue defaultValue = default (TValue))
            {
                if (dictionary == null)
                {
                    return defaultValue;
                }
    
                TValue value;
                return dictionary.TryGetValue (key, out value) ? value : defaultValue;
            }
    
            public static TValue GetOrAdd<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary, 
                TKey key, 
                TValue defaultValue = default (TValue))
            {
                if (dictionary == null)
                {
                    return defaultValue;
                }
    
                TValue value;
                if (!dictionary.TryGetValue (key, out value))
                {
                    value = defaultValue;
                    dictionary[key] = value;
                }
    
                return value;
            }
    
            public static TValue GetOrAdd<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                Func<TValue> valueCreator
                )
            {
                if (dictionary == null)
                {
                    return valueCreator ();
                }
    
                TValue value;
                if (!dictionary.TryGetValue (key, out value))
                {
                    value = valueCreator ();
                    dictionary[key] = value;
                }
    
                return value;
            }
    
            public static void DisposeNoThrow (this IDisposable disposable)
            {
                try
                {
                    if (disposable != null)
                    {
                        disposable.Dispose ();
                    }
                }
                catch (Exception exc)
                {
                    Log.Exception ("DisposeNoThrow: Dispose threw: {0}", exc);
                }
            }
    
            public static TTo CastTo<TTo> (this object value, TTo defaultValue)
            {
                return value is TTo ? (TTo) value : defaultValue;
            }
    
            public static string Concatenate (this IEnumerable<string> values, string delimiter = null, int capacity = 16)
            {
                values = values ?? Array<string>.Empty;
                delimiter = delimiter ?? ", ";
    
                return string.Join (delimiter, values);
            }
    
            public static string GetResourceString (this Assembly assembly, string name, string defaultValue = null)
            {
                defaultValue = defaultValue ?? "";
    
                if (assembly == null)
                {
                    return defaultValue;
                }
    
                var stream = assembly.GetManifestResourceStream (name ?? "");
                if (stream == null)
                {
                    return defaultValue;
                }
    
                using (stream)
                using (var streamReader = new StreamReader (stream))
                {
                    return streamReader.ReadToEnd ();
                }
            }
    
            public static IEnumerable<string> ReadLines (this TextReader textReader)
            {
                if (textReader == null)
                {
                    yield break;
                }
    
                string line;
    
                while ((line = textReader.ReadLine ()) != null)
                {
                    yield return line;
                }
            }
    
    #if !NETFX_CORE
            public static IEnumerable<Type> GetInheritanceChain (this Type type)
            {
                while (type != null)
                {
                    yield return type;
                    type = type.BaseType;
                }
            }
    #endif
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/chgeuer/WA.Tomcat.Sample/master/WebRole/RestartingProcessHost.cs
namespace Microsoft.DPE.Samples.ChGeuer
{
      // @Author: chgeuer@microsoft.com

    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class RestartingProcessHost
    {
        internal class ValueTypeWrapper<T>
        {
            // Maybe I'm stoopid, but how can I change a value type which is wrapped in a closure, unless I wrap it in an object?!?
            public T Value { get; set; }
        }

        private readonly Func<Process> CreateProcess;
        private readonly CancellationTokenSource cancellationTokenSource;
        private bool _onStopRequested = false;
        private Func<bool> AllowedToRestart = () => true; // by default, restart infinitely

        public RestartingProcessHost(Func<Process> createProcess, CancellationTokenSource cancellationTokenSource, Func<bool> allowedToRestart = null)
        {
            this.CreateProcess = createProcess;
            this.cancellationTokenSource = cancellationTokenSource;
            if (allowedToRestart != null) { this.AllowedToRestart = allowedToRestart; }

            this.cancellationTokenSource.Token.Register(() =>
            {
                this._onStopRequested = true;
            });
        }

        #region Logging

        private readonly Action<string> _defaultLogger = (s) => { };

        private Action<string> _error;
        public Action<string> error
        {
            get { return _error ?? (_error = _defaultLogger); }
            set { _error = value; }
        }
        private Action<string> _warn;
        public Action<string> warn
        {
            get { return _warn ?? (_warn = _defaultLogger); }
            set { _warn = value; }
        }
        private Action<string> _info;
        public Action<string> info
        {
            get { return _info ?? (_info = _defaultLogger); }
            set { _info = value; }
        }

        #endregion

        public Task StartRunTask()
        {
            var cancellationToken = cancellationTokenSource.Token;
            var processLaunched = new ValueTypeWrapper<bool> { Value = false };
            var processExitedEventHappened = new ValueTypeWrapper<bool> { Value = false };
            var pid = new ValueTypeWrapper<int>();
            var processName = new ValueTypeWrapper<string>();
            bool IsfirstStart = true;

            Action hostProcess = () =>
            {
                #region hostProcess

                var process = this.CreateProcess();

                process.Exited += (s, a) => processExitedEventHappened.Value = true;

                info(string.Format("Try to launch {0}", process.StartInfo.FileName));

                if (!IsfirstStart && !AllowedToRestart())
                {
                    error("The process was not allowed to restart");
                    return;
                }

                process.Start();
                IsfirstStart = false;
                if (process.StartInfo.RedirectStandardOutput) process.BeginOutputReadLine();
                if (process.StartInfo.RedirectStandardError) process.BeginErrorReadLine();
                pid.Value = process.Id;
                processLaunched.Value = true;
                processName.Value = process.StartInfo.FileName;

                info(string.Format("Launched {0} (pid {1})", processName.Value, pid.Value));

                if (cancellationToken.WaitHandle.WaitOne()) // wait infinite 
                {
                    warn(string.Format("Killing {0} now", processName.Value));
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }

                #endregion
            };

            var processTask = Task.Factory.StartNew(hostProcess, cancellationToken);
            while (!processLaunched.Value)
            {
                warn("Waiting a until the task is launched...");

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Action hostMonitor = () =>
            {
                int i = 0;
                while (true)
                {
                    #region Ensure task didn't crash accidentally, and if so, re-launch

                    bool processFoundInMemory = false;
                    try
                    {
                        var processFromSystem = Process.GetProcessById(pid.Value);
                        processFoundInMemory = true;
                    }
                    catch (ArgumentException) { }

                    bool unexpectedTermination = processExitedEventHappened.Value || !processFoundInMemory;

                    if (processTask.IsCompleted || unexpectedTermination)
                    {
                        if (_onStopRequested)
                        {
                            error(string.Format("Process {0} successfully shut down because of an OnStop() call", processName.Value));
                            return; // Leave the Run() method
                        }
                        else if (unexpectedTermination)
                        {
                            error(string.Format("Process {0} stopped working for unknown reasons... Restarting it", processName.Value));
                            processExitedEventHappened.Value = false;

                            processLaunched.Value = false;
                            processTask = Task.Factory.StartNew(hostProcess, cancellationToken);
                            while (!processLaunched.Value)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(1));
                            }
                        }
                    }

                    #endregion

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));

                    if ((i++) % 600 == 0)
                    {
                        info("Running");
                    }
                }
            };

            return Task.Factory.StartNew(hostMonitor, cancellationToken);
        }

        public void Stop()
        {
            // this._onStopRequested = true;

            cancellationTokenSource.Cancel();
        }

        public static class RestartPolicy
        {
            public static Func<bool> MaximumLaunchTimes(int maxCount) 
            {
                int count = 1;
                return () =>
                {
                    return count++ < maxCount;
                };
            }

            public static Func<bool> IgnoreCrashes()
            {
                return () => true;
            }
        }
    }
}



/*

 static void Main(string[] args)
        {
            Func<Process> createProcess = () => {
             var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\util\procexp64.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true, 
                        WindowStyle = ProcessWindowStyle.Normal, 
                        WorkingDirectory = "."
                    },
                    EnableRaisingEvents = true
                };
                return process;
            };

            var cts = new CancellationTokenSource();

            var host = new RestartingProcessHost(createProcess, cts, 
                RestartingProcessHost.RestartPolicy.MaximumLaunchTimes(2)
                // RestartingProcessHost.RestartPolicy.IgnoreCrashes()
            )
            { 
                info = Console.Out.WriteLine,
                warn = Console.Error.WriteLine, 
                error = Console.Error.WriteLine
            }; 
            host.StartRunTask();

            Console.ReadLine();

        }

*/
// @@@ END_INCLUDE: https://raw.github.com/chgeuer/WA.Tomcat.Sample/master/WebRole/RestartingProcessHost.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
namespace Backend.Utils
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    namespace Source.Common
    {
        static class Array<T>
        {
            public static readonly T[] Empty = new T[0];
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
namespace Backend.Utils
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    namespace Source.Common
    {
        using System.Globalization;
    
        sealed partial class InitConfig
        {
            public CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
        }
    
        static partial class Config
        {
            static partial void Partial_Constructed(ref InitConfig initConfig);
    
            public readonly static CultureInfo DefaultCulture;
    
            static Config ()
            {
                var initConfig = new InitConfig();
    
                Partial_Constructed (ref initConfig);
    
                initConfig = initConfig ?? new InitConfig();
    
                DefaultCulture = initConfig.DefaultCulture;
            }
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
namespace Backend.Utils
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    
    namespace Source.Common
    {
        using System;
        using System.Globalization;
    
        static partial class Log
        {
            static partial void Partial_LogLevel (Level level);
            static partial void Partial_LogMessage (Level level, string message);
            static partial void Partial_ExceptionOnLog (Level level, string format, object[] args, Exception exc);
    
            public static void LogMessage (Level level, string format, params object[] args)
            {
                try
                {
                    Partial_LogLevel (level);
                    Partial_LogMessage (level, GetMessage (format, args));
                }
                catch (Exception exc)
                {
                    Partial_ExceptionOnLog (level, format, args, exc);
                }
                
            }
    
            static string GetMessage (string format, object[] args)
            {
                format = format ?? "";
                try
                {
                    return (args == null || args.Length == 0)
                               ? format
                               : string.Format (Config.DefaultCulture, format, args)
                        ;
                }
                catch (FormatException)
                {
    
                    return format;
                }
            }
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
namespace Backend.Utils
{
    // ############################################################################
    // #                                                                          #
    // #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
    // #                                                                          #
    // # This means that any edits to the .cs file will be lost when its          #
    // # regenerated. Changes should instead be applied to the corresponding      #
    // # template file (.tt)                                                      #
    // ############################################################################
    
    
    
    
    
    
    namespace Source.Common
    {
        using System;
    
        partial class Log
        {
            public enum Level
            {
                Success = 1000,
                HighLight = 2000,
                Info = 3000,
                Warning = 10000,
                Error = 20000,
                Exception = 21000,
            }
    
            public static void Success (string format, params object[] args)
            {
                LogMessage (Level.Success, format, args);
            }
            public static void HighLight (string format, params object[] args)
            {
                LogMessage (Level.HighLight, format, args);
            }
            public static void Info (string format, params object[] args)
            {
                LogMessage (Level.Info, format, args);
            }
            public static void Warning (string format, params object[] args)
            {
                LogMessage (Level.Warning, format, args);
            }
            public static void Error (string format, params object[] args)
            {
                LogMessage (Level.Error, format, args);
            }
            public static void Exception (string format, params object[] args)
            {
                LogMessage (Level.Exception, format, args);
            }
    #if !NETFX_CORE && !SILVERLIGHT && !WINDOWS_PHONE
            static ConsoleColor GetLevelColor (Level level)
            {
                switch (level)
                {
                    case Level.Success:
                        return ConsoleColor.Green;
                    case Level.HighLight:
                        return ConsoleColor.White;
                    case Level.Info:
                        return ConsoleColor.Gray;
                    case Level.Warning:
                        return ConsoleColor.Yellow;
                    case Level.Error:
                        return ConsoleColor.Red;
                    case Level.Exception:
                        return ConsoleColor.Red;
                    default:
                        return ConsoleColor.Magenta;
                }
            }
    #endif
            static string GetLevelMessage (Level level)
            {
                switch (level)
                {
                    case Level.Success:
                        return "SUCCESS  ";
                    case Level.HighLight:
                        return "HIGHLIGHT";
                    case Level.Info:
                        return "INFO     ";
                    case Level.Warning:
                        return "WARNING  ";
                    case Level.Error:
                        return "ERROR    ";
                    case Level.Exception:
                        return "EXCEPTION";
                    default:
                        return "UNKNOWN  ";
                }
            }
    
        }
    }
    
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
// ############################################################################

// ############################################################################
namespace Backend.Utils.Include
{
    static partial class MetaData
    {
        public const string RootPath        = @"https://raw.github.com/";
        public const string IncludeDate     = @"2014-05-07T13:58:56";

        public const string Include_0       = @"https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs";
        public const string Include_1       = @"https://raw.github.com/chgeuer/WA.Tomcat.Sample/master/WebRole/RestartingProcessHost.cs";
        public const string Include_2       = @"https://raw.github.com/mrange/T4Include/master/Common/Array.cs";
        public const string Include_3       = @"https://raw.github.com/mrange/T4Include/master/Common/Config.cs";
        public const string Include_4       = @"https://raw.github.com/mrange/T4Include/master/Common/Log.cs";
        public const string Include_5       = @"https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs";
    }
}
// ############################################################################


