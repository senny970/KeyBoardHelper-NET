using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using VideoPlayerController;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Media;
using System.Xml.Serialization;
using AppTerminator;
#if DEBUG
using NLog;
#endif

namespace KeyBHelper
{
    [Serializable]
    public struct Item_t
    {
        public string Text;
        public Keys Key;
        public Keys ModifierKey;
        public string options;
        public string command;
        public int indexOfImage;
    }

    public class KeyBoardHelper
    {
#if DEBUG
        private static Logger log = LogManager.GetCurrentClassLogger();
#endif
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private Form1 MainForm;
        public KeyBoardHelperFeatures KeyBoardHelperFeatures;

        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        public struct AppSettings
        {
            public Keys TerminateKey;
            public Keys TerminateKey_Modifier;
            public Keys WindowsKey;
            public Keys WindowsKey_Modifier;
            public Keys CD_DriveKey;
            public Keys CD_DriveKey_Modifier;
            public Keys CMDKey;
            public Keys CMDKey_Modifier;
            public Keys MaseterVolumeMuteKey;
            public Keys MaseterVolumeMuteKey_Modifier;
            public Keys MaseterVolumeUpKey;
            public Keys MaseterVolumeUpKey_Modifier;
            public Keys MaseterVolumeDownKey;
            public Keys MaseterVolumeDownKey_Modifier;
            public bool bNitifyMsg;
            public bool bSoundNotify;
            public bool CmdEnable;
            public bool bCD_DriveIsOpen;
            public bool switcher_ProcessTerminate;
            public bool switcher_Windows;
            public bool switcher_CD_Drive;
            public bool switcher_MasterVolumeMute;
            public bool switcher_MasterVolumeControl;
            public bool switcher_CMD;
            public int MasterVolumeStep;
        }
                
        public List<Item_t> EventsCollection = new List<Item_t>();
        public Item_t EditedItem;
        public Keys AdittionalEvent_Key = Keys.None;
        public Keys AdittionalEvent_ModiffiersKey = Keys.None;
        public Keys Bind_Key = Keys.None;
        public Keys Bind_ModiffiersKey = Keys.None;
        public int ItemIndexToEdit;
        public string CommandEvent_Name = "";
        public string CommandEvent_Options = "";
        public string ComamndEvent_Command = "";

        public AppSettings app_settings = new AppSettings();
        public static string settingsFile = "/AppSettings.xml";
        public static string EventsFile = "/Events.xml";
        public string AppFolder = "/KeyBoarHelper";
        public static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);        
        
        public bool isKeyInputFormShow = false;
        public bool isCmdFormShow = false;
        public string PerformCommand = "";
        public bool isEditPerformCommandMode = false;
        public bool isEditKeysMode = false;
        public string Command = "";
        public string phrase = "";
        public char word;

        public KeyBoardHelper(Form1 form)
        {
            MainForm = form;
            KeyBoardHelperFeatures = new KeyBoardHelperFeatures(form);
        }

        public void SetupDefaultAppSettings()
        {
            app_settings.TerminateKey = Keys.Add;
            app_settings.TerminateKey_Modifier = Keys.Control;
            app_settings.WindowsKey = Keys.Subtract;
            app_settings.WindowsKey_Modifier = Keys.Control;
            app_settings.CD_DriveKey = Keys.NumPad0;
            app_settings.CD_DriveKey_Modifier = Keys.Control;
            app_settings.CMDKey = Keys.Decimal;
            app_settings.CMDKey_Modifier = Keys.Control;
            app_settings.MaseterVolumeMuteKey = Keys.M;
            app_settings.MaseterVolumeMuteKey_Modifier = Keys.Shift;
            app_settings.MaseterVolumeUpKey = Keys.Add;
            app_settings.MaseterVolumeUpKey_Modifier = Keys.Shift;
            app_settings.MaseterVolumeDownKey = Keys.Subtract;
            app_settings.MaseterVolumeDownKey_Modifier = Keys.Shift;
            app_settings.bNitifyMsg = false;
            app_settings.bSoundNotify = false;
            app_settings.CmdEnable = false;
            app_settings.bCD_DriveIsOpen = false;
            app_settings.switcher_ProcessTerminate = true;
            app_settings.switcher_Windows = true;
            app_settings.switcher_CD_Drive = true;
            app_settings.switcher_MasterVolumeMute = true;
            app_settings.switcher_MasterVolumeControl = true;
            app_settings.switcher_CMD = true;
            app_settings.MasterVolumeStep = 5;
        }

