﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Drawing
{
    [Serializable]
    public sealed class Font
    {
        private readonly string _name;
        private readonly FontStyle _style;

        internal UnityEngine.Font UFont;

        public string Name { get { return _name; } }
        public float Size { get; set; }
        public FontStyle Style { get { return _style; } }
        
        public Font(string familyName, float emSize)
        {
            _name = familyName;
            Size = emSize;
        }
        public Font(string familyName, float emSize, FontStyle style)
        {
            _name = familyName;
            _style = style;
            Size = emSize;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", Name, Size, Style);
        }

        public static bool Exists(string fontName)
        {
            var fontList = System.Windows.Forms.ApplicationBehaviour.Resources.Fonts.Where(x => x.fontNames[0] == fontName).ToList();
            return fontList.Count > 0;
        }
    }

    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        [Obsolete("it's not working")]
        Underline = 4,
        [Obsolete("it's not working")]
        Strikeout = 8,
    }
}
