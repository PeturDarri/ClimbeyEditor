using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine;
using Color = System.Drawing.Color;
using Screen = System.Windows.Forms.Screen;

namespace ClimbeyEditor
{
    public class MainForm : Form
    {
        private static int yOffset;

        public MainForm()
        {
            Anchor = AnchorStyles.All;
            BackColor = SystemColors.Control;
            ControlBox = false; // Close button.
            Location = new Point(0, yOffset);
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Movable = false;
            Resizable = false;

            #region MenuStrip

            //Create menu
            var menu = new MenuStrip();
            menu.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            menu.BackColor = Color.FromArgb(64, 64, 64);
            menu.Location = new Point(0, 0);
            menu.Height = 26;
            menu.Width = Width;
            Controls.Add(menu);

            yOffset += menu.Height;

            //File
            {
                var itemFile = new ToolStripMenuItem("File");
                itemFile.BackColor = menu.BackColor;
                itemFile.ForeColor = Color.White;
                itemFile.Height = 22; // There is no AutoSize yet.
                itemFile.Width = 52;
                menu.Items.Add(itemFile);

                //Items
                var itemNew = new ToolStripMenuItem("New");
                itemNew.Click += OnClickNew;
                itemFile.DropDownItems.Add(itemNew);

                var itemOpen = new ToolStripMenuItem("Open...");
                itemOpen.Click += OnClickOpen;
                //itemOpen.Click += (s, a) => { UnityEngine.Application.Quit(); };
                itemFile.DropDownItems.Add(itemOpen);

                var itemSave = new ToolStripMenuItem("Save");
                itemSave.Click += OnClickSave;
                itemFile.DropDownItems.Add(itemSave);

                var itemSaveAs = new ToolStripMenuItem("Save as...");
                itemSaveAs.Click += OnClickSaveAs;
                itemFile.DropDownItems.Add(itemSaveAs);
            }

            #endregion
        }

        //Events

        #region MenuStrip

        private void OnClickNew(object sender, EventArgs e)
        {

        }

        private void OnClickOpen(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Txt files|*.txt|All files|*.*";
            openFileDialog.ShowDialog((ofdForm, result) =>
            {
                if (result == DialogResult.OK)
                    MessageBox.Show(openFileDialog.FileName + " was selected to open.");
            });
        }

        private void OnClickSave(object sender, EventArgs e)
        {

        }

        private void OnClickSaveAs(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Txt files|*.txt";
            saveFileDialog.ShowDialog((sfdForm, result) =>
            {
                if (result == DialogResult.OK)
                    MessageBox.Show(saveFileDialog.FileName + " was selected to save.");
            });
        }

        #endregion
    }
}
