// DragNotebook.cs
//
// Author:
//   Todd Berman  <tberman@off.net>
//
// Copyright (c) 2004 Todd Berman
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System.Linq;
using Gdk;
using Gtk;
using System;
using Mono.TextEditor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cairo;

namespace MonoDevelop.Ide.Gui
{
	public delegate void TabsReorderedHandler (Widget widget, int oldPlacement, int newPlacement);

	class DockNotebook : Gtk.VBox
    {
		List<IDockNotebookTab> pages = new List<IDockNotebookTab> ();
		TabStrip tabStrip;
		Gtk.EventBox contentBox;
		Gtk.Alignment topBarAlign;
		ReadOnlyCollection<IDockNotebookTab> pagesCol;

		IDockNotebookTab currentTab;

		public DockNotebook ()
		{
			pagesCol = new ReadOnlyCollection<IDockNotebookTab> (pages);
			ButtonPressEvent += new ButtonPressEventHandler (OnButtonPress);
			ButtonReleaseEvent += new ButtonReleaseEventHandler (OnButtonRelease);
			AddEvents ((Int32) (EventMask.AllEventsMask));

			tabStrip = new TabStrip (this);

			PackStart (tabStrip, false, false, 0);

			contentBox = new EventBox ();
			PackStart (contentBox, true, true, 0);

			ShowAll ();
		}
		
		Cursor fleurCursor = new Cursor (CursorType.Fleur);

		public event TabsReorderedHandler TabsReordered;
		public event EventHandler<TabEventArgs> TabClosed;

		public event EventHandler PageAdded;
		public event EventHandler PageRemoved;
		public event EventHandler SwitchPage;

		public event EventHandler PreviousClicked;
		public event EventHandler NextClicked;

		public ReadOnlyCollection<IDockNotebookTab> Tabs {
			get { return pagesCol; }
		}

		public IDockNotebookTab CurrentTab {
			get { return currentTab; }
			set {
				currentTab = value;
				if (contentBox.Child != null)
					contentBox.Remove (contentBox.Child);

				if (currentTab != null)
					contentBox.Add (currentTab.Content);

				tabStrip.Update ();
			}
		}

		public int CurrentTabIndex {
			get { return currentTab != null ? currentTab.Index : -1; }
			set { CurrentTab = pages [value]; }
		}

		public int TabCount {
			get { return pages.Count; }
		}

		public IDockNotebookTab InsertTab (int index)
		{
			var tab = new DockNotebookTab (this, tabStrip);
			if (index == -1) {
				pages.Add (tab);
				tab.Index = pages.Count - 1;
			}
			else {
				pages.Insert (index, tab);
				tab.Index = index;
				UpdateIndexes (index + 1);
			}
			if (pages.Count == 1)
				CurrentTab = tab;

			tabStrip.Update ();
			return tab;
		}

		void UpdateIndexes (int startIndex)
		{
			for (int n=startIndex; n < pages.Count; n++)
				((DockNotebookTab)pages [n]).Index = n;
		}

		public IDockNotebookTab GetTab (int n)
		{
			if (n < 0 || n >= pages.Count)
				return null;
			else
				return pages [n];
		}

		public void RemoveTab (int page)
		{
			if (page < pages.Count - 1)
				CurrentTabIndex = page + 1;
			else if (page > 0)
				CurrentTabIndex = page - 1;
			else
				CurrentTab = null;
			pages.RemoveAt (page);
			UpdateIndexes (page);
			tabStrip.Update ();
		}

		void ReorderChild (Gtk.Widget widget, int npage)
		{
		}

		internal void ShowContent (DockNotebookTab tab)
		{
			if (tab == currentTab)
				contentBox.Child = tab.Content;
		}

		bool DragInProgress;

