﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace System.Windows.Forms
{
    public abstract class ScrollBar : Control
    {
        private int largeChange = 10;
        private int maximum = 100;
        private int minimum = 0;
        private int minScrollSize = 17;
        private bool scrollCanDrag;
        private ColorF scrollCurrentColor;
        private Color scrollDestinationColor;
        private Point scrollDragStartLocation;
        private Point scrollDragRectOffset;
        private bool scrollDraging;
        protected ScrollOrientation scrollOrientation;
        private Rectangle scrollRect;
        private int smallChange = 1;
        private int value = 0;

        internal Button addButton;
        internal Button subtractButton;

        public int LargeChange
        {
            get { return Math.Min(largeChange, maximum - minimum + 1); }
            set
            {
                largeChange = value;
                UpdateScrollRect();
            }
        }
        public int Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                UpdateScrollRect();
            }
        }
        public int Minimum
        {
            get { return minimum; }
            set { minimum = value; }
        }
        public Color ScrollColor { get; set; }
        public Color ScrollHoverColor { get; set; }
        public int SmallChange
        {
            get { return smallChange; }
            set { smallChange = value; }
        }
        public int Value
        {
            get { return value; }
            set
            {
                bool changed = this.value != value;
                this.value = value;
                if (changed)
                {
                    UpdateScrollRect();
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        public ScrollBar()
        {
            BackColor = Color.FromArgb(240, 240, 240);
            ScrollColor = Color.FromArgb(205, 205, 205);
            ScrollHoverColor = Color.FromArgb(192, 192, 192);

            var backColor = Color.FromArgb(240, 240, 240);
            var backHoverColor = Color.FromArgb(218, 218, 218);
            var borderColor = Color.Transparent;
            var borderHoverColor = Color.Transparent;
            var imageColor = Color.FromArgb(96, 96, 96);
            var imageHoverColor = Color.Black;

            addButton = new RepeatButton();
            addButton.CanSelect = false;
            addButton.BorderHoverColor = borderHoverColor;
            addButton.HoverColor = backHoverColor;
            addButton.ImageColor = imageColor;
            addButton.ImageHoverColor = imageHoverColor;
            addButton.BorderColor = borderColor;
            addButton.BackColor = backColor;
            addButton.Click += (s, a) => { DoScroll(ScrollEventType.SmallIncrement); };
            Controls.Add(addButton);

            subtractButton = new RepeatButton();
            subtractButton.CanSelect = false;
            subtractButton.BorderHoverColor = borderHoverColor;
            subtractButton.HoverColor = backHoverColor;
            subtractButton.ImageColor = imageColor;
            subtractButton.ImageHoverColor = imageHoverColor;
            subtractButton.BorderColor = borderColor;
            subtractButton.BackColor = backColor;
            subtractButton.Click += (s, a) => { DoScroll(ScrollEventType.SmallDecrement); };
            Controls.Add(subtractButton);

            Owner.UpClick += Owner_UpClick;
        }

        private void DoScroll(ScrollEventType type)
        {
            int newValue = value;
            int oldValue = value;

            switch (type)
            {
                case ScrollEventType.First:
                    newValue = minimum;
                    break;

                case ScrollEventType.Last:
                    newValue = maximum - LargeChange + 1;
                    break;

                case ScrollEventType.SmallDecrement:
                    newValue = Math.Max(value - SmallChange, minimum);
                    break;

                case ScrollEventType.SmallIncrement:
                    newValue = Math.Min(value + SmallChange, maximum - LargeChange + 1);
                    break;

                case ScrollEventType.LargeDecrement:
                    newValue = Math.Max(value - LargeChange, minimum);
                    break;

                case ScrollEventType.LargeIncrement:
                    newValue = Math.Min(value + LargeChange, maximum - LargeChange + 1);
                    break;

                case ScrollEventType.ThumbPosition:
                case ScrollEventType.ThumbTrack:

                    Application.Log("not implemented yet");
                    break;
            }

            UpdateScrollRect();

            ScrollEventArgs se = new ScrollEventArgs(type, oldValue, newValue, this.scrollOrientation);
            OnScroll(se);
            Value = se.NewValue;
        }
        private void Owner_UpClick(object sender, MouseEventArgs e)
        {
            if (scrollDraging)
                UpdateScrollRect();

            scrollDraging = false;
            scrollCanDrag = false;
        }
        private void UpdateValueAtScrollRect()
        {
            float scrollLength = 0;
            if (scrollOrientation == ScrollOrientation.HorizontalScroll)
                scrollLength = addButton.Location.X - subtractButton.Location.X - subtractButton.Width;
            else
                scrollLength = addButton.Location.Y - subtractButton.Location.Y - subtractButton.Height;

            int valueRange = (maximum - minimum);
            float barSize = minScrollSize;
            if (largeChange > 0)
                barSize = (float)scrollLength / ((float)valueRange / largeChange);
            if (barSize < minScrollSize)
                barSize = minScrollSize;

            scrollLength -= barSize; // Addjusted range for scroll bar, depending on size.

            if (scrollOrientation == ScrollOrientation.HorizontalScroll)
                value = (int)(((valueRange - largeChange + 1) * (scrollRect.X - subtractButton.Location.X - subtractButton.Width)) / scrollLength + minimum);
            else
                value = (int)(((valueRange - largeChange + 1) * (scrollRect.Y - subtractButton.Location.Y - subtractButton.Height)) / scrollLength + minimum);
            OnValueChanged(EventArgs.Empty);
        }
        protected void UpdateScrollRect()
        {
            // WIN_API Random calculations.

            float sx = 0;
            float sy = 0;
            float sw = 0;
            float sh = 0;

            // Total range for scroll bar.
            float scrollLength = 0;
            if (scrollOrientation == ScrollOrientation.HorizontalScroll)
                scrollLength = addButton.Location.X - subtractButton.Location.X - subtractButton.Width;
            else
                scrollLength = addButton.Location.Y - subtractButton.Location.Y - subtractButton.Height;

            int valueRange = (maximum - minimum);
            float barSize = minScrollSize;
            if (largeChange > 0)
                barSize = (float)scrollLength / ((float)valueRange / largeChange);
            if (barSize < minScrollSize)
                barSize = minScrollSize;
            /*
            Example:
                this.Width = 134;
                addButton.Width = 17;
                subtractButton.Width = 17;

                scrollLength = 134 - 17 - 17 = 100;
                maximum = 400;
                minimum = 0;
                largeChange = 100; // 
                estimatedScrollWidth = 100 / ((400 - 0) / 100) = 100 / (4) = 25;
            */
            scrollLength -= barSize; // Addjusted range for scroll bar, depending on size.

            float valueK = (float)(Value - minimum) / (valueRange - largeChange + 1);
            float scrollPos = scrollLength * valueK;

            if (scrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                sx = subtractButton.Location.X + subtractButton.Width + scrollPos;
                sy = 0;
                sw = barSize;
                sh = Height;

                scrollRect = new Rectangle((int)sx, (int)sy, (int)sw, (int)sh);

                if (sx + sw > addButton.Location.X && sw < scrollLength + barSize)
                {
                    sx = addButton.Location.X - sw;
                    scrollRect = new Rectangle((int)sx, (int)sy, (int)sw, (int)sh);
                    UpdateValueAtScrollRect();
                }
                else if (sw > scrollLength + barSize)
                    scrollRect = new Rectangle(0, 0, 17, (int)sh);
            }
            else
            {
                sx = 0;
                sy = subtractButton.Location.Y + subtractButton.Height + scrollPos;
                sw = Width;
                sh = barSize;

                scrollRect = new Rectangle((int)sx, (int)sy, (int)sw, (int)sh);

                if (sy + sh > addButton.Location.Y && sh < scrollLength + barSize)
                {
                    sy = addButton.Location.Y - sh;
                    scrollRect = new Rectangle((int)sx, (int)sy, (int)sw, (int)sh);
                    UpdateValueAtScrollRect();
                }
                else if (sh > scrollLength + barSize)
                    scrollRect = new Rectangle(0, 0, (int)sw, 17);
            }
        }

        public override void Dispose()
        {
            Owner.UpClick -= Owner_UpClick;

            base.Dispose();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (scrollRect.Contains(e.Location))
            {
                scrollCanDrag = true;
                scrollDragStartLocation = e.Location;
                scrollDragRectOffset = e.Location - scrollRect.Location;
            }
            else
            {
                scrollCanDrag = false;
                scrollDraging = false;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (scrollCanDrag && e.Location.Distance(scrollDragStartLocation) > 2)
            {
                scrollCanDrag = false;
                scrollDraging = true;
            }

            if (scrollDraging)
            {
                int sX = scrollRect.X;
                int sY = scrollRect.Y;
                if (scrollOrientation == ScrollOrientation.HorizontalScroll)
                {
                    sX = e.Location.X - scrollDragRectOffset.X;
                    if (sX < subtractButton.Location.X + subtractButton.Width)
                        sX = subtractButton.Location.X + subtractButton.Width;
                    if (sX + scrollRect.Width > addButton.Location.X)
                        sX = addButton.Location.X - scrollRect.Width;
                }
                else
                {
                    sY = e.Location.Y - scrollDragRectOffset.Y;
                    if (sY < subtractButton.Location.Y + subtractButton.Height)
                        sY = subtractButton.Location.Y + subtractButton.Height;
                    if (sY + scrollRect.Height > addButton.Location.Y)
                        sY = addButton.Location.Y - scrollRect.Height;
                }
                scrollRect = new Rectangle(sX, sY, scrollRect.Width, scrollRect.Height);

                UpdateValueAtScrollRect();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (scrollDraging == false)
            {
                var sEvent = ScrollEventType.LargeIncrement;
                if (scrollRect.Contains(e.Location) == false)
                {
                    if (scrollOrientation == ScrollOrientation.HorizontalScroll)
                    {
                        if (scrollRect.X + scrollRect.Width / 2 > e.Location.X)
                            sEvent = ScrollEventType.LargeDecrement;
                    }
                    else
                    {
                        if (scrollRect.Y + scrollRect.Height / 2 > e.Location.Y)
                            sEvent = ScrollEventType.LargeDecrement;
                    }

                    DoScroll(sEvent);
                }
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var wheelDelta = e.Delta / 4;

            bool scrolled = false;

            while (wheelDelta != 0)
            {
                if (wheelDelta > 0)
                {
                    DoScroll(ScrollEventType.SmallDecrement);
                    wheelDelta -= smallChange;
                    scrolled = true;
                    if (wheelDelta <= 0)
                        break;
                }
                else
                {
                    DoScroll(ScrollEventType.SmallIncrement);
                    wheelDelta += smallChange;
                    scrolled = true;
                    if (wheelDelta >= 0)
                        break;
                }
            }

            if (scrolled)
                DoScroll(ScrollEventType.EndScroll);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Hovered)
                scrollDestinationColor = ScrollHoverColor;
            else
                scrollDestinationColor = ScrollColor;
            scrollCurrentColor = MathHelper.ColorLerp(scrollCurrentColor, scrollDestinationColor, 4);

            if (scrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                int backX = subtractButton.Location.X + subtractButton.Width;
                e.Graphics.FillRectangle(BackColor, backX, 0, addButton.Location.X - backX, Height);
            }
            else
            {
                int backY = subtractButton.Location.Y + subtractButton.Height;
                e.Graphics.FillRectangle(BackColor, 0, backY, Width, addButton.Location.Y - backY);
            }
            e.Graphics.FillRectangle(scrollCurrentColor, scrollRect.X, scrollRect.Y, scrollRect.Width, scrollRect.Height);
        }
        protected override object OnPaintEditor(float width)
        {
            var component = base.OnPaintEditor(width);
#if UNITY_EDITOR
            Editor.NewLine(1);
            Editor.Label("\tScrollBar");

            var editorLargeChange = Editor.IntField("LargeChange", LargeChange);
            if (editorLargeChange.Changed)
                LargeChange = editorLargeChange.Value[0];

            var editorMaximum = Editor.IntField("Maximum", Maximum);
            if (editorMaximum.Changed)
                Maximum = editorMaximum.Value[0];

            var editorMinimum = Editor.IntField("Minimum", Minimum);
            if (editorMinimum.Changed)
                Minimum = editorMinimum.Value[0];

            Editor.ColorField("ScrollColor", ScrollColor, (c) => { ScrollColor = c; });

            var editorSmallChange = Editor.IntField("SmallChange", SmallChange);
            if (editorSmallChange.Changed)
                SmallChange = editorSmallChange.Value[0];

            var editorValue = Editor.IntField("Value", Value);
            if (editorValue.Changed)
                Value = editorValue.Value[0];
#endif
            return component;
        }
        protected override void OnResize(Point delta)
        {
            base.OnResize(delta);
            UpdateScrollRect();
        }
        protected virtual void OnScroll(ScrollEventArgs se)
        {
            Scroll(this, se);
        }
        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged(this, e);
        }
        public override void Refresh()
        {
            base.Refresh();

            UpdateScrollRect();
        }

        public event ScrollEventHandler Scroll = delegate { };
        public event EventHandler ValueChanged = delegate { };
    }
}
