using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.ComponentModel;
using UiTools.WinForms.Designer.Core;

namespace UiTools.WinForms.Designer
{
    internal static class Program
    {
        /*
          OPERATIONAL LOGIC:
          We have 4 scenarios – described in "Given/When/Then" style:
          1. When the initiator is the user, not the VSCode extension. In this case, no any data transfer between UiTools.WinForms.Designer instances is expected –
             only protection against launching a second instance is needed:
            1.1. Given: UiTools.WinForms.Designer is not yet running at all
                 When:  The user launches UiTools.WinForms.Designer.exe in Windows Explorer
                 Then:  The first instance starts, and its PipeServer begins listening on the channel (solely for the sake of VS Code, i.e. scenario #2).
            1.2. Given: The first instance was already launched by the user previously (by calling UiTools.WinForms.Designer.exe in Windows Explorer)
                 When:  The user tries to launch a second instance (in the same manner – by calling UiTools.WinForms.Designer.exe in Windows Explorer)
                 Then:  This second instance activates the window of the first (already running) instance and self-terminates. NO data transfer (from the second
                        instance to the first) is required here (and this is important! because all data will be entered by the user into the UI of the RUNNING designer).
                        The PipeServer is not used here at all.
          2. When the initiator is the VSCode extension:
            2.1. Given: UiTools.WinForms.Designer is not yet running at all
                 When:  The VSCode extension wants to open a form in UiTools.WinForms.Designer
                 Then:  It attempts – as a Named Pipes Client – to send a message to the channel but, obviously, receives an error because UiTools.WinForms.Designer
                        is NOT running, and, consequently, the PipeServer is not yet up. In such a case (and only then), the VSCode extension must launch
                        UiTools.WinForms.Designer.exe (from the "VSIX-bin" folder) and after that, attempt to send the message to the "channel" AGAIN (and this time – it
                        should succeed; if it doesn't succeed this time either, the VSCode extension must show an error message).
            2.2. Given: The first instance was already launched previously – either by the user (scenario 1.1 above) or from VS Code (scenario 2.1 above); its PipeServer
                        is running
                 When:  The VSCode extension wants to open a form in UiTools.WinForms.Designer
                 Then:  It attempts – as a Named Pipes Client – to send a message to the channel and (theoretically) it should succeed, because the first (and only)
                        instance of UiTools.WinForms.Designer is already running, and, consequently, the PipeServer is up and listening on the channel. In this situation,
                        the first instance of UiTools.WinForms.Designer processes the message received from the channel. No second instance even appears on the scene.
          Hence, it follows that protection against launching a second instance is relevant only for section 1 (when the initiator is the user), since
          in section 2 – the VSCode extension does not even attempt to launch a SECOND instance of UiTools.WinForms.Designer: instead, it looks for the server
          to send it a message, and ONLY if the server is not found – then it will try to launch its instance (the FIRST one).
        */
        private static Mutex mutex = new Mutex(true, "17C99F4D-E451-41F5-B2A0-60B0B4768F76");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true)) // allow only single instance
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                // In our ExceptionViewer we want to see inner exceptions *expanded* and view StackTrace in a separate window:
                TypeDescriptor.AddAttributes(typeof(Exception), new TypeConverterAttribute(typeof(ExceptionWithStackTraceConverter)));

                // We are the first instance! Start the pipe server in the background:
                PipeServer.StartListening();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                FixMenuStripDpiBug();
                Application.Run(new MainForm(TryGetVsixVersion(args)));
                mutex.ReleaseMutex();
            }
            else
            {
                // We are the second instance. Send a command to the first instance to activate its main window, then terminate:
                PipeClient.SendMessage(PipeServer.ACTIVATION_COMMAND);
                Environment.Exit(0);
            }
        }

        private static string TryGetVsixVersion(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var currentArg = args[i];
                if (string.Equals(currentArg, "--vsix-version", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                        return args[i + 1];
                }
            }
            return null;
        }

        private static void FixMenuStripDpiBug()
        {
            /*
             * NOTE: without this "hack" we get NRE in MiniToolStripRenderer.OnRenderArrow() method which occurs when we put mouse pointer over the
             *       ToolStripTemplateNode (combobox with "Type here" cue banner) - but only if we have "<add key="DpiAwareness" value="PerMonitorV2"/>"
             *       line in our App.config file:
             *           protected virtual void OnRenderArrow(ToolStripArrowRenderEventArgs e)
             *           {
             *               ...
             *               if (e.Item?.DeviceDpi != previousDeviceDpi && System.Windows.Forms.DpiHelper.EnableToolStripPerMonitorV2HighDpiImprovements)
             *               {
             *                   previousDeviceDpi = e.Item.DeviceDpi; // << NRE occurs here
             *                   ScaleArrowOffsetsIfNeeded(e.Item.DeviceDpi);
             *               }
             *           }
             *       Getter of the static property EnableToolStripPerMonitorV2HighDpiImprovements looks like this (sources of the DpiHelper class):
             *           private static bool enableToolStripHighDpiImprovements = false;
             *           private static bool enableDpiChangedHighDpiImprovements = false;
             *           ...
             *           internal static bool EnableToolStripPerMonitorV2HighDpiImprovements
             *           {
             *               get
             *               {
             *                   if (EnableDpiChangedMessageHandling && enableToolStripHighDpiImprovements)
             *                   {
             *                       return enableDpiChangedHighDpiImprovements;
             *                   }
             *                   return false;
             *               }
             *           }
             *       Setting enableToolStripHighDpiImprovements field to false - fixes NRE because EnableToolStripPerMonitorV2HighDpiImprovements
             *       also becomes false.
             *       
             *       Seems it's a bug in the ToolStripRenderer.OnRenderArrow() - e.Item is checked for null only inside the if statement and is not
             *       checked further on.
             *       
             * MYSTIC: it's not enough to just set enableToolStripHighDpiImprovements field to false - I must also READ
             *         EnableToolStripPerMonitorV2HighDpiImprovements property at least once, otherwise it has no effect! Looks like kicking the
             *         static ctor of the static DpiHelper class but... this class has no static ctor defined. No ideas.
             */
            var dpiHelperType = typeof(Control).Assembly.GetType("System.Windows.Forms.DpiHelper");
            var pi = dpiHelperType.GetProperty("EnableToolStripPerMonitorV2HighDpiImprovements", BindingFlags.Static | BindingFlags.NonPublic);
            var _ = pi.GetValue(null); // << mystic is here :) and it doesn't matter if I place this line before or after fi.SetValue()!
            //Debug.WriteLine("Before: " + pi.GetValue(null).ToString());
            var fi = dpiHelperType.GetField("enableToolStripHighDpiImprovements", BindingFlags.Static | BindingFlags.NonPublic);
            if (fi != null)
                fi.SetValue(null, false);
            //Debug.WriteLine("After: " + pi.GetValue(null).ToString());
        }
    }

    internal static class PipeServer
    {
        public const string ACTIVATION_COMMAND = "ACTIVATE_MAINFORM";
        public const string PIPE_NAME = "UiToolsWinFormsDesignerPipe"; // also used in extension.ts, function sendToDesigner()

        public static void StartListening()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            var message = reader.ReadToEnd();
                            HandleIncomingMessage(message);
                        }
                    }
                }
            });
        }

        private static void HandleIncomingMessage(string message)
        {
            var mainForm = Application.OpenForms[nameof(MainForm)] as MainForm;
            if (mainForm == null)
            {
                MessageBox.Show("Main form not found!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            mainForm.Invoke(() =>
            {
                if (message == ACTIVATION_COMMAND)
                {
                    if (mainForm.WindowState == FormWindowState.Minimized)
                        mainForm.WindowState = FormWindowState.Normal;
                    mainForm.Activate();
                }
                else // message arrived from VSCode extension and contains full path to .designer.cs file
                {
                    if (mainForm.WindowState == FormWindowState.Minimized)
                        mainForm.WindowState = FormWindowState.Normal;
                    mainForm.Activate();
                    mainForm.OpenExistingDesignerFileFromVsCode(message);
                }
            });
        }
    }

    internal static class PipeClient
    {
        public static void SendMessage(string message)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeServer.PIPE_NAME, PipeDirection.Out))
                {
                    client.Connect(1000);
                    using (var writer = new StreamWriter(client))
                    {
                        writer.Write(message);
                        writer.Flush();
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Pipe server not available or timed out.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}
