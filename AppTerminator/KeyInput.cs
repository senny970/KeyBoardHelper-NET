using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AppTerminator
{
    public partial class KeyInput : Form
    {
        private Form1 MainForm;
        CommandInput CmdInputForm;
        public KeyInput(Form1 form)
        {
            InitializeComponent();
            MainForm = form;
            MainForm.keyboardHelper.Bind_Key = Keys.None;
            MainForm.keyboardHelper.Bind_ModiffiersKey = Keys.None;
            MainForm.keyboardHelper.isKeyInputFormShow = false;
        }

        private void KeyInput_Load(object sender, EventArgs e)
        {
            //this.ControlBox = false;
        }

        private void KeyInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MainForm.keyboardHelper.isEditKeysMode)
            {
                MainForm.keyboardHelper.isKeyInputFormShow = false;
                MainForm.EditKeys();
                MainForm.keyboardHelper.isEditKeysMode = false;
                this.Dispose();
            }
            else
            {
                MainForm.keyboardHelper.isKeyInputFormShow = false;
                this.Dispose();
                CmdInputForm = new CommandInput(MainForm);
                CmdInputForm.Show();
            }
        }

        private void KeyInput_Load_1(object sender, EventArgs e)
        {
            button1.Focus();
        }

        private void KeyInput_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}