        public string GetSpecialSymbol()
        {
            string s;
            byte[] symbol = new byte[] { 0x22 };
            s = Encoding.ASCII.GetString(symbol);
            return s;
        }

        public bool IsAutorunFlag()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
            int len = Convert.ToString(key.GetValue("AppTerminator")).Length;

            if (len > 0) return true;
            else return false;
        }

        public void SetupSettingFilePath()
        {
            string folder = "/KeyBoarHelper";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!Directory.Exists(path + folder))
                Directory.CreateDirectory(path + folder);
        }

        public string GetSettingFilePath()
        {            
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (Directory.Exists(path + AppFolder))
                return path + AppFolder + settingsFile;
            else
            {
                SetupSettingFilePath();
                GetSettingFilePath();
            }

            return "";
        }

        public void LoadSettingsFile()
        {
            if (!File.Exists(GetSettingFilePath()))
            {
                SetupDefaultAppSettings();

                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (StreamWriter sw = new StreamWriter(GetSettingFilePath()))
                    serializer.Serialize(sw, app_settings);

                LoadSettingsFile();
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (FileStream fs = new FileStream(GetSettingFilePath(), FileMode.Open))
                    app_settings = (AppSettings)serializer.Deserialize(fs);
            }
        }
        
        public void SaveSettingsFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StreamWriter sw = new StreamWriter(GetSettingFilePath()))
                serializer.Serialize(sw, app_settings);
        }

        public void SaveAdditionalItems()
        {
            if (!Directory.Exists(AppData + AppFolder))
                Directory.CreateDirectory(AppData + AppFolder);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Item_t>));
            using (StreamWriter sw = new StreamWriter(AppData + AppFolder + EventsFile))
                serializer.Serialize(sw, EventsCollection);
        }

        public void LoadAdditionalItems()
        {
            if (!Directory.Exists(AppData + AppFolder))
                Directory.CreateDirectory(AppData + AppFolder);

            EventsCollection.Clear();

            if (File.Exists(AppData + AppFolder + EventsFile))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Item_t>));
                using (FileStream fs = new FileStream(AppData + AppFolder + EventsFile, FileMode.Open))
                    EventsCollection = (List<Item_t>)serializer.Deserialize(fs);
            }
            MainForm.AddItemToListView(true);
        }

        //Добавление элемента в список
        public void AddItemToList()
        {
            if (Bind_Key != Keys.None && Bind_ModiffiersKey != Keys.None)
            {
                if (!CheackKeys(Bind_Key, Bind_ModiffiersKey))
                {
                    MessageBox.Show("Клавиша(и) уже зарегистрированы в системе!", "КеуBoard Helper - назначение клавишь", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

                if (!CheackHotKeys(Bind_Key.ToString(), Bind_ModiffiersKey.ToString()))
                return;

            Item_t ItemToList;            
            ItemToList.Text = MainForm.focus_item.Text;
            ItemToList.Key = AdittionalEvent_Key;
            ItemToList.ModifierKey = AdittionalEvent_ModiffiersKey;
            ItemToList.options = "None";
            ItemToList.command = "None";

            switch (MainForm.focus_item.Text)
            {
                case "Открыть папку":
                    ItemToList.options = KeyBoardHelperFeatures.SelectFolder();
                    break;
                case "Открыть файл":
                    ItemToList.options = KeyBoardHelperFeatures.SelectFile();
                    break;
                case "Открыть страницу":
                    MessageBox.Show("Скопируйте текст в буффер обмена, после чего нажимте ОК.", "KeyBoard Helper - вставка ссылки", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ItemToList.options = Clipboard.GetText();
                    break;
                case "Выполнить":
                    MessageBox.Show("Скопируйте команды в буффер обмена, после чего нажимте ОК.", "KeyBoard Helper - вставка команд", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ItemToList.options = Clipboard.GetText();
                    break;
                default:
                    break;
            }

            if(Command.Length > 0)
                ItemToList.command = Command;

            if (!CheackCommand(ItemToList.command))
                return;

            foreach (Item_t it in EventsCollection)
            {
                if (ItemToList.options.Length > 0)
                {
                    if (it.options != "None" && it.options == ItemToList.options)
                    {
                        MessageBox.Show("Событие " + "'" + ItemToList.Text + "'" + " с такими параметрами уже назначено!", "KeyBoaedHelper - добавление события", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            ItemToList.indexOfImage = MainForm.focus_item.ImageIndex;
            EventsCollection.Add(ItemToList);

            MainForm.AddItemToListView(false);
        }

        public string EditOptions(string ItemName)
        {
            string options = "None";

            switch (ItemName)
            {
                case "Открыть папку":
                    options = KeyBoardHelperFeatures.SelectFolder();
                    break;
                case "Открыть файл":
                    options = KeyBoardHelperFeatures.SelectFile();
                    break;
                case "Открыть страницу":
                    MessageBox.Show("Скопируйте текст в буффер обмена, после чего нажимте ОК.", "KeyBoard Helper - вставка ссылки", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    options = Clipboard.GetText();
                    break;
                case "Выполнить":
                    MessageBox.Show("Скопируйте команды в буффер обмена, после чего нажимте ОК.", "KeyBoard Helper - вставка команд", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    options = Clipboard.GetText();
                    break;
                default:
                    options = "None";
                    break;
            }

            return options;
        }

        public void RunEvent(string ev, string options)
        {
            switch (ev)
            {
                case "Открыть папку":
                    KeyBoardHelperFeatures.OpenFolder(options);
                    break;
                case "Открыть панель управления":
                    KeyBoardHelperFeatures.OpenControlPanel();
                    break;
                case "Удаление программ":
                    KeyBoardHelperFeatures.OpenAppUninstaller();
                    break;
                case "Открыть файл":
                    KeyBoardHelperFeatures.OpenFile(options);
                    break;
                case "Открыть страницу":
                    KeyBoardHelperFeatures.OpenUrl(options);
                    break;
                case "Открыть удаление программ":
                    KeyBoardHelperFeatures.OpenAppUninstaller();
                    break;
                case "Завершение работы":
                    KeyBoardHelperFeatures.PowerOff();
                    break;
                case "Перезагрузка":
                    KeyBoardHelperFeatures.Reboot();
                    break;
                case "Сон":
                    KeyBoardHelperFeatures.Hibernate();
                    break;
                case "Редактор реэстра":
                    KeyBoardHelperFeatures.RegEditor();
                    break;
                case "Выполнить":
                    KeyBoardHelperFeatures.PerformCommand(options);
                    break;
                default:
                    break;
            }
        }

        public bool CheackHotKeys(string MainKey, string Modifier)
        {
            foreach (Item_t it in EventsCollection)
            {
                if (it.Key.ToString() == MainKey && Modifier == Keys.None.ToString() && it.Key.ToString() != Keys.None.ToString())
                {
                    MessageBox.Show("Клавиша " + MainKey.ToString() + " уже назначена на - " + "'" + it.Text + "'" + " !", "KeyBoaedHelper - проверка сочетания клавишь", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (it.Key.ToString() == MainKey && it.ModifierKey.ToString() == Modifier && it.Key.ToString() != Keys.None.ToString())
                {
                    MessageBox.Show("Сочетание клавишь " + Modifier.ToString() + " + " + MainKey.ToString() + " уже назначено на - " + "'" + it.Text + "'" + " !", "KeyBoaedHelper - проверка сочетания клавишь", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        public bool CheackCommand(string command)
        {
            foreach (Item_t it in EventsCollection)
            {
                if (it.command != "None" && it.command == command)
                {
                    MessageBox.Show("Команда " + command + " уже назначена на - " + "'" + it.Text + "'" + " !", "KeyBoaedHelper - проверка команды", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        public bool CheackKeys(Keys key, Keys KeyModifier)
        {
            //Тип: HWND
            //Обработчик окна ассоциированный с горячей клавишей, который должен быть освобождён.
            //Этот параметр должен быть NULL, если не ассоциирован с окном!!!

            /*None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8*/

            KeyModifiers mod = KeyModifiers.None;

            switch (KeyModifier)
            {
                case Keys.None:
                    mod = KeyModifiers.None;
                    break;
                case Keys.Alt:
                    mod = KeyModifiers.Alt;
                    break;
                case Keys.Control:
                    mod = KeyModifiers.Control;
                    break;
                case Keys.Shift:
                    mod = KeyModifiers.Shift;
                    break;
                case Keys.LWin:
                    mod = KeyModifiers.Windows;
                    break;
                case Keys.RWin:
                    mod = KeyModifiers.Windows;
                    break;
                default:
                    break;
            }

            if (!RegisterHotKey(IntPtr.Zero, 0, mod, key))
            {
                //MessageBox.Show(new Win32Exception(Marshal.GetLastWin32Error()).Message, "KeyBoard Helper - проверка клавишь", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            else
            {
                //UnregisterHotKey(IntPtr.Zero, 0);
                return true;
            }
        }

        //--------------//
    }

    public class KeyBoardHelperFeatures
    {
        public AudioManager audioMgr;

        Form1 MainForm;
        public KeyBoardHelperFeatures(Form1 form)
        {
            MainForm = form;
            audioMgr = new AudioManager();
        }

        protected SoundPlayer sp;
        protected Process[] proc;
        public int Driveresult;

        //MessageBox флаги.
        const int MB_YESNO = 0x00000004;
        const int MB_ICONQUESTION = 0x00000020;

        //ShowWindowAsync флаги.
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        //WinApi импорт.
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        protected static extern int mciSendString (string mciCommand, StringBuilder returnValue, int returnLength, IntPtr callback);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop,
        EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd,
        StringBuilder lpWindowText, int nMaxCount);

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        public String GetActiveWindowName()
        {
            IntPtr h;
            int pid = 0;
            h = GetForegroundWindow();
            GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            return p.ProcessName;
        }

        public String GetActiveWindowProcessName()
        {
            IntPtr h = IntPtr.Zero;
            int pid = 0;
            h = GetForegroundWindow();
            GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            return p.ProcessName;
        }

        public bool CheakActiveWindowProcess()
        {
            bool cheak_result = false;
            IntPtr h = IntPtr.Zero;
            int pid = 0;
            h = GetForegroundWindow();
            GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            if (p.ProcessName != "explorer" && p.ProcessName != "AppTerminator")
            {
                cheak_result = true;
            }
            return cheak_result;
        }

        public void TerminateActiveWindowProcess()
        {
            IntPtr h = IntPtr.Zero;
            int pid = 0;
            h = GetForegroundWindow();
            GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            if (p.ProcessName != "explorer" && p.ProcessName != "AppTerminator")
                p.Kill();
        }

        public void PlayWindowSound()
        {
            if (File.Exists("sound/nitify.wav"))
            {
                sp = new SoundPlayer(@"sound/nitify.wav");
                sp.Play();
            }
        }

        public void PlayProcessSound()
        {
            if (File.Exists("sound/process.wav"))
            {
                sp = new SoundPlayer(@"sound/process.wav");
                sp.Play();
            }
        }

        public void PlayButtonSwitchSound()
        {
            if (File.Exists("sound/switch.wav"))
            {
                sp = new SoundPlayer(@"sound/switch.wav");
                sp.Play();
            }
        }

        private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            if (hWnd != GetDesktopWindow())
            {
                if (IsWindowVisible(hWnd) & IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }

                if (IsWindowVisible(hWnd) & !IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, SW_MINIMIZE);
                }
            }

            return true;
        }

        public void MinimizeOpenWindows()
        {
            EnumDelegate delEnumfunc = new EnumDelegate(EnumWindowsProc);
            EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero);
        }

        public void CD_DriveOpen(bool state)
        {
            if (state)
                Driveresult = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
            else
                Driveresult = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
        }

        public bool ShowTopQuestionMessageBox()
        {
            IntPtr h = IntPtr.Zero;
            int result;
            int pid = 0;
            h = GetForegroundWindow();
            GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            result = MessageBox(h, "Завершить " + p.ProcessName + "?", "KeyBoard Helper", MB_YESNO | MB_ICONQUESTION);
            if (result == 6) return true;
            else
                return false;
        }

        public bool SetForegdWindow(IntPtr windowHandle)
        {
            return SetForegroundWindow(windowHandle);
        }

        public void OpenUrl(string url)
        {
            Process.Start(url);
        }

        public void OpenAppUninstaller()
        {
            Process.Start("appwiz.cpl");
        }

        public void OpenControlPanel()
        {
            Process.Start("control");
        }

        public void OpenFile(string file)
        {
            if (System.IO.File.Exists(file))
                Process.Start(file);
        }

        public void OpenFolder(string folder)
        {
            if (System.IO.Directory.Exists(folder))
                Process.Start(folder);
        }

        public string SelectFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return "null";
        }

        public string SelectFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult SelectFolderResult = dialog.ShowDialog();
            if (SelectFolderResult == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else
            {
                if (SelectFolderResult == DialogResult.Cancel)
                {
                    dialog.Dispose();
                    DialogResult MBresult;
                    MBresult = System.Windows.Forms.MessageBox.Show("Вы отменили выбор папки.\nЗадать путь папки с буффера обмена?", " KeyBoaedHelper - Выбор папки", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (MBresult == DialogResult.Yes)
                    {
                        return Clipboard.GetText();
                    }
                }
            }
            return "null";
        }

        /*public string SelectFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "null";
        }*/

        public void PowerOff()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        public void Reboot()
        {
            Process.Start("shutdown", "/r /t 0");
        }


        public void RegEditor()
        {
            Process.Start("regedit");
        }

        public void MuteSound()
        {
            audioMgr.SetMasterVolumeMute(!audioMgr.GetMasterVolumeMute());
            if (audioMgr.GetMasterVolumeMute())
            {

                MainForm.notifiIcon1.BalloonTipText = "Звук отключен!";
                MainForm.notifiIcon1.ShowBalloonTip(500);
            }
            else
            {
                MainForm.notifiIcon1.BalloonTipText = "Звук включен!";
                MainForm.notifiIcon1.ShowBalloonTip(500);
            }
        }

        public void MasterVolumeUP()
        {
            int MasterVolume = Convert.ToInt32(audioMgr.GetMasterVolume());
            int NewMasterVolume = MasterVolume + Convert.ToInt32(MainForm.numericUpDown1.Value);

            if (MasterVolume < 100)
            {
                if (NewMasterVolume > 100)
                    NewMasterVolume = 100;
                audioMgr.SetMasterVolume(NewMasterVolume);               
            }
        }

        public void MasterVolumeDown()
        {
            int MasterVolume = Convert.ToInt32(audioMgr.GetMasterVolume());            
            int NewMasterVolume = MasterVolume - Convert.ToInt32(MainForm.numericUpDown1.Value);

            if (MasterVolume > 0)
            {
                if (NewMasterVolume < 0)
                    NewMasterVolume = 0;
                audioMgr.SetMasterVolume(NewMasterVolume);
            }
        }

        public void Hibernate()
        {
            bool isHibernate = Application.SetSuspendState(PowerState.Hibernate, false, false);
            if (isHibernate == false)
                System.Windows.Forms.MessageBox.Show("Ошибка перевода системы в режим Сон!", "KeyBoard Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void PerformCommand(string command)
        {
            if (command.Contains(" "))
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.Arguments = "/C " + command;
                cmd.Start();
            }
            else
            {
                System.Diagnostics.Process.Start(command);
            }
        }
    }
}