		public int FindTabAtPosition (double cursorX, double cursorY) {
/*			int    dragNotebookXRoot;
			int    dragNotebookYRoot;
			Widget page;
			int    pageNumber        = CurrentPage;
			Widget tab;
			int    tabMaxX;
			int    tabMaxY;
			int    tabMinX;
			int    tabMinY;
			int? direction = null;

			ParentWindow.GetOrigin (out dragNotebookXRoot, out dragNotebookYRoot);
			
			// We cannot rely on the allocations being zero for tabs which are
			// offscreen. If we write the logic to walk from page 0 til NPages,
			// we can end up choosing the wrong page because pages which are
			// offscreen will match the mouse coordinates. We can work around
			// this by walking either up or down from the active page and choosing
			// the first page which is within the mouse x/y coordinates.
			while ((page = GetTab (pageNumber)) != null && pageNumber >= 0 && pageNumber <= NPages) {

				if ((tab = GetTabLabel (page)) == null)
					return -1;

				tabMinX = dragNotebookXRoot + tab.Allocation.X;
				tabMaxX = tabMinX + tab.Allocation.Width;

				tabMinY = dragNotebookYRoot + tab.Allocation.Y;
				tabMaxY = tabMinY + tab.Allocation.Height;

				if ((tabMinX <= cursorX) && (cursorX <= tabMaxX) &&
					(tabMinY <= cursorY) && (cursorY <= tabMaxY))
					return pageNumber;

				if (!direction.HasValue)
						direction = cursorX > tabMaxX ? 1 : -1;

				pageNumber += direction.Value;
			}
*/
			return -1;
		}

		void MoveTab (int destinationPage)
		{
			if (destinationPage >= 0 && destinationPage != CurrentTabIndex) {
				int oldPage = CurrentTabIndex;
				ReorderChild (CurrentTab.Content, destinationPage);

				if (TabsReordered != null)
					TabsReordered (CurrentTab.Content, oldPage, destinationPage);
			}
		}

		[GLib.ConnectBefore]
		void OnButtonPress (object obj, ButtonPressEventArgs args) {

			if (DragInProgress || args.Event.TriggersContextMenu ())
				return;

			if (args.Event.Button == 1 && args.Event.Type == EventType.ButtonPress && FindTabAtPosition (args.Event.XRoot, args.Event.YRoot) >= 0)
				MotionNotifyEvent += new MotionNotifyEventHandler (OnMotionNotify);
		}
		
		public void LeaveDragMode (uint time)
		{
			if (DragInProgress) {
				Pointer.Ungrab (time);
				Grab.Remove (this);
			}
			MotionNotifyEvent -= new MotionNotifyEventHandler (OnMotionNotify);
			DragInProgress = false;
		}
		
		[GLib.ConnectBefore]
		void OnButtonRelease (object obj, ButtonReleaseEventArgs args) {
			LeaveDragMode (args.Event.Time);
		}


		[GLib.ConnectBefore]
		void OnMotionNotify (object obj, MotionNotifyEventArgs args) {

			if (!DragInProgress) {
				DragInProgress = true;
				Grab.Add (this);

				if (!Pointer.IsGrabbed)
					Pointer.Grab (ParentWindow, false, EventMask.Button1MotionMask | EventMask.ButtonReleaseMask, null, fleurCursor, args.Event.Time);	
			}

			MoveTab (FindTabAtPosition (args.Event.XRoot, args.Event.YRoot));
		}
		
		protected override void OnDestroyed ()
		{
			if (fleurCursor != null) {
				fleurCursor.Dispose ();
				fleurCursor = null;
			}
			base.OnDestroyed ();
		}
	}

	interface IDockNotebookTab
	{
		int Index { get; }
		string Text { get; set; }
		string Markup { get; set; }
		Pixbuf Icon { get; set; }
		Widget Content { get; set; }
		string Tooltip { get; set; }
	}

	internal class DockNotebookTab: IDockNotebookTab
	{
		DockNotebook notebook;
		TabStrip strip;

		string text;
		string markup;
		Gdk.Pixbuf icon;
		Gtk.Widget content;

		internal Gdk.Rectangle Allocation;

		public int Index { get; internal set; }

		public string Text {
			get {
				return this.text;
			}
			set {
				text = value;
				markup = null;
				strip.Update ();
			}
		}

		public string Markup {
			get {
				return this.markup;
			}
			set {
				markup = value;
				text = null;
				strip.Update ();
			}
		}

		public Pixbuf Icon {
			get {
				return this.icon;
			}
			set {
				icon = value;
				strip.Update ();
			}
		}

		public Widget Content {
			get {
				return this.content;
			}
			set {
				content = value;
				notebook.ShowContent (this);
			}
		}

		public string Tooltip { get; set; }

		internal DockNotebookTab (DockNotebook notebook, TabStrip strip)
		{
			this.notebook = notebook;
			this.strip = strip;
		}
	}

	class TabEventArgs: EventArgs
	{
		public DockNotebookTab Tab { get; set; }
	}

