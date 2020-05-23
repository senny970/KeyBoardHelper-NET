using System;
using System.ComponentModel;
using System.Windows.Forms;
using KeyBHelper;
using Microsoft.Win32;

#if DEBUG
using NLog;
#endif

namespace AppTerminator
{

    public partial class Form1 : Form
    {
#if DEBUG
            private static Logger log = LogManager.GetCurrentClassLogger();
#endif
        public GlobalHook KeyHook;
        public KeyBoardHelper keyboardHelper;
        public Info InfoForm;
        public KeyInput KeyInputForm;
        public CommandInput CommandInputForm;

        public ListViewItem focus_item = null;
        public string ProcName = "";

        private void OnPowerModeChange(object s, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                KeyHook = new GlobalHook();
                keyboardHelper = new KeyBoardHelper(this);
                KeyHook.KeyDown += OnKeyDownEvent_Global;
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            keyboardHelper.SaveAdditionalItems();
            keyboardHelper.SaveSettingsFile();
        }

        private void ShowTip(string msg, string title, Control obj)
        {
            toolTip1.SetToolTip(obj, msg);
            toolTip1.ToolTipIcon = ToolTipIcon.Info;
            toolTip1.ToolTipTitle = title;
            toolTip1.IsBalloon = true;
        }

        private void OnKeyDownEvent_Global(object s, KeyEventArgs ev)
        {
            //Commands
            if (keyboardHelper.app_settings.CmdEnable)
            {
                if (ev.KeyCode == Keys.Space)
                {
                    keyboardHelper.phrase = keyboardHelper.phrase + " ";
                }

                if (char.TryParse(ev.KeyCode.ToString(), out keyboardHelper.word))
                    keyboardHelper.phrase = (keyboardHelper.phrase + keyboardHelper.word.ToString()).ToLower();

                if (ev.KeyCode.ToString().Contains("D"))
                {
                    keyboardHelper.phrase = keyboardHelper.phrase + ev.KeyCode.ToString().Replace("D", "");
                }

                if (ev.KeyCode.ToString().Contains("NumPad"))
                {
                    keyboardHelper.phrase = keyboardHelper.phrase + ev.KeyCode.ToString().Replace("NumPad", "");
                }

                if (keyboardHelper.phrase.Length > 20)
                    keyboardHelper.phrase = "";

                //Command enter debug
                //notifiIcon1.BalloonTipText = keyboardHelper.phrase;
                //notifiIcon1.ShowBalloonTip(500);

                foreach (Item_t it in keyboardHelper.EventsCollection)
                {
                    if (keyboardHelper.phrase.Contains(it.command + " "))
                    {
                        keyboardHelper.RunEvent(it.Text, it.options);
                        keyboardHelper.CommandEvent_Name = it.Text;
                        keyboardHelper.CommandEvent_Options = it.options;
                        keyboardHelper.ComamndEvent_Command = it.command;

                        keyboardHelper.phrase = "";
                    }
                }

                if (keyboardHelper.CommandEvent_Name.Length > 0)
                {
                    if (keyboardHelper.app_settings.bNitifyMsg)
                    {
                        if (keyboardHelper.CommandEvent_Options.Length > 0 && keyboardHelper.CommandEvent_Options != "None")
                        {
                            notifiIcon1.BalloonTipText = "Команда: " + keyboardHelper.ComamndEvent_Command + "\n" + keyboardHelper.CommandEvent_Name + ": " + keyboardHelper.CommandEvent_Options;
                            notifiIcon1.ShowBalloonTip(500);
                        }
                        else
                        {
                            notifiIcon1.BalloonTipText = "Команда: " + keyboardHelper.ComamndEvent_Command + "\n" + keyboardHelper.CommandEvent_Name;
                            notifiIcon1.ShowBalloonTip(500);
                        }

                        keyboardHelper.phrase = "";
                        keyboardHelper.CommandEvent_Name = "";
                        keyboardHelper.CommandEvent_Options = "";
                        keyboardHelper.ComamndEvent_Command = "";
                    }
                }
            }

            //BindKeys
            if (KeyInputForm != null && ev.KeyCode != Keys.Enter)
            {
                if (KeyInputForm.Created)
                {
                    if (Control.ModifierKeys == Keys.None)
                    {
                        keyboardHelper.Bind_Key = ev.KeyCode;
                        keyboardHelper.Bind_ModiffiersKey = Keys.None;
                        KeyInputForm.SetTextBoxText(Convert.ToString(ev.KeyCode));
                    }
                    else
                    {
                        keyboardHelper.Bind_Key = ev.KeyCode;
                        keyboardHelper.Bind_ModiffiersKey = Control.ModifierKeys;
                        KeyInputForm.SetTextBoxText(Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode));
                    }
                }
            }

