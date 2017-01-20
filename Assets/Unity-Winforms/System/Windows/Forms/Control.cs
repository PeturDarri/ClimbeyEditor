﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    [Serializable]
    public class Control : Component, IDisposable
    {
        public static Application DefaultController { get; set; }
        public static Font DefaultFont = new Font("Arial", 12);
        public static Point MousePosition
        {
            get
            {
                return new Point(
                    (int)(UnityEngine.Input.mousePosition.x / Application.ScaleX), 
                    (int)((UnityEngine.Screen.height - UnityEngine.Input.mousePosition.y) / Application.ScaleY));
            }
        }

        private bool _context;
        private Control.ControlCollection _controls;
        private bool _disposing;
        internal bool selected = false;
        internal static Control lastSelected;
        //internal static Control lastSelected;
        private Point _location = new Point();
        private int _height;
        private bool _isDisposed;
        internal bool hovered;
        internal bool mouseEntered;
        private Control _parent;
        private bool _visible;
        private int _width;

        // Editor toggles.
        private bool _toggleEditor = true;
        private bool _toggleFont;
        private bool _toggleControls;
        private bool _toggleSource;
        
        public bool AllowDrop { get; set; }
        public bool AlwaysFocused { get; set; }
        public virtual AnchorStyles Anchor { get; set; }
        /// <summary>
        /// GUI.BeingGroup(...) & GUI.EndGroup().
        /// </summary>
        public bool AutoGroup { get; set; }
        public virtual bool AutoSize { get; set; }
        public Color BackColor { get; set; }
        public virtual ImageLayout BackgroundImageLayout { get; set; }
        internal bool Batched { get; set; } // For testing.
        public int Batches { get; internal set; }
        public Rectangle Bounds
        {
            get { return new Rectangle(Location.X, Location.Y, Width, Height); }
            set
            {
                Location = new Point(value.X, value.Y);
                Size = new Size(value.Width, value.Height);
            }
        }
        public bool CanSelect { get; set; }
        public Rectangle ClientRectangle { get { return new Rectangle(0, 0, Width, Height); } }
        public virtual bool Context // Close on click control. TODO: make it obsolete, find other way.
        {
            get { return _context; }
            set
            {
                if (_context != value)
                {
                    _context = value;
                    if (_context)
                        Owner.Contexts.Add(this);
                    else
                        Owner.Contexts.Remove(this);
                }
            }
        }
        public Control.ControlCollection Controls { get { return _controls; } set { _controls = value; } }
        public virtual Rectangle DisplayRectangle { get { return ClientRectangle; } }
        public bool Disposing { get { return _disposing; } }
        public bool Enabled { get; set; }
        public bool Focused { get { return selected; } }
        public virtual Font Font { get; set; }
        public Color ForeColor { get; set; }
        public int Height
        {
            get { return _height; }
            set
            {
                var delta = new Point(0, _height - value);
                _height = value;
                OnResize(delta);
            }
        }
        public bool Hovered { get { return hovered; } }
        public bool IsDisposed { get { return _isDisposed; } }
        public int Left
        {
            get { return this.Location.X; }
            set { Location = new Point(value, Location.Y); }
        }
        public Point Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnLocationChanged(null);
            }
        }
        public virtual Size MaximumSize { get; set; }
        public virtual Size MinimumSize { get; set; }
        public string Name { get; set; }
        internal Point Offset { get; set; }
        public Application Owner { get; internal set; }
        public Padding Padding { get; set; }
        public Control Parent { get { return _parent; } set { _parent = value; } }
        public bool PreventChildDisposing { get; set; }
        public Size Size
        {
            get
            {
                return new Size(Width, Height);
            }
            set
            {
                int widthBuffer = _width;
                int heightBuffer = _height;

                _width = value.Width;
                _height = value.Height;

                OnResize(new Point(widthBuffer - value.Width, heightBuffer - value.Height));
            }
        }
        public bool ShadowBox { get; set; }
        public DrawHandler ShadowHandler { get; set; }
        public int TabIndex { get; set; }
        public object Tag { get; set; }
        public virtual string Text { get; set; }
        public int Top
        {
            get { return this.Location.Y; }
            set { Location = new Point(Location.X, value); }
        }
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                VisibleChanged(this, new EventArgs());
            }
        }
        public int Width
        {
            get { return _width; }
            set
            {
                var delta = new Point(_width - value, 0);
                _width = value;
                OnResize(delta);
            }
        }

        internal string Source { get; set; }

        private void _parent_Resize(Point delta)
        {
            bool an_right = (Anchor & AnchorStyles.Right) == AnchorStyles.Right;
            bool an_bottom = (Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom;
            bool an_left = (Anchor & AnchorStyles.Left) == AnchorStyles.Left;
            bool an_top = (Anchor & AnchorStyles.Top) == AnchorStyles.Top;

            if (Anchor != AnchorStyles.None)
            {
                int prevWidth = _width;
                int prevHeight = _height;

                if (an_right)
                    Location = new Point(Location.X - delta.X, Location.Y);
                else
                    delta = new Point(0, delta.Y);
                if (an_bottom)
                    Location = new Point(Location.X, Location.Y - delta.Y);
                else
                    delta = new Point(delta.X, 0);
                if (an_left)
                {
                    if (an_right)
                    {
                        Location = new Point(Location.X + delta.X, Location.Y);
                        _width = Size.Width - delta.X;
                        delta = new Point(delta.X, delta.Y);
                    }
                }
                if (an_top)
                {
                    if (an_bottom)
                    {
                        Location = new Point(Location.X, Location.Y + delta.Y);
                        _height = Size.Height - delta.Y;
                        delta = new Point(delta.X, delta.Y);
                    }
                }

                // Reset delta for childs.
                if (prevWidth - _width == 0)
                    delta = new Point(0, delta.Y);
                if (prevHeight - _height == 0)
                    delta = new Point(delta.X, 0);

                if ((an_left && an_right) || (an_top && an_bottom))
                    OnResize(delta);
            }
        }

        public Control()
        {
            if (Parent != null && Parent.Owner != null)
                Parent.Owner.Run(this);
            else if (DefaultController != null)
                DefaultController.Run(this);

            Anchor = AnchorStyles.Left | AnchorStyles.Top;
            Controls = new ControlCollection(this);
            Enabled = true;
            Font = DefaultFont;
            ForeColor = Color.Black;
            TabIndex = -1;
            AutoGroup = true;
            _visible = true;

#if UNITY_EDITOR
            var stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            Source = stackTrace;
#endif
        }

        public void BringToFront()
        {
            if (AlwaysFocused) return;

            if (Parent != null)
            {
                Parent.Controls.Remove(this);
                Parent.Controls.Add(this);
            }

            var form = this as Form ?? Application.GetRootControl(this) as Form;
            if (form != null)
            {
                if (this.Owner.Forms.Contains(form))
                {
                    this.Owner.Forms.Remove(form);
                    this.Owner.Forms.Add(form);
                }
                else if (form.IsModal)
                {
                    this.Owner.ModalForms.Remove(form);
                    this.Owner.ModalForms.Add(form);
                }
            }
        }
        public new virtual void Dispose()
        {
            if (_isDisposed) return;

            _disposing = true;
            OnDisposing(this, EventArgs.Empty);

            if (PreventChildDisposing == false)
            {
                for (; Controls.Count > 0;)
                    Controls[0].Dispose();

                Controls.Clear();
            }

            if (Parent != null)
            {
                int self = Parent.Controls.FindIndex(x => x == this);
                if (self > -1)
                    Parent.Controls.RemoveAt(self);
            }

            if (Context)
                Owner.Contexts.Remove(this);

            Disposed(this, null);
            _isDisposed = true;
        }
        public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects, DragDropRenderHandler render = null)
        {
            Application.DoDragDrop(data, allowedEffects, render);
            return DragDropEffects.None; // TODO: ?
        }
        private static void _FocusParent(Control self)
        {
            if (self.Parent != null)
            {
                self.Parent.selected = true;
                _FocusParent(self.Parent);
            }
        }
        public virtual void Focus()
        {
            Select();

            var form = this as Form ?? Application.GetRootControl(this) as Form;
            if (form != null) form.BringToFront();

            GotFocus(this, EventArgs.Empty);
        }
        public void Invalidate()
        {

        }
        public void Invalidate(Rectangle rc)
        {
            // Dunno.
        }
        public Point PointToClient(Point p)
        {
            return PointToClient(new PointF(p.X, p.Y));
        }
        public PointF PointToClient(PointF p)
        {
            if (Parent != null)
                p = Parent.PointToClient(p);

            p.X -= Location.X + Offset.X;
            p.Y -= Location.Y + Offset.Y;
            return p;
        }
        public Point PointToScreen(Point p)
        {
            if (Parent != null)
                p = Parent.PointToScreen(p);

            p.X += Location.X + Offset.X;
            p.Y += Location.Y + Offset.Y;
            return p;
        }
        public virtual void Refresh()
        {
            // In da future: batch stuff through this (like OnResized -> Refresh).
        }
        public void ResumeLayout()
        {

        }
        public void Select()
        {
            if (CanSelect == false || lastSelected == this) return;

            if (lastSelected != null && lastSelected.AlwaysFocused == false)
            {
                lastSelected.selected = false;
                lastSelected.LostFocus(lastSelected, EventArgs.Empty);
            }

            selected = true;
            lastSelected = this;
        }
        public void SuspendLayout()
        {
            // dunno.
        }

        protected virtual void OnClick(EventArgs e)
        {
            Click(this, e);
        }
        protected virtual void OnDoubleClick(EventArgs e)
        {

        }
        protected virtual void OnDragDrop(DragEventArgs drgevent)
        {

        }
        protected virtual void OnDragEnter(DragEventArgs drgevent)
        {

        }
        protected virtual void OnDragLeave(EventArgs e)
        {

        }
        protected virtual void OnLatePaint(PaintEventArgs e)
        {

        }
        protected virtual void OnLocationChanged(EventArgs e)
        {

        }
        protected virtual void OnKeyDown(KeyEventArgs e)
        {

        }
        protected virtual void OnKeyPress(KeyEventArgs e)
        {

        }
        protected virtual void OnKeyUp(KeyEventArgs e)
        {

        }
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            Click(this, e);
        }
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {

        }
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            MouseDown(this, e);
        }
        protected virtual void OnMouseEnter(EventArgs e)
        {
            MouseEnter(this, e);
            if (Enabled)
                hovered = true;
        }
        protected virtual void OnMouseHover(EventArgs e)
        {
            MouseHover(this, e);
        }
        protected virtual void OnMouseLeave(EventArgs e)
        {
            MouseLeave(this, e);
            hovered = false;
        }
        protected virtual void OnMouseMove(MouseEventArgs e)
        {

        }
        protected virtual void OnMouseUp(MouseEventArgs e)
        {

        }
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            // TODO: should I raise in parent?
            // remove base call if you have stack overflow.
            if (Parent != null)
                Parent.RaiseOnMouseWheel(e);
        }
        protected virtual void OnPaint(PaintEventArgs e)
        {

        }
        protected virtual void OnPaintBackground(PaintEventArgs pevent)
        {

        }
        protected virtual object OnPaintEditor(float width)
        {
            System.Windows.Forms.Control controlToSet = null;

            Editor.BeginGroup(width - 24);

            _toggleEditor = Editor.Foldout("Control (" + GetType().Name + ")", _toggleEditor);
            if (_toggleEditor)
            {
                var editorAllowDrop = Editor.BooleanField("AllowDrop", this.AllowDrop);
                if (editorAllowDrop.Changed) this.AllowDrop = editorAllowDrop;

                var editorAlwaysFocused = Editor.BooleanField("AlwaysFocused", this.AlwaysFocused);
                if (editorAlwaysFocused.Changed) this.AlwaysFocused = editorAlwaysFocused;

                var editorAnchor = Editor.EnumField("Anchor", this.Anchor);
                if (editorAnchor.Changed) this.Anchor = (AnchorStyles)editorAnchor.Value;

                var editorAutoSize = Editor.BooleanField("AutoSize", this.AutoSize);
                if (editorAutoSize.Changed) this.AutoSize = editorAutoSize;

                Editor.ColorField("BackColor", this.BackColor, (c) => { this.BackColor = c; });

                var editorBackgroundImageLayout = Editor.EnumField("BackgroundImageLayout", this.BackgroundImageLayout);
                if (editorBackgroundImageLayout.Changed) this.BackgroundImageLayout = (ImageLayout)editorBackgroundImageLayout.Value;

                Editor.Label("Batches", Batches);

                var editorCanSelect = Editor.BooleanField("CanSelect", CanSelect);
                if (editorCanSelect.Changed)
                    CanSelect = editorCanSelect.Value;

                Editor.Label("ClientRectangle", this.ClientRectangle);
                Editor.Label("Context", this.Context);

                if (this.Controls != null && this.Controls.Count > 0)
                {
                    _toggleControls = Editor.Foldout("Controls (" + this.Controls.Count.ToString() + ")", _toggleControls);
                    if (_toggleControls)
                    {
                        Editor.BeginGroup(width - 32);
                        for (int i = 0; i < this.Controls.Count; i++)
                        {
                            var childControl = this.Controls[i];
                            if (Editor.Button(childControl.Name, childControl.GetType().Name))
                                controlToSet = childControl;
                        }
                        Editor.EndGroup();
                    }
                }

                Editor.Label("DisplayRectangle", this.DisplayRectangle);
                Editor.Label("Disposing", this.Disposing);

                var editorEnabled = Editor.BooleanField("Enabled", this.Enabled);
                if (editorEnabled.Changed) Enabled = editorEnabled;

                Editor.Label("Focused", this.Focused);

                _toggleFont = Editor.Foldout("Font", _toggleFont);
                if (_toggleFont)
                {
                    int selectedFont = -1;
                    float size = 12;
                    if (this.Font != null)
                        size = this.Font.Size;
                    System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
                    if (this.Font != null)
                        style = this.Font.Style;
                    List<string> fonts = new List<string>();
                    for (int i = 0; i < System.Windows.Forms.ApplicationBehaviour.Resources.Fonts.Count; i++)
                    {
                        fonts.Add(System.Windows.Forms.ApplicationBehaviour.Resources.Fonts[i].fontNames[0]);
                        if (this.Font != null && this.Font.Name.ToLower() == System.Windows.Forms.ApplicationBehaviour.Resources.Fonts[i].fontNames[0].ToLower())
                            selectedFont = i;
                    }

                    var editorFont = Editor.Popup("      Font", selectedFont, fonts.ToArray());
                    if (editorFont.Changed)
                        this.Font = new System.Drawing.Font(System.Windows.Forms.ApplicationBehaviour.Resources.Fonts[editorFont].fontNames[0], size);

                    var editorFontSize = Editor.Slider("      Size", size, 8, 128);
                    if (editorFontSize.Changed)
                    {
                        if (this.Font == null)
                            this.Font = new System.Drawing.Font("Arial", editorFontSize);
                        else
                            this.Font = new System.Drawing.Font(this.Font.Name, editorFontSize);
                    }

                    var editorFontStyle = Editor.EnumField("      Style", style);
                    if (editorFontStyle.Changed)
                    {
                        if (this.Font == null)
                            this.Font = new System.Drawing.Font("Arial", editorFontSize, (System.Drawing.FontStyle)editorFontStyle.Value);
                        else
                            this.Font = new System.Drawing.Font(this.Font.Name, editorFontSize, (System.Drawing.FontStyle)editorFontStyle.Value);
                    }
                }

                Editor.ColorField("ForeColor", this.ForeColor, (c) => { this.ForeColor = c; });

                var editorHeight = Editor.Slider("Height", this.Height, 0, 4096);
                if (editorHeight.Changed) Height = (int)editorHeight.Value;

                Editor.Label("Hovered", this.Hovered);

                Editor.Label("IsDisposed", this.IsDisposed.ToString());

                var editorLocation = Editor.IntField("Location", this.Location.X, this.Location.Y);
                if (editorLocation.Changed) Location = new Point(editorLocation.Value[0], editorLocation.Value[1]);

                var editorMaximumSize = Editor.IntField("MaximumSize", this.MaximumSize.Width, this.MaximumSize.Height);
                if (editorMaximumSize.Changed) this.MaximumSize = new Drawing.Size(editorMaximumSize.Value[0], editorMaximumSize.Value[1]);

                var editorMinimumSize = Editor.IntField("MinimumSize", this.MinimumSize.Width, this.MinimumSize.Height);
                if (editorMinimumSize.Changed) this.MinimumSize = new Drawing.Size(editorMinimumSize.Value[0], editorMinimumSize.Value[1]);

                var editorName = Editor.TextField("Name", Name ?? "");
                if (editorName.Changed) this.Name = editorName;

                Editor.Label("Offset", Offset);

                var editorPadding = Editor.IntField("Padding", this.Padding.Left, this.Padding.Top, this.Padding.Right, this.Padding.Bottom);
                if (editorPadding.Changed)
                    Padding = new Forms.Padding(editorPadding.Value[0], editorPadding.Value[3], editorPadding.Value[2], editorPadding.Value[1]);

                if (this.Parent != null)
                {
                    if (Editor.Button("Parent", this.Parent.GetType().Name))
                        controlToSet = this.Parent;
                }
                else
                    Editor.Label("Parent", "null");


                var editorShadowBox = Editor.BooleanField("ShadowBox", this.ShadowBox);
                if (editorShadowBox.Changed) ShadowBox = editorShadowBox;

                var editorSize = Editor.IntField("Size", this.Size.Width, this.Size.Height);
                if (editorSize.Changed) this.Size = new Drawing.Size(editorSize.Value[0], editorSize.Value[1]);

                _toggleSource = Editor.Foldout("Source", _toggleSource);
                if (_toggleSource) Editor.Label(this.Source);

                var editorTabIndex = Editor.Slider("TabIndex", this.TabIndex, 0, 255);
                if (editorTabIndex.Changed) this.TabIndex = (int)editorTabIndex.Value;

                var editorText = Editor.TextField("Text", Text ?? "");
                if (editorText.Changed) this.Text = editorText;

                Editor.Label("Type", this.GetType());

                var editorVisible = Editor.BooleanField("Visible", this.Visible);
                if (editorVisible.Changed) this.Visible = editorVisible;

                var editorWidth = Editor.Slider("Width", this.Width, 0, 4096);
                if (editorWidth.Changed) this.Width = (int)editorWidth.Value;

                Editor.NewLine(1);

                if (Editor.Button("BringToFront()")) BringToFront();
                if (Editor.Button("Dispose()")) Dispose();
                if (Editor.Button("Focus()")) Focus();
                if (Editor.Button("Refresh()")) Refresh();
                if (Editor.Button("Select()")) Select();
            }

            Editor.EndGroup();

            return controlToSet;
        }
        protected virtual void OnResize(Point delta)
        {
            if (delta != Point.Zero)
                if (Controls != null)
                    for (int i = 0; i < Controls.Count; i++)
                        Controls[i]._parent_Resize(delta);
            Resize(this, null);
        }
        protected virtual void OnTextChanged(EventArgs e)
        {
            TextChanged(this, e);
        }

        internal void AddjustSizeToScreen(Size delta)
        {
            _parent_Resize(new Point(delta.Width, delta.Height));
        }
        internal Form FindFormInternal()
        {
            Control cur = this;
            while (cur != null && !(cur is Form))
                cur = cur.Parent;

            return (Form)cur;
        }
        internal void RaiseOnDragDrop(DragEventArgs drgevent)
        {
            OnDragDrop(drgevent);
        }
        internal void RaiseOnDragEnter(DragEventArgs drgevent)
        {
            OnDragEnter(drgevent);
        }
        internal void RaiseOnDragLeave(EventArgs e)
        {
            OnDragLeave(e);
        }
        internal void RaiseOnMouseClick(MouseEventArgs e)
        {
            OnMouseClick(e);
        }
        internal void RaiseOnMouseDoubleClick(MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
            OnDoubleClick(e);
        }
        internal void RaiseOnMouseDown(MouseEventArgs e)
        {
            OnMouseDown(e);
        }
        internal void RaiseOnMouseEnter(EventArgs e)
        {
            OnMouseEnter(e);
        }
        internal void RaiseOnMouseHover(EventArgs e)
        {
            OnMouseHover(e);
        }
        internal void RaiseOnMouseLeave(EventArgs e)
        {
            OnMouseLeave(e);
        }
        internal void RaiseOnMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
        }
        internal void RaiseOnMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
            MouseUp(this, e);

            if (Owner != null && ApplicationBehaviour.ShowControlProperties && Application.ShowCallback != null)
                Application.ShowCallback.Invoke(this);
        }
        internal void RaiseOnMouseWheel(MouseEventArgs e)
        {
            OnMouseWheel(e);
        }
        internal void RaiseOnKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
            KeyDown(this, e);
        }
        internal void RaiseOnKeyPress(KeyEventArgs e)
        {
            OnKeyPress(e);
        }
        internal void RaiseOnKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
            KeyUp(this, e);
        }
        internal void RaiseOnPaint(PaintEventArgs e)
        {
            e.Graphics.Control = this;
            Batches = 0;

            if (ShadowBox)
            {
                if (ShadowHandler == null)
                {
                    int shX = e.ClipRectangle.X + 6;
                    int shY = e.ClipRectangle.Y + 6;
                    var shadowColor = Color.FromArgb(12, 64, 64, 64);
                    e.Graphics.FillRectangle(shadowColor, shX + 6, shY + 6, Width - 12, Height - 12);
                    e.Graphics.FillRectangle(shadowColor, shX + 5, shY + 5, Width - 10, Height - 10);
                    e.Graphics.FillRectangle(shadowColor, shX + 4, shY + 4, Width - 8, Height - 8);
                    e.Graphics.FillRectangle(shadowColor, shX + 3, shY + 3, Width - 6, Height - 6);
                    e.Graphics.FillRectangle(shadowColor, shX + 2, shY + 2, Width - 4, Height - 4);
                }
                else
                    ShadowHandler.Invoke(e);
            }

            if (AutoGroup)
            {
                int gx = e.ClipRectangle.X;
                int gy = e.ClipRectangle.Y;

                if (gx != 0 && gy != 9) // Reset?
                    e.ClipRectangle = new Rectangle(0, 0, Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

                e.Graphics.Group = new Drawing.Rectangle(gx, gy, Width, Height);
                e.Graphics.GroupBegin(this);
            }
            OnPaintBackground(e);
            OnPaint(e);
            if (Controls != null)
                for (int i = 0; i < Controls.Count; i++)
                {
                    if (Application.ControlIsVisible(Controls[i]) == false) continue;

                    var currentAbspos = Controls[i].PointToScreen(System.Drawing.Point.Zero);
                    if (currentAbspos.X + Controls[i].Width < 0 || currentAbspos.X > Screen.PrimaryScreen.WorkingArea.Width ||
                    currentAbspos.Y + Controls[i].Height < 0 || currentAbspos.Y > Screen.PrimaryScreen.WorkingArea.Height)
                        continue;

                    e.Graphics.Control = Controls[i];
                    Controls[i].RaiseOnPaint(e);
                }
            e.Graphics.Control = this;
            if (Application.Debug)
            {
                e.Graphics.DrawString(GetType().Name, Font, Brushes.White, 3, 1, 256, 32);
                e.Graphics.DrawString(GetType().Name, Font, Brushes.White, 5, 3, 256, 32);
                e.Graphics.DrawString(GetType().Name, Font, Brushes.DarkRed, 4, 2, 256, 32);
                e.Graphics.DrawRectangle(Pens.DarkRed, 0, 0, Width, Height);
            }
            OnLatePaint(e);
            if (AutoGroup)
                e.Graphics.GroupEnd();
            Batched = true;

            e.Graphics.Control = null;
        }
        internal object RaiseOnPaintEditor(float width)
        {
            return OnPaintEditor(width);
        }
        internal void RaiseOnResize(Point delta)
        {
            OnResize(delta);
        }

        public event EventHandler Click = delegate { };
        public new event EventHandler Disposed = delegate { };
        public event EventHandler GotFocus = delegate { };
        public event EventHandler LostFocus = delegate { };
        public event KeyEventHandler KeyDown = delegate { };
        public event KeyEventHandler KeyUp = delegate { };
        public event MouseEventHandler MouseDown = delegate { };
        public event EventHandler MouseEnter = delegate { };
        public event EventHandler MouseHover = delegate { };
        public event EventHandler MouseLeave = delegate { };
        public event MouseEventHandler MouseUp = delegate { };
        public event EventHandler OnDisposing = delegate { };
        public event EventHandler Resize = delegate { };
        public event EventHandler TextChanged = delegate { };
        public event EventHandler VisibleChanged = delegate { };

        public delegate void DrawHandler(PaintEventArgs e);
        public delegate void ResizeHandler(Point delta);

        public class ControlCollection : IEnumerator<Control>, IEnumerable<Control>, IList
        {
            private readonly List<Control> _items = new List<Control>();
            private readonly Control _owner;

            public ControlCollection(Control owner)
            {
                _owner = owner;
            }

            public virtual Control this[int index] { get { return _items[index]; } }

            public int Count { get { return _items.Count; } }
            public Control Owner { get { return _owner; } }

            object IEnumerator.Current
            {
                get { return _items.GetEnumerator().Current; }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
            bool IEnumerator.MoveNext()
            {
                return _items.GetEnumerator().MoveNext();
            }
            public virtual void Add(Control value)
            {
                _items.Add(value);
                value.Parent = _owner;
            }
            public virtual void AddRange(Control[] controls)
            {
                foreach (var c in controls)
                {
                    _items.Add(c);
                    c.Parent = _owner;
                }
            }
            public void Clear()
            {
                _items.Clear();
            }
            public bool Contains(Control control)
            {
                return _items.Contains(control);
            }
            public Control Current
            {
                get { return _items.GetEnumerator().Current; }
            }
            public void Dispose()
            {
                _items.GetEnumerator().Dispose();
            }
            public Control Find(Predicate<Control> match)
            {
                return _items.Find(match);
            }
            public int FindIndex(Predicate<Control> match)
            {
                return _items.FindIndex(match);
            }
            public IEnumerator<Control> GetEnumerator()
            {
                return _items.GetEnumerator();
            }
            public int IndexOf(Control control)
            {
                return _items.IndexOf(control);
            }
            public void Insert(int index, Control value)
            {
                _items.Insert(index, value);
            }
            public virtual void Remove(Control item)
            {
                _items.Remove(item);
            }
            public void RemoveAt(int index)
            {
                _items.RemoveAt(index);
            }
            public void Reset()
            {

            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }
            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }
            public object SyncRoot
            {
                get
                {
                    return null;
                }
            }
            object IList.this[int index]
            {
                get
                {
                    return _items[index];
                }
                set
                {
                    if (value is Control)
                        _items[index] = (Control)value;
                }
            }

            int IList.Add(object value)
            {
                var control = value as Control;
                if (control != null)
                {
                    Add(control);
                    return _items.Count - 1;
                }
                return -1;
            }
            bool IList.Contains(object value)
            {
                var control = value as Control;
                if (control != null)
                    return _items.Contains(control);
                return false;
            }
            int IList.IndexOf(object value)
            {
                var control = value as Control;
                if (control != null)
                    return _items.IndexOf(control);
                return -1;
            }
            void IList.Insert(int index, object value)
            {
                var control = value as Control;
                if (control != null)
                    _items.Insert(index, control);
            }
            void IList.Remove(object value)
            {
                var control = value as Control;
                if (control != null)
                    _items.Remove(control);
            }
            public void CopyTo(Array array, int index)
            {
                var controls = array as Control[];
                if (controls != null)
                    _items.CopyTo(controls, index);
            }
        }
    }

    public delegate void DragDropRenderHandler(Graphics g);
}