	class TabStrip: EventBox
	{
		DockNotebook notebook;
		IDockNotebookTab firstTab;
		int firstTabIndex;
		IDockNotebookTab highlightedTab;
		Gtk.Button previousButton;
		Gtk.Button nextButton;
		Gtk.Button dropDownButton;
		int tabStartX, tabEndX;
		Fixed frame;

		const int TopBarPadding = 3;
		const int BottomBarPadding = 3;
		const int LeftRightPadding = 11;
		const int LeftRightPaddingSel = 7;
		const int TopPadding = 7;
		const int BottomPadding = 7;
		const int SeparatorWidth = 2;
		const int LabelButtonSeparatorWidth = 4;
		const double TabBorderRadius = 3;

		public TabStrip (DockNotebook notebook)
		{
			this.notebook = notebook;
			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			Events |= Gdk.EventMask.PointerMotionMask | Gdk.EventMask.LeaveNotifyMask | Gdk.EventMask.ButtonPressMask;

			previousButton = new Button (new Gtk.Arrow (Gtk.ArrowType.Left, Gtk.ShadowType.None));
			previousButton.Relief = ReliefStyle.None;
			previousButton.ShowAll ();
			nextButton = new Button (new Gtk.Arrow (Gtk.ArrowType.Right, Gtk.ShadowType.None));
			nextButton.Relief = ReliefStyle.None;
			nextButton.ShowAll ();
			dropDownButton = new Button ("");
			dropDownButton.Relief = ReliefStyle.None;
			dropDownButton.ShowAll ();

			int x = 0;

			frame = new Fixed ();

			frame.Put (previousButton, x, TopBarPadding);
			x += previousButton.SizeRequest ().Width;
			frame.Put (nextButton, x, TopBarPadding);
			x += nextButton.SizeRequest ().Width;

			frame.Put (dropDownButton, 0, 0);

			tabStartX = x;

			frame.ShowAll ();
			Add (frame);
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			tabEndX = allocation.Width - dropDownButton.SizeRequest ().Height;
			frame.Put (dropDownButton, tabEndX, TopBarPadding);
			base.OnSizeAllocated (allocation);
		}

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			var t = FindTab ((int)evnt.X, (int)evnt.Y);
			if (t != highlightedTab) {
				highlightedTab = t;
				QueueDraw ();
			}
			return base.OnMotionNotifyEvent (evnt);
		}

		protected override bool OnLeaveNotifyEvent (EventCrossing evnt)
		{
			highlightedTab = null;
			QueueDraw ();
			return base.OnLeaveNotifyEvent (evnt);
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			var t = FindTab ((int)evnt.X, (int)evnt.Y);
			if (t != null) {
				notebook.CurrentTab = t;
				return true;
			}
			return base.OnButtonPressEvent (evnt);
		}

		IDockNotebookTab FindTab (int x, int y)
		{
			for (int n = firstTabIndex; n < notebook.Tabs.Count; n++) {
				DockNotebookTab tab = (DockNotebookTab) notebook.Tabs[n];
				if (tab.Allocation.Contains (x, y))
					return tab;
			}
			return null;
		}

		public void Update ()
		{
			if (firstTab != null && notebook.Tabs.Contains (firstTab))
				firstTabIndex = firstTab.Index;

			int tabsAllocWidth = tabEndX - tabStartX;
			while (firstTabIndex > 0 && CalcTabExtent () < tabsAllocWidth)
				firstTabIndex--;

			firstTab = notebook.GetTab (firstTabIndex);

			QueueDraw ();
		}

		int CalcTabExtent ()
		{
			int x = 0;
			for (int n = firstTabIndex; n < notebook.Tabs.Count; n++) {
				if (n > firstTabIndex && (n - 1 != notebook.CurrentTabIndex))
				    x += SeparatorWidth;
				var tab = notebook.Tabs [n];
				CalcTabSize (tab, ref x, tab == notebook.CurrentTab, tab == highlightedTab);
			}
			return x;
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			requisition.Width = 0;
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			using (var ctx = Gdk.CairoHelper.Create (GdkWindow)) {

				// Draw the background
				ctx.Rectangle (0, 0, Allocation.Width, Allocation.Height - BottomBarPadding);
				Cairo.LinearGradient gr = new LinearGradient (0, 0, 0, Allocation.Height);
				gr.AddColorStop (0, Styles.TabBarGradientStartColor);
				gr.AddColorStop (1, Styles.TabBarGradientEndColor);
				ctx.Pattern = gr;
				ctx.Fill ();

				ctx.Rectangle (0, Allocation.Height - BottomBarPadding - 1, Allocation.Width, BottomBarPadding);
				ctx.Color = Styles.BreadcrumbBackgroundColor;
				ctx.Fill ();

				int x = tabStartX;
				int y = 0;
				for (int n = firstTabIndex; n < notebook.Tabs.Count; n++) {
					int sx = x;
					var tab = (DockNotebookTab) notebook.Tabs [n];
					if (tab == notebook.CurrentTab)
						DrawActiveTab (ctx, tab, ref x, y, tab == highlightedTab);
					else
						DrawTab (ctx, tab, ref x, y, tab == highlightedTab);
					tab.Allocation = new Gdk.Rectangle (sx, Allocation.Y, x - sx, Allocation.Height);
				}
			}
			return base.OnExposeEvent (evnt);
		}