            //Дополнительные события
            if (KeyInputForm != null && ev.KeyCode != Keys.Enter)
            {
                if (KeyInputForm.Created)
                {
                    if (Control.ModifierKeys == Keys.None)
                    {
                        keyboardHelper.AdittionalEvent_Key = ev.KeyCode;
                        keyboardHelper.AdittionalEvent_ModiffiersKey = Keys.None;
                        KeyInputForm.SetTextBoxText(Convert.ToString(ev.KeyCode));
                    }
                    else
                    {
                        keyboardHelper.AdittionalEvent_Key = ev.KeyCode;
                        keyboardHelper.AdittionalEvent_ModiffiersKey = Control.ModifierKeys;
                        KeyInputForm.SetTextBoxText(Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode));
                    }
                }
            }

            foreach (Item_t it in keyboardHelper.EventsCollection)
            {
                if (ev.KeyCode == it.Key
                    && Control.ModifierKeys == it.ModifierKey
                    && !keyboardHelper.isCmdFormShow
                    && !keyboardHelper.isKeyInputFormShow)
                {                    
                    keyboardHelper.RunEvent(it.Text, it.options);
                }
            }

            //Завершение процессов.
            if (checkBox1.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.TerminateKey = ev.KeyCode;
                    keyboardHelper.app_settings.TerminateKey_Modifier = Keys.None;
                    textBox1.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.TerminateKey = ev.KeyCode;
                    keyboardHelper.app_settings.TerminateKey_Modifier = Control.ModifierKeys;
                    textBox1.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.TerminateKey && Control.ModifierKeys == keyboardHelper.app_settings.TerminateKey_Modifier && !checkBox1.Checked && keyboardHelper.app_settings.switcher_ProcessTerminate && keyboardHelper.KeyBoardHelperFeatures.CheakActiveWindowProcess())
            {
                if (keyboardHelper.KeyBoardHelperFeatures.ShowTopQuestionMessageBox())
                {
                    ProcName = keyboardHelper.KeyBoardHelperFeatures.GetActiveWindowName();
                    ProcName = ProcName.Substring(0, 1).ToUpper() + ProcName.Remove(0, 1);
                    keyboardHelper.KeyBoardHelperFeatures.TerminateActiveWindowProcess();
                    notifiIcon1.BalloonTipText = ProcName + " завершен!";

                    if (checkBox4.Checked)
                    {
                        keyboardHelper.KeyBoardHelperFeatures.PlayProcessSound();
                    }

                    if (checkBox3.Checked)
                    {
                        notifiIcon1.ShowBalloonTip(500);
                    }
                }
            }

            //Сворачивание окон.
            if (checkBox8.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    //keyboardHelper.app_settings.app_settings.
                    keyboardHelper.app_settings.WindowsKey = ev.KeyCode;
                    keyboardHelper.app_settings.WindowsKey_Modifier = Keys.None;
                    textBox3.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.WindowsKey = ev.KeyCode;
                    keyboardHelper.app_settings.WindowsKey_Modifier = Control.ModifierKeys;
                    textBox3.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.WindowsKey && Control.ModifierKeys == keyboardHelper.app_settings.WindowsKey_Modifier && !checkBox8.Checked && keyboardHelper.app_settings.switcher_Windows)
            {
                keyboardHelper.KeyBoardHelperFeatures.MinimizeOpenWindows();

                if (checkBox4.Checked)
                {
                    keyboardHelper.KeyBoardHelperFeatures.PlayWindowSound();
                }
            }

            //Управление лотком дисковода.
            if (checkBox7.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.CD_DriveKey = ev.KeyCode;
                    keyboardHelper.app_settings.CD_DriveKey_Modifier = Keys.None;
                    textBox2.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.CD_DriveKey = ev.KeyCode;
                    keyboardHelper.app_settings.CD_DriveKey_Modifier = Control.ModifierKeys;
                    textBox2.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.CD_DriveKey && Control.ModifierKeys == keyboardHelper.app_settings.CD_DriveKey_Modifier && !checkBox7.Checked && keyboardHelper.app_settings.switcher_CD_Drive)
            {
                if (!keyboardHelper.app_settings.bCD_DriveIsOpen)
                {
                    keyboardHelper.KeyBoardHelperFeatures.CD_DriveOpen(true);
                    keyboardHelper.app_settings.bCD_DriveIsOpen = true;

                    if (checkBox3.Checked)
                    {
                        notifiIcon1.BalloonTipText = "Дисковод открыт!";
                        notifiIcon1.ShowBalloonTip(500);
                    }
                }

                else
                {
                    keyboardHelper.KeyBoardHelperFeatures.CD_DriveOpen(false);
                    keyboardHelper.app_settings.bCD_DriveIsOpen = false;

                    if (checkBox3.Checked)
                    {
                        notifiIcon1.BalloonTipText = "Дисковод закрыт!";
                        notifiIcon1.ShowBalloonTip(500);
                    }
                }
            }

