﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    [Serializable]
    public class ToolStrip : ScrollableControl
    {
        private ToolStripItemCollection _items;
        private PaintEventArgs p_args;

        public ToolStrip()
        {
            _items = new ToolStripItemCollection(this, null);
            p_args = new PaintEventArgs();

            BackColor = Color.FromArgb(246, 246, 246);
            BorderColor = Color.FromArgb(204, 206, 219);
            Orientation = Forms.Orientation.Vertical;

            Owner.UpClick += Application_UpClick;
        }
        public ToolStrip(ToolStripItem[] items)
        {
            _items = new ToolStripItemCollection(this, items);
            _items.AddRange(items);

            BackColor = Color.FromArgb(246, 246, 246);
            BorderColor = Color.FromArgb(204, 206, 219);
            Orientation = Forms.Orientation.Vertical;

            Owner.UpClick += Application_UpClick;
        }

        public Color BorderColor { get; set; }
        public virtual ToolStripItemCollection Items { get { return _items; } }
        public Orientation Orientation { get; set; }
        internal ToolStripItem OwnerItem { get; set; }

        void Application_UpClick(object sender, EventArgs e)
        {
            bool reset = true;
            if (sender != null && sender is ToolStrip)
            {
                if ((sender as ToolStrip).OwnerItem != null)
                {
                    var parent = (sender as ToolStrip).OwnerItem.Parent;
                    while (true)
                    {
                        if (parent == null) break;
                        if (parent == this)
                        {
                            reset = false;
                            break;
                        }

                        if (parent.OwnerItem == null) break;

                        parent = parent.OwnerItem.Parent;
                    }
                }
            }

            var mc_pos = PointToClient(MousePosition);
            if (!ClientRectangle.Contains(mc_pos) && reset)
                for (int i = 0; i < _items.Count; i++)
                    _items[i].Selected = false;
        }

        public override void Dispose()
        {
            Owner.DownClick -= Application_UpClick;
            base.Dispose();
        }
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            var mc_pos = ((MouseEventArgs)e).Location;
            for (int i = 0, x = Padding.Left, y = Padding.Top; i < _items.Count; i++)
            {
                if (_items[i].JustVisual) continue;

                _items[i].RaiseOnMouseLeave(e);
                if (mc_pos.X > x && mc_pos.X < x + _items[i].Width && mc_pos.Y > y && mc_pos.Y < y + _items[i].Height)
                    _items[i].RaiseOnMouseEnter(e);

                if (Orientation == Forms.Orientation.Horizontal)
                    x += _items[i].Width;
                if (Orientation == Forms.Orientation.Vertical)
                    y += _items[i].Height;
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].JustVisual) continue;
                _items[i].RaiseOnMouseLeave(e);
            }
        }
        protected override void OnMouseUp(MouseEventArgs e) // Click.
        {
            base.OnMouseUp(e);

            int prevSelected = -1;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Selected) prevSelected = i;
                _items[i].Selected = false;
            }

            var mc_pos = e.Location;
            for (int i = 0, x = Padding.Left, y = Padding.Top; i < _items.Count; i++)
            {
                if (_items[i].JustVisual) continue;

                if (mc_pos.X > x && mc_pos.X < x + _items[i].Width && mc_pos.Y > y && mc_pos.Y < y + _items[i].Height)
                {
                    if (i != prevSelected)
                        _items[i].Selected = true;

                    ItemClicked(this, new ToolStripItemClickedEventArgs(_items[i]));
                    _items[i].RaiseOnMouseUp(e);
                    break;
                }

                if (Orientation == Forms.Orientation.Horizontal)
                    x += _items[i].Width;
                if (Orientation == Forms.Orientation.Vertical)
                    y += _items[i].Height;
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case UnityEngine.KeyCode.DownArrow: break;
                case UnityEngine.KeyCode.LeftArrow: break;
                case UnityEngine.KeyCode.RightArrow: break;
                case UnityEngine.KeyCode.UpArrow: break;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (OwnerItem != null && OwnerItem.Parent != null && OwnerItem.Parent.Orientation == Forms.Orientation.Horizontal && ShadowHandler == null)
            {
                MakeShadow();
            }

            base.OnPaint(e);

            p_args.Graphics = e.Graphics;
            p_args.ClipRectangle = e.ClipRectangle;

            p_args.Graphics.FillRectangle(new Drawing.SolidBrush(BackColor), 0, 0, Width, Height);
            if (Orientation == Forms.Orientation.Vertical)
                p_args.Graphics.DrawLine(new Drawing.Pen(Drawing.Color.FromArgb(215, 215, 215)), 24, 2, 24, Height - 2);
            for (int i = 0, x = Padding.Left, y = Padding.Top; i < _items.Count; i++)
            {
                p_args.ClipRectangle = new Drawing.Rectangle(x, y, _items[i].Width, _items[i].Height);
                _items[i].RaiseOnPaint(p_args);

                if (_items[i].JustVisual) continue;
                if (Orientation == Forms.Orientation.Horizontal)
                    x += _items[i].Width;
                if (Orientation == Forms.Orientation.Vertical)
                    y += _items[i].Height;
            }

            p_args.Graphics.DrawRectangle(new Drawing.Pen(BorderColor), 0, 0, Width, Height);
        }
        protected override object OnPaintEditor(float width)
        {
            var control = base.OnPaintEditor(width);

#if UNITY_EDITOR
            Editor.NewLine(1);
            Editor.ColorField("BorderColor", BorderColor, new Action<Color>((c) => { BorderColor = c; }));
            Editor.Label("Orientation", Orientation);
            Editor.Label("OwnerItem", OwnerItem);
#endif

            return control;
        }

        public event ToolStripItemClickedEventHandler ItemClicked = delegate { };

        public void ResetSelected()
        {
            for (int i = 0; i < _items.Count; i++)
                _items[i].Selected = false;
        }

        /// <summary>
        /// For menu strips.
        /// </summary>
        internal void MakeShadow()
        {
            ShadowHandler = (g) =>
            {
                g.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(12, 64, 64, 64)), -3, 0, Width + 6, Height + 3);
                g.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(12, 64, 64, 64)), -2, 0, Width + 4, Height + 2);
                g.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(12, 64, 64, 64)), -1, 0, Width + 2, Height + 1);
            };
        }
    }
}
