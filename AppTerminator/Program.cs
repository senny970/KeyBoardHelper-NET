using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
#if DEBUG
using NLog;
#endif

namespace AppTerminator
{
    static class Program
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //Проверка запущенного екземпляра приложения.
        public static bool HasPriorInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string fileName = currentProcess.StartInfo.FileName;
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id == currentProcess.Id) { continue; }
                if (process.StartInfo.FileName != fileName) { continue; }
                SetForegroundWindow(process.MainWindowHandle);
                MessageBox.Show("Экземпляр приложения уже запущен!", "KeyBoard Helper", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

#if DEBUG
            private static Logger log = LogManager.GetCurrentClassLogger();
#endif
        [STAThread]
        static void Main(string[] args)
        {
            if (!HasPriorInstance())
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                ApplicationContext AppContext = new ApplicationContext(new Form1());
                if (args.Length > 0)
                {
                    if (args[0] == "-a")
                    {
                        AppContext.MainForm.Hide();
                        AppContext.MainForm.WindowState = FormWindowState.Minimized;
                        AppContext.MainForm.ShowInTaskbar = false;
                    }
                }
                Application.Run(AppContext);

#if DEBUG
                log.Debug("Program::Main");
#endif
            }
        }
    }
}