            //Вызов командной строки.
            if (checkBox5.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.CMDKey = ev.KeyCode;
                    keyboardHelper.app_settings.CMDKey_Modifier = Keys.None;
                    textBox4.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.CMDKey = ev.KeyCode;
                    keyboardHelper.app_settings.CMDKey_Modifier = Control.ModifierKeys;
                    textBox4.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.CMDKey && Control.ModifierKeys == keyboardHelper.app_settings.CMDKey_Modifier && !checkBox5.Checked && keyboardHelper.app_settings.switcher_CMD)
            {
                keyboardHelper.KeyBoardHelperFeatures.MinimizeOpenWindows();
                System.Diagnostics.Process.Start("cmd.exe");
            }

            //Отключение звука.
            if (checkBox9.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.MaseterVolumeMuteKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeMuteKey_Modifier = Keys.None;
                    textBox6.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.MaseterVolumeMuteKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeMuteKey_Modifier = Control.ModifierKeys;
                    textBox6.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.MaseterVolumeMuteKey && Control.ModifierKeys == keyboardHelper.app_settings.MaseterVolumeMuteKey_Modifier && !checkBox9.Checked && keyboardHelper.app_settings.switcher_MasterVolumeControl)
            {
                keyboardHelper.KeyBoardHelperFeatures.MuteSound();
            }

            //Управление громкостью. (Volume+)
            if (checkBox6.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.MaseterVolumeUpKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeUpKey_Modifier = Keys.None;
                    textBox5.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.MaseterVolumeUpKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeUpKey_Modifier = Control.ModifierKeys;
                    textBox5.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.MaseterVolumeUpKey && Control.ModifierKeys == keyboardHelper.app_settings.MaseterVolumeUpKey_Modifier && !checkBox6.Checked && !checkBox11.Checked && keyboardHelper.app_settings.switcher_MasterVolumeMute)
            {
                keyboardHelper.KeyBoardHelperFeatures.MasterVolumeUP();
            }

