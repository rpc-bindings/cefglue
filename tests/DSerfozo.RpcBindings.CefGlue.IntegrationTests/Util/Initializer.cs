using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util
{
    public class Initializer : IDisposable
    {
        public class TestBrowserProcessHandler : CefBrowserProcessHandler
        {
            protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine)
            {
                //commandLine.AppendArgument("--renderer-startup-dialog");
                //commandLine.AppendArgument("--gpu-startup-dialog");
                commandLine.AppendArgument("--disable-gpu");
                commandLine.AppendArgument("--disable-gpu-compositing");
            }
        }

        public class TestApp : CefApp
        {
            private readonly TestBrowserProcessHandler browserProcessHandler = new TestBrowserProcessHandler();
            protected override CefBrowserProcessHandler GetBrowserProcessHandler()
            {
                return browserProcessHandler;
            }
        }

        private readonly AutoResetEvent stopEvent;

        public Initializer()
        {
            var settings = new CefSettings
            {
                MultiThreadedMessageLoop = true,
                SingleProcess = false,
                LogSeverity = CefLogSeverity.Error,
                LogFile = "cef.log",
                ResourcesDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                BrowserSubprocessPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DSerfozo.CefGlue.SubProcess.exe"),
                RemoteDebuggingPort = 20480,
                WindowlessRenderingEnabled = true,
                NoSandbox = true,
            };

            var startEvent = new AutoResetEvent(false);
            stopEvent = new AutoResetEvent(false);
            var app = new TestApp();
            var cefMainThread = new Thread(() =>
            {
                CefRuntime.Initialize(new CefMainArgs(new string[]{}), settings, app, IntPtr.Zero);
                startEvent.Set();
                stopEvent.WaitOne();
                CefRuntime.Shutdown();
                stopEvent.Set();
            });
            cefMainThread.SetApartmentState(ApartmentState.STA);
            cefMainThread.IsBackground = true;
            cefMainThread.Start();

            startEvent.WaitOne();
        }

        public void Dispose()
        {
            stopEvent.Set();
            stopEvent.WaitOne();
        }
    }
}
