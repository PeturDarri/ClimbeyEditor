﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Drawing
{
    public class BitmapFont
    {
        internal Dictionary<int, BitmapChar> textureList;

        public bool Loaded { get; private set; }

        public BitmapFont()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fontImage"></param>
        /// <param name="format">GHL format (FontBuilder)</param>
        public void Load(Bitmap fontImage, byte[] format)
        {
            var ser = new Xml.Serialization.XmlSerializer(typeof(font));
            using (var str = new System.IO.MemoryStream(format))
            {
                var font = (font)ser.Deserialize(str);
                textureList = new Dictionary<int, BitmapChar>();
                for (int i = 0; i < font.chars.Length; i++)
                {
                    var fChar = font.chars[i];

                    var bChar = new BitmapChar();
                    bChar.Id = fChar.id[0];
                    bChar.Advance = fChar.advance;

                    var charRect = fChar.rect.Split(' ');
                    int charX = Convert.ToInt32(charRect[0]);
                    int charY = Convert.ToInt32(charRect[1]);
                    int charW = Convert.ToInt32(charRect[2]);
                    int charH = Convert.ToInt32(charRect[3]);

                    var offset = fChar.offset.Split(' ');
                    bChar.OffsetX = Convert.ToInt32(offset[0]);
                    bChar.OffsetY = Convert.ToInt32(offset[1]);

                    bChar.Texture = new Bitmap(charW, charH);
                    bChar.Texture.uTexture.name = (char)bChar.Id + "_GlyphTexture";
                    if (charW > 0 && charH > 0)
                    {
                        
                    }

                    if (charW > 0 && charH > 0)
                        for (int y = charY, by = 0; by < charH; y++, by++)
                            for (int x = charX, bx = 0; bx < charW; x++, bx++)
                            {
                                var c1 = fontImage.GetPixel(x, y);
                                bChar.Texture.SetPixel(bx, by, c1);
                            }

                    bChar.Texture.Apply();
                    textureList.Add(bChar.Id, bChar);
                }
            }

            Loaded = true;
        }

        public class BitmapChar
        {
            public byte Advance { get; set; }
            public int Id { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
            public Bitmap Texture { get; set; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class font
        {

            private fontDescription descriptionField;

            private fontMetrics metricsField;

            private fontTexture textureField;

            private fontChar[] charsField;

            private string typeField;

            /// <remarks/>
            public fontDescription description
            {
                get
                {
                    return this.descriptionField;
                }
                set
                {
                    this.descriptionField = value;
                }
            }

            /// <remarks/>
            public fontMetrics metrics
            {
                get
                {
                    return this.metricsField;
                }
                set
                {
                    this.metricsField = value;
                }
            }

            /// <remarks/>
            public fontTexture texture
            {
                get
                {
                    return this.textureField;
                }
                set
                {
                    this.textureField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("char", IsNullable = false)]
            public fontChar[] chars
            {
                get
                {
                    return this.charsField;
                }
                set
                {
                    this.charsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class fontDescription
        {

            private byte sizeField;

            private string familyField;

            private string styleField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte size
            {
                get
                {
                    return this.sizeField;
                }
                set
                {
                    this.sizeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string family
            {
                get
                {
                    return this.familyField;
                }
                set
                {
                    this.familyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string style
            {
                get
                {
                    return this.styleField;
                }
                set
                {
                    this.styleField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class fontMetrics
        {

            private byte ascenderField;

            private byte heightField;

            private sbyte descenderField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte ascender
            {
                get
                {
                    return this.ascenderField;
                }
                set
                {
                    this.ascenderField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte height
            {
                get
                {
                    return this.heightField;
                }
                set
                {
                    this.heightField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public sbyte descender
            {
                get
                {
                    return this.descenderField;
                }
                set
                {
                    this.descenderField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class fontTexture
        {

            private ushort widthField;

            private ushort heightField;

            private string fileField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort width
            {
                get
                {
                    return this.widthField;
                }
                set
                {
                    this.widthField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort height
            {
                get
                {
                    return this.heightField;
                }
                set
                {
                    this.heightField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string file
            {
                get
                {
                    return this.fileField;
                }
                set
                {
                    this.fileField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class fontChar
        {

            private string offsetField;

            private string rectField;

            private byte advanceField;

            private string idField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string offset
            {
                get
                {
                    return this.offsetField;
                }
                set
                {
                    this.offsetField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string rect
            {
                get
                {
                    return this.rectField;
                }
                set
                {
                    this.rectField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte advance
            {
                get
                {
                    return this.advanceField;
                }
                set
                {
                    this.advanceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string id
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }
        }
    }
}