            //Управление громкостью. (Volume-)
            if (checkBox10.Checked)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    keyboardHelper.app_settings.MaseterVolumeDownKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeDownKey_Modifier = Keys.None;
                    textBox7.Text = Convert.ToString(ev.KeyCode);
                }
                else
                {
                    keyboardHelper.app_settings.MaseterVolumeDownKey = ev.KeyCode;
                    keyboardHelper.app_settings.MaseterVolumeDownKey_Modifier = Control.ModifierKeys;
                    textBox7.Text = Convert.ToString((Keys)Control.ModifierKeys) + " + " + Convert.ToString((Keys)ev.KeyCode);
                }
            }

            if (ev.KeyCode == keyboardHelper.app_settings.MaseterVolumeDownKey && Control.ModifierKeys == keyboardHelper.app_settings.MaseterVolumeDownKey_Modifier && !checkBox10.Checked && !checkBox11.Checked && keyboardHelper.app_settings.switcher_MasterVolumeMute)
            {
                keyboardHelper.KeyBoardHelperFeatures.MasterVolumeDown();
            }

            //Удаление дополнительных событий.
            if (ev.KeyCode == Keys.Delete
                && this.Visible
                && !keyboardHelper.isKeyInputFormShow)
            {
                DeleteItem();
            }
        }

        //--------------------------------TAB2-------------------------------//

        void EventsCollectionInit()
        {
            //Элементы listView1
            ListViewItem OpenFolder = new ListViewItem();
            OpenFolder.Text = "Открыть папку"; //SubItems[0]
            ListViewItem.ListViewSubItem OpenFolder_key = new ListViewItem.ListViewSubItem(); //Key SubItems[1]
            ListViewItem.ListViewSubItem OpenFolder_ModiffierKey = new ListViewItem.ListViewSubItem(); //ModiffierKey SubItems[2]
            ListViewItem.ListViewSubItem OpenFolder_options = new ListViewItem.ListViewSubItem(); //Параметры SubItems[3]
            OpenFolder.SubItems.Add(OpenFolder_key);
            OpenFolder.SubItems.Add(OpenFolder_ModiffierKey);
            OpenFolder.SubItems.Add(OpenFolder_options);
            OpenFolder.ImageIndex = 0;
            listView1.Items.Add(OpenFolder);

            ListViewItem OpenControlPanel = new ListViewItem();
            OpenControlPanel.Text = "Открыть панель управления";
            ListViewItem.ListViewSubItem OpenControlPanel_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenControlPanel_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenControlPanel_options = new ListViewItem.ListViewSubItem();
            OpenControlPanel.SubItems.Add(OpenControlPanel_key);
            OpenControlPanel.SubItems.Add(OpenControlPanel_ModiffierKey);
            OpenControlPanel.SubItems.Add(OpenControlPanel_options);
            OpenControlPanel.ImageIndex = 1;
            listView1.Items.Add(OpenControlPanel);

            ListViewItem AppUninstaller = new ListViewItem();
            AppUninstaller.Text = "Удаление программ";
            ListViewItem.ListViewSubItem AppUninstaller_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem AppUninstaller_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem AppUninstaller_option = new ListViewItem.ListViewSubItem();
            AppUninstaller.SubItems.Add(AppUninstaller_key);
            AppUninstaller.SubItems.Add(AppUninstaller_ModiffierKey);
            AppUninstaller.SubItems.Add(AppUninstaller_option);
            AppUninstaller.ImageIndex = 2;
            listView1.Items.Add(AppUninstaller);

            ListViewItem OpenUrl = new ListViewItem();
            OpenUrl.Text = "Открыть страницу";
            ListViewItem.ListViewSubItem OpenUrl_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenUrl_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenUrl_option = new ListViewItem.ListViewSubItem();
            OpenUrl.SubItems.Add(OpenUrl_key);
            OpenUrl.SubItems.Add(OpenUrl_ModiffierKey);
            OpenUrl.SubItems.Add(OpenUrl_option);
            OpenUrl.ImageIndex = 3;
            listView1.Items.Add(OpenUrl);

            ListViewItem OpenFile = new ListViewItem();
            OpenFile.Text = "Открыть файл";
            ListViewItem.ListViewSubItem OpenFile_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenFile_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem OpenFile_option = new ListViewItem.ListViewSubItem();
            OpenFile.SubItems.Add(OpenFile_key);
            OpenFile.SubItems.Add(OpenFile_ModiffierKey);
            OpenFile.SubItems.Add(OpenFile_option);
            OpenFile.ImageIndex = 4;
            listView1.Items.Add(OpenFile);

            ListViewItem PowerOff = new ListViewItem();
            PowerOff.Text = "Завершение работы";
            ListViewItem.ListViewSubItem PowerOff_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem PowerOff_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem PowerOffe_option = new ListViewItem.ListViewSubItem();
            PowerOff.SubItems.Add(PowerOff_key);
            PowerOff.SubItems.Add(PowerOff_ModiffierKey);
            PowerOff.SubItems.Add(PowerOffe_option);
            PowerOff.ImageIndex = 5;
            listView1.Items.Add(PowerOff);

            ListViewItem Reboot = new ListViewItem();
            Reboot.Text = "Перезагрузка";
            ListViewItem.ListViewSubItem Reboot_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Reboot_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Reboote_option = new ListViewItem.ListViewSubItem();
            Reboot.SubItems.Add(Reboot_key);
            Reboot.SubItems.Add(PowerOff_ModiffierKey);
            Reboot.SubItems.Add(Reboote_option);
            Reboot.ImageIndex = 8;
            listView1.Items.Add(Reboot);

            ListViewItem Hibernation = new ListViewItem();
            Hibernation.Text = "Сон";
            ListViewItem.ListViewSubItem Hibernation_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Hibernation_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Hibernation_option = new ListViewItem.ListViewSubItem();
            Hibernation.SubItems.Add(Hibernation_key);
            Hibernation.SubItems.Add(Hibernation_ModiffierKey);
            Hibernation.SubItems.Add(Hibernation_option);
            Hibernation.ImageIndex = 7;
            listView1.Items.Add(Hibernation);

            ListViewItem Perform = new ListViewItem();
            Perform.Text = "Выполнить";
            ListViewItem.ListViewSubItem Perform_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Perform_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem Perform_option = new ListViewItem.ListViewSubItem();
            Perform.SubItems.Add(Perform_key);
            Perform.SubItems.Add(Perform_ModiffierKey);
            Perform.SubItems.Add(Perform_option);
            Perform.ImageIndex = 6;
            listView1.Items.Add(Perform);

            ListViewItem RegEdit = new ListViewItem();
            RegEdit.Text = "Редактор реэстра";
            ListViewItem.ListViewSubItem RegEdit_key = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem RegEdit_ModiffierKey = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem RegEdit_option = new ListViewItem.ListViewSubItem();
            RegEdit.SubItems.Add(RegEdit_key);
            RegEdit.SubItems.Add(RegEdit_ModiffierKey);
            RegEdit.SubItems.Add(RegEdit_option);
            RegEdit.ImageIndex = 9;
            listView1.Items.Add(RegEdit);
        }

        void AdditionalEventsFieldsInit()
        {
            //Поля listView2
            listView2.Columns[0].Width = 200;            
            listView2.Columns[1].Width = 115;
            listView2.Columns[1].TextAlign = HorizontalAlignment.Center;
            listView2.Columns[2].Width = 60;
            listView2.Columns[3].Width = 300;
            listView2.FullRowSelect = true;
        }

        public Form1()
        {
#if DEBUG
            log.Debug("Form1::Form1()");
#endif            
            InitializeComponent();
            SystemEvents.PowerModeChanged += this.OnPowerModeChange;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            KeyHook = new GlobalHook();
            keyboardHelper = new KeyBoardHelper(this);
            keyboardHelper.SetupSettingFilePath();
            keyboardHelper.LoadSettingsFile();
            KeyHook.KeyDown += OnKeyDownEvent_Global;
#if DEBUG
            log.Debug("Form1::Form1() -> keyboardHelper.app_settings.TerminateKey = " + Convert.ToString(keyboardHelper.app_settings.TerminateKey));
#endif
        }

        ~Form1()
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EventsCollectionInit();
            AdditionalEventsFieldsInit();
            keyboardHelper.LoadAdditionalItems();
