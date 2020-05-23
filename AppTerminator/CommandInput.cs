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
    public partial class CommandInput : Form
    {
        private Form1 MainForm;
        public CommandInput(Form1 form)
        {
            InitializeComponent();
            MainForm = form;
            MainForm.keyboardHelper.isCmdFormShow = true;
        }

        private void CommandInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void CommandInput_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Main
            if (!MainForm.keyboardHelper.isEditKeysMode && !MainForm.keyboardHelper.isEditPerformCommandMode)
            {
                MainForm.keyboardHelper.isCmdFormShow = false;
                MainForm.keyboardHelper.Command = textBox1.Text.ToLower();
                MainForm.keyboardHelper.AddItemToList();
                this.Dispose();
                return;
            }

            //EditCommand
            if (MainForm.keyboardHelper.isEditPerformCommandMode)
            {
                MainForm.keyboardHelper.Command = textBox1.Text.ToLower();
                MainForm.keyboardHelper.isEditPerformCommandMode = false;
                MainForm.EditCommand();
                this.Dispose();
                return;
            }
        }

        private void CommandInput_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