		void CalcTabSize (IDockNotebookTab tab, ref int x, bool active, bool highlight)
		{
			var la = CreateTabLayout (tab);

			int w, h;
			la.GetPixelSize (out w, out h);

			if (active)
				x += LeftRightPaddingSel * 2 + w;
			else
				x += LeftRightPadding * 2 + w;

			la.Dispose ();
		}

		void DrawTab (Cairo.Context ctx, DockNotebookTab tab, ref int x, int y, bool highlight)
		{
			var la = CreateTabLayout (tab);
			int w, h;
			la.GetPixelSize (out w, out h);

			var gc = highlight ? Style.TextGC (Gtk.StateType.Normal) : Style.TextGC (Gtk.StateType.Insensitive);
			x += LeftRightPadding;

			GdkWindow.DrawLayout (gc, x, y + TopPadding, la);

			x += w + LeftRightPadding;

			la.Dispose ();
		}

		void DrawActiveTab (Cairo.Context ctx, DockNotebookTab tab, ref int x, int y, bool highlight)
		{
			var la = CreateTabLayout (tab);
			int w, h;
			la.GetPixelSize (out w, out h);

			ctx.LineWidth = 1;
			DrawTabBorder (ctx, w + LeftRightPaddingSel * 2 - 2, x, 0);
			ctx.ClosePath ();
			Cairo.LinearGradient gr = new LinearGradient (x, Allocation.Y, x, Allocation.Bottom);
			gr.AddColorStop (0, Styles.BreadcrumbGradientStartColor);
			gr.AddColorStop (0.3, Styles.BreadcrumbBackgroundColor);
			gr.AddColorStop (1, Styles.BreadcrumbBackgroundColor);
			ctx.Pattern = gr;
			ctx.Fill ();

			ctx.Color = Styles.BreadcrumbBorderColor;
			DrawTabBorder (ctx, w + LeftRightPaddingSel * 2 - 2, x, 0);
			ctx.Stroke ();

			ctx.Color = Styles.BreadcrumbInnerBorderColor;
			DrawTabBorder (ctx, w + LeftRightPaddingSel * 2 - 4, x + 1, 1);
			ctx.Stroke ();

			var gc = Style.WhiteGC;

			x += LeftRightPaddingSel;

			GdkWindow.DrawLayout (gc, x, y + TopPadding, la);

			x += w + LeftRightPaddingSel;

			la.Dispose ();
		}

		void DrawTabBorder (Cairo.Context ctx, int contentWidth, int px, int margin)
		{
			double x = 0.5 + (double)px;
			double y = (double) Allocation.Height - 0.5 - BottomBarPadding + margin;
			double height = Allocation.Height - TopBarPadding - BottomBarPadding;

			ctx.MoveTo (0.5, y);
			ctx.LineTo (x, y);
			ctx.LineTo (x, y - height + TabBorderRadius);
			ctx.Arc (x + TabBorderRadius, y - height + TabBorderRadius, TabBorderRadius, System.Math.PI, System.Math.PI * 1.5d);
			double rightx = x + contentWidth + 1;
			ctx.LineTo (rightx - TabBorderRadius, y - height);
			ctx.Arc (rightx - TabBorderRadius, y - height + TabBorderRadius, TabBorderRadius, System.Math.PI * 1.5d, System.Math.PI * 2d);
			ctx.LineTo (rightx, y);
			ctx.LineTo (Allocation.Width - 0.5, y);
		}

		Pango.Layout CreateTabLayout (IDockNotebookTab tab)
		{
			Pango.Layout la = new Pango.Layout (PangoContext);
			if (tab.Markup != null)
				la.SetMarkup (tab.Markup);
			else
				la.SetText (tab.Text);
			return la;
		}
	}
}