#if DEBUG
            log.Debug("Form1::Form1_Load()");
#endif            

            //Обновление елементов формы.
            textBox1.Text = keyboardHelper.app_settings.TerminateKey_Modifier == Keys.None ? keyboardHelper.app_settings.TerminateKey.ToString() : keyboardHelper.app_settings.TerminateKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.TerminateKey.ToString();
            textBox2.Text = keyboardHelper.app_settings.CD_DriveKey_Modifier == Keys.None ? keyboardHelper.app_settings.CD_DriveKey.ToString() : keyboardHelper.app_settings.CD_DriveKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.CD_DriveKey.ToString();
            textBox3.Text = keyboardHelper.app_settings.WindowsKey_Modifier == Keys.None ? keyboardHelper.app_settings.WindowsKey.ToString() : keyboardHelper.app_settings.WindowsKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.WindowsKey.ToString();
            textBox4.Text = keyboardHelper.app_settings.CMDKey_Modifier == Keys.None ? keyboardHelper.app_settings.CMDKey.ToString() : keyboardHelper.app_settings.CMDKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.CMDKey.ToString();
            textBox6.Text = keyboardHelper.app_settings.MaseterVolumeMuteKey_Modifier == Keys.None ? keyboardHelper.app_settings.MaseterVolumeMuteKey.ToString() : keyboardHelper.app_settings.MaseterVolumeMuteKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.MaseterVolumeMuteKey.ToString();
            textBox5.Text = keyboardHelper.app_settings.MaseterVolumeUpKey_Modifier == Keys.None ? keyboardHelper.app_settings.MaseterVolumeUpKey.ToString() : keyboardHelper.app_settings.MaseterVolumeUpKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.MaseterVolumeUpKey.ToString();
            textBox7.Text = keyboardHelper.app_settings.MaseterVolumeDownKey_Modifier == Keys.None ? keyboardHelper.app_settings.MaseterVolumeDownKey.ToString() : keyboardHelper.app_settings.MaseterVolumeDownKey_Modifier.ToString() + " + " + keyboardHelper.app_settings.MaseterVolumeDownKey.ToString();
#if DEBUG
            log.Debug("Form1::Form1_Load() -> keyboardHelper.app_settings.MasterVolumeStep = " + Convert.ToString(keyboardHelper.app_settings.MasterVolumeStep));
