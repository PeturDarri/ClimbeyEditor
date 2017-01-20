﻿using UnityEngine;
using System;
using System.Collections.Generic;

using Image = UnityEngine.Texture2D;

namespace System.Windows.Forms
{
    [Serializable]
    public class AppResources
    {
        public List<Font> Fonts;

        public ReservedResources Images;

        /// <summary>
        /// System resources.
        /// </summary>
        [Serializable]
        public struct ReservedResources
        {
            [Tooltip("Form resize icon")]
            public Image ArrowDown;

            [Tooltip("Form resize icon, MonthCalendar, TabControl")]
            public Image ArrowLeft;

            [Tooltip("Form resize icon, MonthCalendar, TabControl")]
            public Image ArrowRight;

            [Tooltip("Form resize icon")]
            public Image ArrowUp;

            public Image Circle;

            [Tooltip("Checkbox, ToolStripItem")]
            public Image Checked;

            [Tooltip("Form close button")]
            public Image Close;

            public CursorImages Cursors;

            [Tooltip("ComboBox, VScrollBar")]
            public Image CurvedArrowDown;

            [Tooltip("HScrollBar")]
            public Image CurvedArrowLeft;

            [Tooltip("HScrollBar")]
            public Image CurvedArrowRight;

            [Tooltip("VScrollBar")]
            public Image CurvedArrowUp;

            [Tooltip("ToolStripDropDown")]
            public Image DropDownRightArrow;

            [Tooltip("Form")]
            public Image FormResize;

            [Tooltip("NumericUpDown")]
            public Image NumericDown;

            [Tooltip("NumericUpDown")]
            public Image NumericUp;

            [Tooltip("Tree")]
            public Image TreeNodeCollapsed;

            [Tooltip("Tree")]
            public Image TreeNodeExpanded;
        }

        [Serializable]
        public struct CursorImages
        {
            [Tooltip("Leave this field empty if you don't want to use your own cursor.")]
            public Image Default;

            public Image Hand;
            public Image Help;
            public Image HSplit;
            public Image SizeAll;
            public Image SizeNESW;
            public Image SizeNS;
            public Image SizeNWSE;
            public Image SizeWE;
            public Image VSplit;
        }
    }
}