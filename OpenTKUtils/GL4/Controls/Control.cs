﻿/*
 * Copyright © 2019-2020 Robbyxp1 @ github.com
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

// Rules - no winforms in Control land except for Keys
 
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public struct Padding
    {
        public int Left; public int Top; public int Right; public int Bottom;
        public Padding(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public Padding(int pad = 0) { Left = pad; Top = pad; Right = pad; Bottom = pad; }
        public int TotalWidth { get { return Left + Right; } }
        public int TotalHeight { get { return Top + Bottom; } }

        public static bool operator ==(Padding l, Padding r) { return l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom; }
        public static bool operator !=(Padding l, Padding r) { return !(l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom); }
        public override bool Equals(Object other) { return other is Padding && this == (Padding)other; }
        public override int GetHashCode() { return base.GetHashCode(); }
    };

    public struct Margin
    {
        public int Left; public int Top; public int Right; public int Bottom;
        public Margin(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public Margin(int pad = 0) { Left = pad; Top = pad; Right = pad; Bottom = pad; }
        public int TotalWidth { get { return Left + Right; } }
        public int TotalHeight { get { return Top + Bottom; } }

        public static bool operator ==(Margin l, Margin r) { return l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom; }
        public static bool operator !=(Margin l, Margin r) { return !(l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom); }
        public override bool Equals(Object other) { return other is Margin && this == (Margin)other; }
        public override int GetHashCode() { return base.GetHashCode(); }
    };

    public enum CheckState { Unchecked, Checked, Indeterminate };

    public enum Appearance
    {
        Normal = 0,
        Button = 1
    }

    // Others - DockFlowRight - pass to next item the position of last item, and flow left, with wrap, keep the area to the max height used
    // Others - DockFlowDown- pass to next item the position of last item, and flow Down, then top down again, with wrap, keep the area to the max width used
    // bottomcentre, topcentre.

    public enum DockingType { None, Left, Right, Top, Bottom, Fill, Center, LeftCenter, RightCenter };

    public abstract class GLBaseControl
    {
        #region Main UI
        public string Name { get; set; } = "?";

        // bounds of the window - include all margin/padding/borders/
        // co-ords are in offsets from 0,0 being the parent top left corner.

        public Rectangle Bounds { get { return window; } set { SetPos(value.Left, value.Top, value.Width, value.Height); } }
        public int Left { get { return window.Left; } set { SetPos(value, window.Top, window.Width, window.Height); } }
        public int Right { get { return window.Right; } set { SetPos(window.Left, window.Top, value - window.Left, window.Height); } }
        public int Top { get { return window.Top; } set { SetPos(window.Left, value, window.Width, window.Height); } }
        public int Bottom { get { return window.Bottom; } set { SetPos(window.Left, window.Top, window.Width, value - window.Top); } }
        public int Width { get { return window.Width; } set { SetPos(window.Left, window.Top, value, window.Height); } }
        public int Height { get { return window.Height; } set { SetPos(window.Left, window.Top, window.Width, value); } }
        public Point Location { get { return new Point(window.Left, window.Top); } set { SetPos(value.X, value.Y, window.Width, window.Height); } }
        public Size Size { get { return new Size(window.Width, window.Height); } set { SetPos(window.Left, window.Top, value.Width, value.Height); } }
        public virtual void SizeClipped(Size s) { SetPos(window.Left, window.Top, Math.Min(Width, s.Width), Math.Min(Height, s.Height)); }

        // this is the client area, inside the margin/padding/border

        public int ClientLeftMargin { get { return Margin.Left + Padding.Left + BorderWidth; } }
        public int ClientRightMargin { get { return Margin.Right + Padding.Right + BorderWidth; } }
        public int ClientTopMargin { get { return Margin.Top + Padding.Top + BorderWidth; } }
        public int ClientBottomMargin { get { return Margin.Bottom + Padding.Bottom + BorderWidth; } }
        public int ClientWidth { get { return Width - Margin.TotalWidth - Padding.TotalWidth - BorderWidth*2; } }
        public int ClientHeight { get { return Height - Margin.TotalHeight - Padding.TotalHeight - BorderWidth*2; } }
        public Rectangle ClientRectangle { get { return new Rectangle(0,0,ClientWidth,ClientHeight); }  }

        public DockingType Dock { get { return docktype; } set { if (docktype != value) { docktype = value; InvalidateLayoutParent(); } } }
        public float DockPercent { get { return dockpercent; } set { if (value != dockpercent) { dockpercent = value; InvalidateLayoutParent(); } } }        // % in 0-1 terms used to dock on left,top,right,bottom.  0 means just use width/height

        public GLBaseControl Parent { get { return parent; } }
        public Font Font { get { return font ?? parent?.Font; } set { SetFont(value); InvalidateLayout(); } }

        public Color BackColor { get { return backcolor; } set { if (backcolor != value) { backcolor = value; Invalidate(); } } }
        public int BackColorGradient { get { return backcolorgradient;} set { if ( backcolorgradient != value) { backcolorgradient = value;Invalidate(); } } }
        public Color BackColorGradientAlt { get { return backcolorgradientalt; } set { if (backcolorgradientalt != value) { backcolorgradientalt = value; Invalidate(); } } }
        public Color BorderColor { get { return bordercolor; } set { if (bordercolor != value) { bordercolor = value; Invalidate(); } } }
        public int BorderWidth { get { return borderwidth; } set { if (borderwidth != value) { borderwidth = value; InvalidateLayout(); } } }

        public GL4.Controls.Padding Padding { get { return padding; } set { if (padding != value) { padding = value; InvalidateLayout(); } } }
        public GL4.Controls.Margin Margin { get { return margin; } set { if (margin != value) { margin = value; InvalidateLayout(); } } }
        public void SetMarginBorderWidth(Margin m, int borderw, Color borderc, Padding p) { margin = m; padding = p; bordercolor = borderc; borderwidth = borderw; InvalidateLayout(); }

        public bool Enabled { get { return enabled; } set { if (enabled != value) { SetEnabled(value); Invalidate(); } } }
        public bool Visible { get { return visible; } set { if (visible != value) { visible = value; InvalidateLayoutParent(); } } }

        public bool AutoSize { get { return autosize; } set { if (autosize != value) { autosize = value; InvalidateLayoutParent(); } } }

        public virtual bool Focused { get { return focused; }  } 
        public virtual bool Focusable { get { return focusable; } set { focusable = value; } }
        public virtual void SetFocus()  { FindDisplay()?.SetFocus(this); }

        public int Row { get { return row; } set { row = value; InvalidateLayoutParent(); } }
        public int Column { get { return column; } set { column = value; InvalidateLayoutParent(); } }

        public bool InvalidateOnEnterLeave { get; set; } = false;       // if set, invalidate on enter/leave to force a redraw
        public bool InvalidateOnMouseDownUp { get; set; } = false;      // if set, invalidate on mouse button down/up to force a redraw
        public bool InvalidateOnFocusChange { get; set; } = false;      // if set, invalidate on focus change

        public bool Hover { get; set; } = false;            // mouse is over control
        public MouseEventArgs.MouseButtons MouseButtonsDown { get; set; } // set if mouse buttons down over control

        public GLControlDisplay FindDisplay() { return this is GLControlDisplay ? this as GLControlDisplay : parent?.FindDisplay(); }
        public Bitmap GetLevelBitmap { get { return levelbmp; } }

        public virtual List<GLBaseControl> Controls { get { return children.ToList(); } }      // read only

        public Action<Object, MouseEventArgs> MouseDown { get; set; } = null;  // location in client terms, NonClientArea set if on border with negative/too big x/y for clients
        public Action<Object, MouseEventArgs> MouseUp { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseMove { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseClick { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseWheel { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseEnter { get; set; } = null;  // location in terms of whole window
        public Action<Object, MouseEventArgs> MouseLeave { get; set; } = null;  // location in terms of whole window
        public Action<Object, KeyEventArgs> KeyDown { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyUp { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyPress { get; set; } = null;
        public Action<Object, bool> FocusChanged { get; set; } = null;
        public Action<Object> FontChanged { get; set; } = null;
        public Action<Object> Resize { get; set; } = null;
        public Action<GLBaseControl,GLBaseControl> ControlAdd { get; set; } = null;
        public Action<GLBaseControl,GLBaseControl> ControlRemove { get; set; } = null;

        public GLBaseControl(GLBaseControl p = null)
        {
            parent = p;
            children = new LinkedList<GLBaseControl>();
            window = new Rectangle(0, 0, 100, 100);
        }

        public void Invalidate()
        {
            //System.Diagnostics.Debug.WriteLine("Invalidate " + Name);
            NeedRedraw = true;
            var f = FindDisplay();
            if (f != null)
                f.RequestRender = true;
        }

        public void InvalidateLayout()
        {
            Invalidate();
            PerformLayout();
        }

        public void InvalidateLayoutParent()
        {
            //System.Diagnostics.Debug.WriteLine("Invalidate Layout Parent " + Name);
            NeedRedraw = true;
            if (parent != null)
            {
                var f = FindDisplay();
                if (f != null)
                    f.RequestRender = true;
                //System.Diagnostics.Debug.WriteLine(".. Redraw and layout on " + Parent.Name);
                parent.NeedRedraw = true;
                parent.PerformLayout();
            }
        }

        public Point DisplayControlCoords(bool adjustclient)       
        {
            Point p = Location;     // Left/Top of bounding box
            GLBaseControl b = this;
            while ( b.Parent != null )
            {       // we need to add on the parent left and clientleftmargin, top the same, to move the point up to the next level
                p = new Point(p.X + b.parent.Left + b.parent.ClientLeftMargin, p.Y + b.parent.Top + b.parent.ClientTopMargin);
                b = b.parent;
            }
            if (adjustclient)
            {
                p.X += ClientLeftMargin;
                p.Y += ClientTopMargin;
            }
            return p;
        }

        public Rectangle ChildArea()
        {
            int left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;

            foreach (var c in children)         // first let all children autosize
            {
                if (c.Left < left)
                    left = c.Left;
                if (c.Right > right)
                    right = c.Right;
                if (c.Top < top)
                    top = c.Top;
                if (c.Bottom > bottom)
                    bottom = c.Bottom;
            }

            return new Rectangle(left, top, right - left, bottom - top);
        }

        public void SuspendLayout()
        {
            suspendLayoutSet = true;
        }

        public void ResumeLayout()
        {
            suspendLayoutSet = false;
            if (needLayout)
            {
                //System.Diagnostics.Debug.WriteLine("Resumed layout on " + Name);
                PerformLayout();
                needLayout = false;
            }
        }

        public virtual void Add(GLBaseControl other)
        {
            other.parent = this;
            children.AddFirst(other);

            if (this is GLControlDisplay) // if adding to a form, the child must have a bitmap
            {
                System.Diagnostics.Debug.Assert(other is GLVerticalScrollPanel == false, "GLScrollPanel must not be child of GLForm");
                other.levelbmp = new Bitmap(other.Width, other.Height);
            }

            OnControlAdd(this, other);
            Invalidate();           // we are invalidated
            PerformLayout();        // reperform layout
        }

        public virtual void Remove(GLBaseControl other)
        {
            if (children.Contains(other))
            {
                OnControlRemove(this, other);

                FindDisplay()?.ControlRemoved(other);

                if (other.levelbmp != null)
                    other.levelbmp.Dispose();

                children.Remove(other);
                
                Invalidate();
                PerformLayout();        // reperform layout
            }
        }

        #endregion

        #region For Inheritors

        protected bool CheckSuspendedLayout() // call to see if suspended in PerformLayout overrides
        {
            if (suspendLayoutSet)
            {
                needLayout = true;
                return true;
            }
            else
                return false;
        }

        protected GLBaseControl FindControlOver(Point p)       // p = form co-coords, finds including margin/padding/border area, so inside bounds
        {
            //System.Diagnostics.Debug.WriteLine("Find " + Name + " "  + p + " in " + Bounds + " " + ClientLeftMargin + " " + ClientTopMargin);
            if (p.X < Left || p.X > Right || p.Y < Top || p.Y > Bottom)     
                return null;

            foreach (GLBaseControl c in children)
            {
                var r = c.FindControlOver(new Point(p.X - Left - ClientLeftMargin, p.Y - Top - ClientTopMargin));   // find, converting co-ords into child co-ords
                if (r != null)
                    return r;
            }

            return this;
        }

        protected void PerformSizeChildren(GLBaseControl c)
        {
            c.inautosize = true;            // this flag stops reentrancy due to size changes
            c.PerformSize();
            c.inautosize = false;
        }

        #endregion

        #region Overridables

        public virtual void PerformLayout()     // override for other layouts
        {
            if (suspendLayoutSet)
            {
                needLayout = true;
                //System.Diagnostics.Debug.WriteLine("Suspend layout on " + Name);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Perform layout on " + Name);

                foreach (var c in children)         // first let all children autosize
                {
                    if ( c.Visible)     // invisible children don't layout
                        PerformSizeChildren(c);
                }

                // now pass the client rectangle to the layour engines

                Rectangle area = ClientRectangle;

                foreach (var c in children)
                {
                    if (c.Visible)      // invisible children don't layout
                    {
                        area = c.Layout(area);
                        c.PerformLayout();
                    }
                }
            }
        }

        public virtual Rectangle Layout(Rectangle area)      // layout yourself inside the area, return area left.
        {
            int ws = DockPercent > 0 ? ((int)(area.Width * DockPercent)) : window.Width;
            ws = Math.Min(ws, area.Width);
            int hs = DockPercent > 0 ? ((int)(area.Height * DockPercent)) : window.Height;
            hs = Math.Min(hs, area.Height);
            int wl = Math.Min(area.Width, Width);
            int hl = Math.Min(area.Height, Height);

            Rectangle oldwindow = window;
            Rectangle areaout = area;

            if (docktype == DockingType.Fill)
            {
                window = area;
                areaout = new Rectangle(0, 0, 0, 0);
            }
            else if (docktype == DockingType.Center)
            {
                int xcentre = (area.Left + area.Right) / 2;
                int ycentre = (area.Top + area.Bottom) / 2;
                Width = Math.Min(area.Width, Width);
                Height = Math.Min(area.Height, Height);
                window = new Rectangle(xcentre - Width / 2, ycentre - Height / 2, Width, Height);       // centre in area, bounded by area, no change in area in
            }
            else if (docktype == DockingType.Left)
            {
                window = new Rectangle(area.Left, area.Top, ws, area.Height);
                areaout = new Rectangle(area.Left + ws, area.Top, area.Width - ws, area.Height);
            }
            else if (docktype == DockingType.LeftCenter)
            {
                window = new Rectangle(area.Left, area.Top + area.Height / 2 - hl / 2, ws, hl);
                areaout = new Rectangle(area.Left + ws, area.Top, area.Width - ws, area.Height);
            }
            else if (docktype == DockingType.Right)
            {
                window = new Rectangle(area.Right - ws, area.Top, ws, area.Height);
                areaout = new Rectangle(area.Left, area.Top, area.Width - window.Width, area.Height);
            }
            else if (docktype == DockingType.RightCenter)
            {
                window = new Rectangle(area.Right - ws, area.Top + area.Height / 2 - hl / 2, ws, hl);
                areaout = new Rectangle(window.Left, area.Top, area.Width - window.Width, area.Height);
            }
            else if (docktype == DockingType.Top)
            {
                window = new Rectangle(area.Left, area.Top, area.Width, hs);
                areaout = new Rectangle(area.Left, area.Top + hs, area.Width, area.Height - hs);
            }
            else if (docktype == DockingType.Bottom)
            {
                window = new Rectangle(area.Left, area.Bottom - hs, area.Width, hs);
                areaout = new Rectangle(area.Left, area.Top, area.Width, area.Height - hs);
            }
            else
            {   // any other docking - we just leave window alone and area alone.
            }

            System.Diagnostics.Debug.WriteLine("{0} dock {1} win {2} Area in {3} Area out {4}", Name, Dock, window, area, areaout);

            if (oldwindow.Size != window.Size) // if window size changed
            {
                OnResize();

                if (levelbmp != null && !(this is GLVerticalScrollPanel))       // if changed size, and not a scroll panel, we resize the bitmap
                {
                    levelbmp.Dispose();
                    levelbmp = new Bitmap(Width, Height);       // occurs for controls directly under form
                }
            }

            return areaout;
        }

        // redraw, into usebmp
        // drawarea = area that our control occupies on the bitmap, in bitmap co-ords, which may be outside of the clip area
        // cliparea = area that we can draw into, in bitmap co-ords, so we don't exceed the bounds of any parent clip areas
        // gr = graphics to draw into
        // we must be visible to be called. Children may not be visible

        public virtual bool Redraw(Bitmap usebmp, Rectangle bounds, Rectangle cliparea, Graphics gr, bool forceredraw)
        {
            Graphics parentgr = null;                           // if we changed level bmp, we need to give the control the opportunity
            Rectangle parentarea = bounds;                    // to paint thru its level bmp to the parent bmp

            if (levelbmp != null)                               // bitmap on this level, use it for itself and its children
            {
                if ( usebmp != null )                           // must have a bitmap to paint thru to
                    parentgr = gr;                              // allow parent paint thru

                usebmp = levelbmp;

                cliparea = bounds = new Rectangle(0, 0, usebmp.Width, usebmp.Height);      // restate area in terms of bitmap, this is the bounds

                gr = Graphics.FromImage(usebmp);        // get graphics for it
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            bool redrawn = false;

            if (NeedRedraw || forceredraw)          // if we need a redraw, or we are forced to draw by a parent redrawing above us.
            {
                System.Diagnostics.Debug.WriteLine("redraw {0}->{1} Bounds {2} clip {3} nr {4} fr {5}", Parent?.Name, Name, bounds, cliparea, NeedRedraw, forceredraw);

                gr.SetClip(cliparea);   // set graphics to the clip area so we can draw the background/border

                DrawBack(bounds,gr, BackColor, BackColorGradientAlt, BackColorGradient);

                if (BorderWidth>0)
                {
                    Rectangle rectarea = new Rectangle(bounds.Left + Margin.Left,
                                                    bounds.Top + Margin.Top,
                                                    bounds.Width - Margin.TotalWidth - 1,
                                                    bounds.Height - Margin.TotalHeight - 1);

                    using (var p = new Pen(BorderColor, BorderWidth))
                    {
                        gr.DrawRectangle(p, rectarea);
                    }
                }

                forceredraw = true;             // all children, force redraw
                NeedRedraw = false;             // we have been redrawn

                redrawn = true;                 // and signal up we have been redrawn
            }

            // client area, in terms of last bitmap
            Rectangle clientarea = new Rectangle(bounds.Left + ClientLeftMargin, bounds.Top + ClientTopMargin, ClientWidth, ClientHeight);

            LinkedListNode<GLBaseControl> pos = children.Last;      // render in order from last z to first z.
            while( pos != null)
            {
                var c = pos.Value;

                if (c.Visible)
                {
                    Rectangle childbounds = new Rectangle(clientarea.Left + c.Left,     // not bounded by clip area.
                                                          clientarea.Top + c.Top,
                                                          c.Width,
                                                          c.Height);

                    int cleft = Math.Max(childbounds.Left, cliparea.Left); // update clip area for child based on its bounds, limited to current clip
                    int ctop = Math.Max(childbounds.Top, cliparea.Top);
                    int cright = Math.Min(childbounds.Left + c.Width, cliparea.Right);
                    int cbot = Math.Min(childbounds.Top + c.Height, cliparea.Bottom);
                    Rectangle childcliparea = new Rectangle(cleft, ctop, cright - cleft, cbot - ctop);

                    redrawn |= c.Redraw(usebmp, childbounds, childcliparea, gr, forceredraw);
                }

                pos = pos.Previous;
            }

            if ( forceredraw)       // will be set if NeedRedrawn or forceredrawn
            {
                gr.SetClip(cliparea);   // set graphics to the clip area
                Paint(clientarea, gr);

                if (parentgr != null)      // give us a chance of parent paint thru
                {
                    parentgr.SetClip(parentarea);       // must set the clip area again to address the parent area
                    PaintParent(parentarea, parentgr);
                }
            }

            if (levelbmp != null)                               // bitmap on this level, we made a GR, dispose
                gr.Dispose();

            return redrawn;
        }

        // draw back area - call if you wish to override the background area with another colour

        protected virtual void DrawBack(Rectangle bounds, Graphics gr, Color bc, Color bcgradientalt, int bcgradient)
        {
            if (bc != Color.Transparent)
            {
                gr.Clear(Color.Transparent);        // reset back to transparent, if we don't, and we draw with alpha, successive draws cause the colour to build up

                if (bcgradient != int.MinValue)
                {
                    //System.Diagnostics.Debug.WriteLine("Background " + Name +  " " + bounds + " " + bc + " -> " + bcgradientalt );
                    using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, bc, bcgradientalt, bcgradient))
                        gr.FillRectangle(b, bounds);       // linear grad brushes do not respect smoothing mode, btw
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Background " + Name + " " + bounds + " " + backcolor);
                    using (Brush b = new SolidBrush(bc))     // always fill, so we get back to start
                        gr.FillRectangle(b, bounds);
                }
            }
        }

        public virtual void PerformSize()        // see if you want to resize
        {
        }

        public virtual void Paint(Rectangle area, Graphics gr)      // normal override
        {
            //System.Diagnostics.Debug.WriteLine("Paint {0} to {1}", Name, area);
        }

        public virtual void PaintParent(Rectangle parentarea, Graphics parentgr) // only called if you've defined a bitmap yourself, 
        {                                                                        // gives you a chance to paint to the parent bitmap
           // System.Diagnostics.Debug.WriteLine("Paint Into parent {0} to {1}", Name, parentarea);
        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("leave " + Name + " " + e.Location);
            MouseLeave?.Invoke(this, e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("enter " + Name + " " + e.Location);
            MouseEnter?.Invoke(this, e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("up   " + Name + " " + e.Location + " " + e.Button);
            MouseUp?.Invoke(this, e);

            if (InvalidateOnMouseDownUp)
                Invalidate();
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("down " + Name + " " + e.Location +" " + e.Button);
            MouseDown?.Invoke(this, e);

            if (InvalidateOnMouseDownUp)
                Invalidate();
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("click " + Name + " " + e.Button + " " + e.Clicks + " " + e.Location);
            MouseClick?.Invoke(this, e);
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseMove?.Invoke(this, e);
        }

        public virtual void OnMouseWheel(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseWheel?.Invoke(this, e);
        }

        public delegate void KeyFunc(KeyEventArgs e);
        public void CallKeyFunction(KeyFunc f, KeyEventArgs e)
        {
            f.Invoke(e);
        }

        public virtual void OnKeyDown(KeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(KeyEventArgs e)
        {
            KeyUp?.Invoke(this, e);
        }

        public virtual void OnKeyPress(KeyEventArgs e)
        {
            KeyPress?.Invoke(this, e);
        }

        public virtual void OnFocusChanged(bool focused)
        {
            this.focused = focused;
            if (InvalidateOnFocusChange)
                Invalidate();
            FocusChanged?.Invoke(this, focused);
        }

        public virtual void OnFontChanged()
        {
            FontChanged?.Invoke(this);
        }

        public virtual void OnResize()
        {
            Resize?.Invoke(this);
        }

        public virtual void OnControlAdd(GLBaseControl parent, GLBaseControl child)
        {
            ControlAdd?.Invoke(parent, child);
        }

        public virtual void OnControlRemove(GLBaseControl parent, GLBaseControl child)
        {
            ControlRemove?.Invoke(parent, child);
        }

        #endregion


        #region Implementation


        private void SetPos(int left, int top, int width, int height) // change window rectangle
        {
            Rectangle w = new Rectangle(left, top, width, height);

            if (w != window)        // if changed
            {
                window = w;

                NeedRedraw = true;      // we need a redraw
                //System.Diagnostics.Debug.WriteLine("setpos need redraw on " + Name);
                parent?.Invalidate();   // parent is invalidated as well, and the whole form needs reendering

                if (!inautosize)        // if not in autosize, then we need to perform a layout
                {
                    parent?.PerformLayout();     // go up one and perform layout on all its children, since we are part of it.
                }
            }
        }

        private void SetEnabled(bool v)
        {
            enabled = v;
            foreach (var c in children)
                SetEnabled(v);
        }

        private void SetFont(Font f)
        {
            font = f;
            PropergateFontChanged(this);
        }

        private void PropergateFontChanged(GLBaseControl p)
        {
            p.OnFontChanged();
            foreach (var c in p.children)
            {
                if (c.Font == null)
                    PropergateFontChanged(c);
            }
        }

        public virtual void DebugWhoWantsRedraw()
        {
            //if (NeedRedraw)
                System.Diagnostics.Debug.WriteLine("Debug Redraw Flag " + Name + " " + NeedRedraw);

            foreach (var c in children)
                c.DebugWhoWantsRedraw();
        }

        //tbd
        protected bool NeedRedraw { get; set; } = true;         // we need to redraw, therefore all children also redraw
        protected Bitmap levelbmp;       // set if the level has a new bitmap.  Controls under Form always does. Other ones may if they scroll
        protected Rectangle window;       // total area owned, in parent co-ords
        protected Font font;

        private bool needLayout { get; set; } = false;        // need a layout after suspend layout was called
        private bool suspendLayoutSet { get; set; } = false;        // suspend layout is on
        private bool inautosize { get; set; } = false;        // changing size in autosize 
        private bool enabled { get; set; } = true;
        private bool visible { get; set; } = true;
        private DockingType docktype { get; set; } = DockingType.None;
        private float dockpercent { get; set; } = 0;
        private Color backcolor { get; set; } = Color.Transparent;
        private Color backcolorgradientalt { get; set; } = Color.Black;
        private int backcolorgradient { get; set; } = int.MinValue;           // in degrees
        private Color bordercolor { get; set; } = Color.Transparent;         // Margin - border - padding is common to all controls. Area left is control area to draw in
        private int borderwidth { get; set; } = 0;
        private GL4.Controls.Padding padding { get; set; }
        private GL4.Controls.Margin margin { get; set; }
        private bool autosize { get; set; }
        private int column { get; set; } = 0;     // for table layouts
        private int row { get; set; } = 0;        // for table layouts
        private bool focused { get; set; } = false;
        private bool focusable { get; set; } = false;

        private GLBaseControl parent { get; set; } = null;       // its parent, null if top of top
        protected LinkedList<GLBaseControl> children;   // its children.  First is at the top of the Z list and gets first layed out and last rendered


        #endregion


    }
}