#endif
            numericUpDown1.Value = keyboardHelper.app_settings.MasterVolumeStep;
            numericUpDown1.Update();
            checkBox3.Checked = keyboardHelper.app_settings.bNitifyMsg;
            checkBox4.Checked = keyboardHelper.app_settings.bSoundNotify;          
            checkBox2.Checked = keyboardHelper.IsAutorunFlag() ? true : false;  
            checkBox12.Checked = keyboardHelper.app_settings.CmdEnable;

            //Переключатель завершение процессов.
            if (keyboardHelper.app_settings.switcher_ProcessTerminate)
            {
                pictureBox1.Image = Properties.Resources.sw_on; checkBox1.Enabled = keyboardHelper.app_settings.switcher_ProcessTerminate;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.sw_off; checkBox1.Enabled = keyboardHelper.app_settings.switcher_ProcessTerminate;
            }

            //Переключатель Окна.
            if (keyboardHelper.app_settings.switcher_Windows)
            {
                pictureBox2.Image = Properties.Resources.sw_on; checkBox8.Enabled = keyboardHelper.app_settings.switcher_Windows;
            }
            else
            {
                pictureBox2.Image = Properties.Resources.sw_off; checkBox8.Enabled = keyboardHelper.app_settings.switcher_Windows;
            }

            //Переключатель CD_Rom.
            if (keyboardHelper.app_settings.switcher_CD_Drive)
            {
                pictureBox3.Image = Properties.Resources.sw_on; checkBox7.Enabled = keyboardHelper.app_settings.switcher_CD_Drive;
            }
            else
            {
                pictureBox3.Image = Properties.Resources.sw_off; checkBox7.Enabled = keyboardHelper.app_settings.switcher_CD_Drive;
            }

            //Переключатель Отключение звука.
            if (keyboardHelper.app_settings.switcher_MasterVolumeMute)
            {
                pictureBox5.Image = Properties.Resources.sw_on; checkBox9.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeMute;
            }
            else
            {
                pictureBox5.Image = Properties.Resources.sw_off; checkBox9.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeMute;
            }

            //Переключатель Управление громкостью.
            if (keyboardHelper.app_settings.switcher_MasterVolumeControl)
            {
                pictureBox4.Image = Properties.Resources.sw_on; checkBox6.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl; checkBox10.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl; checkBox11.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl;
            }
            else
            {
                pictureBox4.Image = Properties.Resources.sw_off; checkBox6.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl; checkBox10.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl; checkBox11.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl;
            }

            //Переключатель Командная строка.
            if (keyboardHelper.app_settings.switcher_CMD)
            {
                pictureBox6.Image = Properties.Resources.sw_on; checkBox5.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeMute;
            }
            else
            {
                pictureBox6.Image = Properties.Resources.sw_off; checkBox5.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeMute;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.Visible == false && e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }

            else if (e.Button == MouseButtons.Left)
            {
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            textBox1.Enabled = checkBox1.Checked ? true : false;
        }

        private void checkBox1_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Клавиша, при нажатии которой будет заврешен процесс.", "Клавиша назначения", checkBox1);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void развернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void свернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                this.Show();
                this.WindowState = FormWindowState.Minimized;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox6.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            textBox2.Enabled = checkBox7.Checked ? true : false;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox5.Checked = false;
            checkBox7.Checked = false;
            checkBox6.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            textBox3.Enabled = checkBox8.Checked ? true : false;
        }

        private void checkBox8_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Клавиша, при нажатии которой будут свернуты или развернуты все окна.", "Клавиша назначения", checkBox8);
        }

        private void checkBox7_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Клавиша, при нажатии которой будет открыт дисковод CD дисков.", "Клавиша назначения", checkBox7);
        }

        private void groupBox1_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Завершает процесс, на окне которого установлен курсор.", "Завершение процессов", groupBox1);
        }

        private void groupBox2_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Сворачивает или разворачивает все активные окна.", "Окна", groupBox2);
        }

        private void groupBox3_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Открывает и закрывает лоток CD дисковода.", "CD Rom", groupBox3);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
                key.SetValue("AppTerminator", keyboardHelper.GetSpecialSymbol() + Application.ExecutablePath + keyboardHelper.GetSpecialSymbol() + " -a");
                key.Close();
            }

            else
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
                key.DeleteValue("AppTerminator");
                key.Close();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //Каждый раз создаем обект формы заново, т.к при закрытии формы он удаляеться.
            InfoForm = new Info();
            InfoForm.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            //keyboardHelper.app_settings.SaveSettingsFile();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.bNitifyMsg = checkBox3.Checked ? true : false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.bSoundNotify = checkBox4.Checked ? true : false;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                contextMenuStrip1.Show(MousePosition);
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox8.Checked = false;
            checkBox7.Checked = false;
            checkBox6.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            textBox4.Enabled = checkBox5.Checked ? true : false;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox8.Checked = false;
            checkBox7.Checked = false;
            checkBox5.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            textBox5.Enabled = checkBox6.Checked ? true : false;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox8.Checked = false;
            checkBox7.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox10.Checked = false;
            textBox6.Enabled = checkBox9.Checked ? true : false;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox8.Checked = false;
            checkBox7.Checked = false;
            checkBox9.Checked = false;
            checkBox6.Checked = false;
            checkBox5.Checked = false;

            textBox7.Enabled = checkBox10.Checked ? true : false;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = checkBox11.Checked ? true : false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.MasterVolumeStep = Convert.ToInt32(numericUpDown1.Value);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_ProcessTerminate = keyboardHelper.app_settings.switcher_ProcessTerminate ? false : true;
            if (keyboardHelper.app_settings.switcher_ProcessTerminate)
                pictureBox1.Image = Properties.Resources.sw_on;
            else
                pictureBox1.Image = Properties.Resources.sw_off;

            checkBox1.Enabled = keyboardHelper.app_settings.switcher_ProcessTerminate;
            checkBox1.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_Windows = keyboardHelper.app_settings.switcher_Windows ? false : true;
            if (keyboardHelper.app_settings.switcher_Windows)
                pictureBox2.Image = Properties.Resources.sw_on;
            else
                pictureBox2.Image = Properties.Resources.sw_off;

            checkBox8.Enabled = keyboardHelper.app_settings.switcher_Windows;
            checkBox8.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_CD_Drive = keyboardHelper.app_settings.switcher_CD_Drive ? false : true;
            if (keyboardHelper.app_settings.switcher_CD_Drive)
                pictureBox3.Image = Properties.Resources.sw_on;
            else
                pictureBox3.Image = Properties.Resources.sw_off;

            checkBox7.Enabled = keyboardHelper.app_settings.switcher_CD_Drive;
            checkBox7.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_MasterVolumeMute = keyboardHelper.app_settings.switcher_MasterVolumeMute ? false : true;
            if (keyboardHelper.app_settings.switcher_MasterVolumeMute)
                pictureBox5.Image = Properties.Resources.sw_on;
            else
                pictureBox5.Image = Properties.Resources.sw_off;

            checkBox9.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeMute;
            checkBox9.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_MasterVolumeControl = keyboardHelper.app_settings.switcher_MasterVolumeControl ? false : true;
            if (keyboardHelper.app_settings.switcher_MasterVolumeControl)
                pictureBox4.Image = Properties.Resources.sw_on;
            else
                pictureBox4.Image = Properties.Resources.sw_off;

            checkBox6.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl;
            checkBox10.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl;
            checkBox11.Enabled = keyboardHelper.app_settings.switcher_MasterVolumeControl;
            checkBox6.Checked = false;
            checkBox10.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.switcher_CMD = keyboardHelper.app_settings.switcher_CMD ? false : true;
            if (keyboardHelper.app_settings.switcher_CMD)
                pictureBox6.Image = Properties.Resources.sw_on;
            else
                pictureBox6.Image = Properties.Resources.sw_off;

            checkBox5.Enabled = keyboardHelper.app_settings.switcher_CMD;
            checkBox5.Checked = false;
            keyboardHelper.KeyBoardHelperFeatures.PlayButtonSwitchSound();
        }

        private void groupBox6_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Отключает звук на Мастер канале микшера Windows.", "Отключение звука", groupBox6);
        }

        private void groupBox7_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Регулирует уровень громкости на Мастер канале микшера Windows.", "Управление громкостью звука", groupBox7);
        }

        private void groupBox5_MouseHover(object sender, EventArgs e)
        {
            ShowTip("Открывае командную строку Windows.", "Командная строка", groupBox5);
        }

        private void tabPage1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                contextMenuStrip1.Show(MousePosition);
        }

        private void tabPage2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                contextMenuStrip1.Show(MousePosition);
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            foreach (ListViewItem it in listView2.Items)
            {
                if (listView1.FocusedItem.Text == it.Text
                && it.Text != "Открыть папку"
                && it.Text != "Открыть файл"
                && it.Text != "Открыть страницу")
                {
                    MessageBox.Show("Событие " + "'" + it.Text + "'" + " уже назначено!", "KeyBoaedHelper - добавление события", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            focus_item = listView1.FocusedItem;
            KeyInputForm = new KeyInput(this);
            KeyInputForm.Show();
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            keyboardHelper.app_settings.CmdEnable = checkBox12.Checked;
        }

        //Добавление элемента в listView2
        public void AddItemToListView(bool fromFile)
        {
            ListViewItem ListView2_Item = new ListViewItem();
            ListViewItem.ListViewSubItem ListView2Item_keys = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem ListView2Item_command = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem ListView2Item_options = new ListViewItem.ListViewSubItem();

            if (!fromFile)
            {                
                foreach (Item_t it in keyboardHelper.EventsCollection)
                {
                    if (it.Text == focus_item.Text)
                    {
                        if (it.ModifierKey == Keys.None)
                            ListView2Item_keys.Text = it.Key.ToString();
                        else
                            ListView2Item_keys.Text = it.ModifierKey.ToString() + " + " + it.Key.ToString();

                        ListView2Item_options.Text = it.options;
                        ListView2_Item.Text = it.Text;

                        if (it.command.Length == 0)
                            ListView2Item_command.Text = "None";
                        else
                            ListView2Item_command.Text = it.command;

                        ListView2_Item.SubItems.Add(ListView2Item_keys);
                        ListView2_Item.SubItems.Add(ListView2Item_command);
                        ListView2_Item.SubItems.Add(ListView2Item_options);
                        ListView2_Item.ImageIndex = it.indexOfImage;
                    }
                }
                listView2.Items.Add(ListView2_Item);
            }

            if (fromFile)
            {
                foreach (Item_t it in keyboardHelper.EventsCollection)
                {
                    ListView2_Item = new ListViewItem();
                    ListView2Item_keys = new ListViewItem.ListViewSubItem();
                    ListView2Item_options = new ListViewItem.ListViewSubItem();
                    ListView2Item_command = new ListViewItem.ListViewSubItem();

                    if (it.ModifierKey == Keys.None)
                        ListView2Item_keys.Text = it.Key.ToString();
                    else
                        ListView2Item_keys.Text = it.ModifierKey.ToString() + " + " + it.Key.ToString();

                    ListView2_Item.Text = it.Text;
                    ListView2Item_options.Text = it.options;
                    if (it.command.Length == 0)
                        ListView2Item_command.Text = "None";
                    else
                        ListView2Item_command.Text = it.command;
                    ListView2_Item.SubItems.Add(ListView2Item_keys);
                    ListView2_Item.SubItems.Add(ListView2Item_command);
                    ListView2_Item.SubItems.Add(ListView2Item_options);
                    ListView2_Item.ImageIndex = it.indexOfImage;
                    listView2.Items.Add(ListView2_Item);
                }
            }
        }

        void DeleteItem()
        {
            DialogResult MBresult = DialogResult.No;            
            foreach (Item_t it in keyboardHelper.EventsCollection)
            {
                if (it.Text == listView2.FocusedItem.Text
                    && it.options == listView2.FocusedItem.SubItems[3].Text //options
                    && it.command == listView2.FocusedItem.SubItems[2].Text) //command
                {
                    MBresult = MessageBox.Show("Удалить запись " + "'" + it.Text + "' ?", " KeyBoaedHelper - удаление записи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (MBresult == DialogResult.Yes)
                    {
                        keyboardHelper.EventsCollection.Remove(it);
                        break;
                    }
                }
            }

            if (MBresult == DialogResult.Yes)
            {
                listView2.Items.Remove(listView2.FocusedItem);
                keyboardHelper.SaveAdditionalItems();
            }
        }
        
        public void EditKeys()
        {
            keyboardHelper.EditedItem = new Item_t();
            keyboardHelper.EditedItem = keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit];
            keyboardHelper.EditedItem.Key = keyboardHelper.Bind_Key;
            keyboardHelper.EditedItem.ModifierKey = keyboardHelper.Bind_ModiffiersKey;
            keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit] = keyboardHelper.EditedItem;

            if (keyboardHelper.EditedItem.ModifierKey == Keys.None)
                focus_item.SubItems[1].Text = keyboardHelper.EditedItem.Key.ToString();
            else
                focus_item.SubItems[1].Text = keyboardHelper.EditedItem.ModifierKey.ToString() + " + " + keyboardHelper.EditedItem.Key.ToString();

            keyboardHelper.SaveAdditionalItems();
        }

        public void EditCommand()
        {
            keyboardHelper.EditedItem = new Item_t();
            keyboardHelper.EditedItem = keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit];
            keyboardHelper.EditedItem.command = keyboardHelper.Command;
            keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit] = keyboardHelper.EditedItem;

            focus_item.SubItems[2].Text = keyboardHelper.EditedItem.command;

            keyboardHelper.SaveAdditionalItems();
        }

        public void EditOptions()
        {
            keyboardHelper.EditedItem = new Item_t();
            keyboardHelper.EditedItem = keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit];
            keyboardHelper.EditedItem.options = keyboardHelper.EditOptions(focus_item.Text);
            keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit] = keyboardHelper.EditedItem;

            focus_item.SubItems[3].Text = keyboardHelper.EditedItem.options;

            keyboardHelper.SaveAdditionalItems();
        }

        void EditItem2(string FieldType)
        {
            switch (FieldType)
            {
                case "Keys":
                    foreach (Item_t it in keyboardHelper.EventsCollection)
                    {
                        if (listView2.FocusedItem.Text == it.Text
                            && it.options == listView2.FocusedItem.SubItems[3].Text
                            && it.command == listView2.FocusedItem.SubItems[2].Text)
                        {
                            keyboardHelper.ItemIndexToEdit = keyboardHelper.EventsCollection.IndexOf(it);
                            focus_item = listView2.FocusedItem;

                            keyboardHelper.isEditKeysMode = true;
                            KeyInputForm = new KeyInput(this);
                            KeyInputForm.Show();
                        }
                    }
                    break;
                case "Command":
                    foreach (Item_t it in keyboardHelper.EventsCollection)
                    {
                        if (listView2.FocusedItem.Text == it.Text
                            && it.options == listView2.FocusedItem.SubItems[3].Text
                            && it.command == listView2.FocusedItem.SubItems[2].Text)
                        {
                            keyboardHelper.ItemIndexToEdit = keyboardHelper.EventsCollection.IndexOf(it);
                            focus_item = listView2.FocusedItem;

                            keyboardHelper.isEditPerformCommandMode = true;                           
                            CommandInputForm = new CommandInput(this);
                            CommandInputForm.Show();
                        }
                    }
                    break;
                case "Options":
                    foreach (Item_t it in keyboardHelper.EventsCollection)
                    {
                        if (listView2.FocusedItem.Text == it.Text
                            && it.options == listView2.FocusedItem.SubItems[3].Text
                            && it.command == listView2.FocusedItem.SubItems[2].Text)
                        {
                            keyboardHelper.ItemIndexToEdit = keyboardHelper.EventsCollection.IndexOf(it);
                            focus_item = listView2.FocusedItem;

                            EditOptions();
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void редактироватьСочитаниеКлавишьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditItem2("Keys");
        }

        private void редактироватьКомандуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditItem2("Command");
        }

        private void редактироватьПараметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditItem2("Options");
        }

        /*public void EditItem()
        {
            keyboardHelper.EditedItem = new Item_t();
            keyboardHelper.EditedItem = keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit];
            keyboardHelper.EditedItem.Key = keyboardHelper.Bind_Key;
            keyboardHelper.EditedItem.ModifierKey = keyboardHelper.Bind_ModiffiersKey;
            keyboardHelper.EditedItem.command = keyboardHelper.Command;
            if (keyboardHelper.PerformCommandEdit)
            {
                keyboardHelper.EditedItem.options = keyboardHelper.PerformCommand;
            }
            keyboardHelper.EventsCollection[keyboardHelper.ItemIndexToEdit] = keyboardHelper.EditedItem;

            if (keyboardHelper.EditedItem.ModifierKey == Keys.None)
                focus_item.SubItems[1].Text = keyboardHelper.EditedItem.Key.ToString();
            else
                focus_item.SubItems[1].Text = keyboardHelper.EditedItem.ModifierKey.ToString() + " + " + keyboardHelper.EditedItem.Key.ToString();

            focus_item.SubItems[2].Text = keyboardHelper.EditedItem.command.ToString();

            keyboardHelper.SaveAdditionalItems();
        }*/
    }
}
 