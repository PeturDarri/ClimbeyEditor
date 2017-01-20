﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    public class CheckBox : Button
    {
        public bool Checked { get; set; }

        public CheckBox()
        {
            BackColor = Color.White;
            BorderColor = Color.FromArgb(112, 112, 112);
            BorderHoverColor = Color.FromArgb(51, 153, 255);
            BorderDisableColor = Color.FromArgb(188, 188, 188);
            CanSelect = true;
            DisableColor = Color.FromArgb(230, 230, 230);
            ForeColor = Color.Black;
            HoverColor = Color.FromArgb(243, 249, 255);
            Size = new Drawing.Size(128, 17);
            TextAlign = ContentAlignment.MiddleLeft;

            Click += CheckBox_Click;
        }

        void CheckBox_Click(object sender, EventArgs e)
        {
            Checked = !Checked;
            CheckedChanged(this, new EventArgs());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var backColor = DisableColor;
            var borderColor = BorderDisableColor;
            if (Enabled)
            {
                if (Hovered)
                {
                    backColor = HoverColor;
                    borderColor = BorderHoverColor;
                }
                else
                {
                    backColor = BackColor;
                    borderColor = BorderColor;
                }
            }

            var checkRectX = Padding.Left;
            var checkRectY = Padding.Top + Height / 2 - 6;
            var checkRectWH = 12;

            g.FillRectangle(backColor, checkRectX, checkRectY, checkRectWH, checkRectWH);
            g.DrawRectangle(new Pen(borderColor), checkRectX, checkRectY, checkRectWH, checkRectWH);
            if (Checked)
                g.DrawTexture(ApplicationBehaviour.Resources.Images.Checked, checkRectX, checkRectY, checkRectWH, checkRectWH);

            g.DrawString(Text, Font, new SolidBrush(ForeColor), checkRectX + checkRectWH + 4, Padding.Top + 0, Width - 20, Height, TextAlign);
        }

        public event EventHandler CheckedChanged = delegate { };
    }
}
