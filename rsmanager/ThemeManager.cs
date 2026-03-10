using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RSBotManager
{
    public class ThemeColors
    {
        public Color FormBackground { get; set; }
        public Color PanelBackground { get; set; }
        public Color ControlBackground { get; set; }
        public Color TextColor { get; set; }
        public Color ButtonBackground { get; set; }
        public Color ButtonText { get; set; }
        public Color AccentColor { get; set; }
        public Color StatusBarBackground { get; set; }
        public Color StatusBarText { get; set; }
        public Color ListBackgroundColor { get; set; }
        public Color ListTextColor { get; set; }
        public Color ListAlternateBackground { get; set; }
        public Color DisabledButtonColor { get; set; }
        public Color BorderColor { get; set; }
        public Color RunningBotForeColor { get; set; }
        public Color RunningBotBackColor { get; set; }
        public Color HiddenBotForeColor { get; set; }
        public Color HiddenBotBackColor { get; set; }
        public Color ClosedBotForeColor { get; set; }
        public Color ClosedBotBackColor { get; set; }
        public Color SplitterColor { get; set; }
        public Color GridLineColor { get; set; }
        public Color ColumnHeaderBackground { get; set; }
        public Color ColumnHeaderText { get; set; }
    }

    public static class ThemeManager
    {
        private static readonly Color[] SemanticButtonColors = new[]
        {
            Color.FromArgb(40, 167, 69),    // Green — Start/Add/Hide
            Color.FromArgb(220, 53, 69),    // Red — Stop/Remove
            Color.FromArgb(13, 110, 253),   // Blue — Edit/HideShow
            Color.FromArgb(0, 123, 255),    // Blue variant — Show
            Color.FromArgb(108, 117, 125),  // Gray — MoveUp/MoveDown/inactive
            Color.FromArgb(23, 162, 184),   // Teal — Add Group
            Color.FromArgb(255, 193, 7),    // Amber — Remove Group
        };

        public static readonly ThemeColors LightColors = new ThemeColors
        {
            FormBackground        = Color.FromArgb(245, 245, 250),
            PanelBackground       = Color.FromArgb(250, 250, 252),
            ControlBackground     = Color.White,
            TextColor             = Color.Black,
            ButtonBackground      = Color.FromArgb(229, 229, 229),
            ButtonText            = Color.Black,
            AccentColor           = Color.FromArgb(13, 110, 253),
            StatusBarBackground   = Color.FromArgb(240, 240, 240),
            StatusBarText         = Color.Black,
            ListBackgroundColor   = Color.White,
            ListTextColor         = Color.Black,
            ListAlternateBackground = Color.FromArgb(245, 245, 245),
            DisabledButtonColor   = Color.FromArgb(150, 150, 150),
            BorderColor           = Color.FromArgb(200, 200, 200),
            RunningBotForeColor   = Color.DarkGreen,
            RunningBotBackColor   = Color.FromArgb(240, 255, 240),
            HiddenBotForeColor    = Color.DarkBlue,
            HiddenBotBackColor    = Color.FromArgb(240, 240, 255),
            ClosedBotForeColor    = Color.Red,
            ClosedBotBackColor    = Color.FromArgb(255, 240, 240),
            SplitterColor         = Color.FromArgb(200, 200, 200),
            GridLineColor         = Color.FromArgb(230, 230, 230),
            ColumnHeaderBackground = SystemColors.Control,
            ColumnHeaderText       = Color.Black
        };

        public static readonly ThemeColors DarkColors = new ThemeColors
        {
            FormBackground        = Color.FromArgb(30, 30, 30),
            PanelBackground       = Color.FromArgb(37, 37, 38),
            ControlBackground     = Color.FromArgb(51, 51, 51),
            TextColor             = Color.FromArgb(204, 204, 204),
            ButtonBackground      = Color.FromArgb(63, 63, 70),
            ButtonText            = Color.FromArgb(204, 204, 204),
            AccentColor           = Color.FromArgb(0, 120, 212),
            StatusBarBackground   = Color.FromArgb(0, 122, 204),
            StatusBarText         = Color.White,
            ListBackgroundColor   = Color.FromArgb(37, 37, 38),
            ListTextColor         = Color.FromArgb(204, 204, 204),
            ListAlternateBackground = Color.FromArgb(45, 45, 48),
            DisabledButtonColor   = Color.FromArgb(63, 63, 70),
            BorderColor           = Color.FromArgb(63, 63, 70),
            RunningBotForeColor   = Color.FromArgb(106, 215, 106),
            RunningBotBackColor   = Color.FromArgb(30, 50, 30),
            HiddenBotForeColor    = Color.FromArgb(100, 149, 237),
            HiddenBotBackColor    = Color.FromArgb(30, 30, 55),
            ClosedBotForeColor    = Color.FromArgb(255, 100, 100),
            ClosedBotBackColor    = Color.FromArgb(55, 30, 30),
            SplitterColor         = Color.FromArgb(63, 63, 70),
            GridLineColor         = Color.FromArgb(50, 50, 52),
            ColumnHeaderBackground = Color.FromArgb(45, 45, 48),
            ColumnHeaderText       = Color.FromArgb(204, 204, 204)
        };

        public static string GetSystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    var value = key.GetValue("AppsUseLightTheme");
                    if (value is int intValue && intValue == 0)
                        return "Dark";
                }
            }
            catch
            {
                // Fall through to default (Light)
            }
            return "Light";
        }

        public static ThemeColors GetCurrentColors(string themeSetting)
        {
            if (themeSetting == "Dark")
                return DarkColors;
            if (themeSetting == "Light")
                return LightColors;
            return GetSystemTheme() == "Dark" ? DarkColors : LightColors;
        }

        private const int LVM_GETHEADER = 0x101F;
        private static readonly System.Collections.Generic.Dictionary<IntPtr, HeaderPainter> _headerPainters = new();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        private static void SetHeaderBackColor(ListView lv, Color color)
        {
            if (!lv.IsHandleCreated)
            {
                void handler(object s, EventArgs e)
                {
                    lv.HandleCreated -= handler;
                    SetHeaderBackColor(lv, color);
                };
                lv.HandleCreated += handler;
                return;
            }

            IntPtr headerHandle = SendMessage(lv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (headerHandle != IntPtr.Zero)
            {
                SetWindowTheme(headerHandle, "", "");

                if (_headerPainters.TryGetValue(headerHandle, out var existing))
                {
                    existing.UpdateColor(color);
                }
                else
                {
                    var painter = new HeaderPainter(headerHandle, color, lv);
                    _headerPainters[headerHandle] = painter;
                }
                InvalidateRect(headerHandle, IntPtr.Zero, true);
            }
        }

        public static void ApplyTheme(Control control, ThemeColors colors)
        {
            ApplyToControl(control, colors);
            foreach (Control child in control.Controls)
            {
                ApplyTheme(child, colors);
            }
        }

        private static bool IsSemanticButtonColor(Color color)
        {
            foreach (var semantic in SemanticButtonColors)
            {
                if (color.ToArgb() == semantic.ToArgb())
                    return true;
            }
            return false;
        }

        private static void ApplyToControl(Control control, ThemeColors colors)
        {
            switch (control)
            {
                case Form form:
                    form.BackColor = colors.FormBackground;
                    form.ForeColor = colors.TextColor;
                    break;

                case SplitContainer sc:
                    sc.BackColor = colors.SplitterColor;
                    sc.Panel1.BackColor = colors.PanelBackground;
                    sc.Panel2.BackColor = colors.PanelBackground;
                    break;

                case TableLayoutPanel tlp:
                    tlp.BackColor = colors.PanelBackground;
                    break;

                case Panel panel:
                    panel.BackColor = colors.PanelBackground;
                    break;

                case Button btn:
                    if (!IsSemanticButtonColor(btn.BackColor))
                    {
                        btn.BackColor = colors.ButtonBackground;
                        btn.ForeColor = colors.ButtonText;
                    }
                    break;

                case TextBox tb:
                    tb.BackColor = colors.ControlBackground;
                    tb.ForeColor = colors.TextColor;
                    break;

                case ListView lv:
                    lv.BackColor = colors.ListBackgroundColor;
                    lv.ForeColor = colors.ListTextColor;
                    lv.OwnerDraw = true;
                    lv.DrawColumnHeader -= Lv_DrawColumnHeader;
                    lv.DrawItem -= Lv_DrawItem;
                    lv.DrawSubItem -= Lv_DrawSubItem;
                    lv.DrawColumnHeader += Lv_DrawColumnHeader;
                    lv.DrawItem += Lv_DrawItem;
                    lv.DrawSubItem += Lv_DrawSubItem;
                    lv.Tag = colors;
                    // Set native header background color (covers empty area after last column)
                    SetHeaderBackColor(lv, colors.ColumnHeaderBackground);
                    break;

                case TreeView tv:
                    tv.BackColor = colors.ListBackgroundColor;
                    tv.ForeColor = colors.ListTextColor;
                    break;

                case ListBox lb:
                    lb.BackColor = colors.ListBackgroundColor;
                    lb.ForeColor = colors.ListTextColor;
                    break;

                case StatusStrip ss:
                    ss.BackColor = colors.StatusBarBackground;
                    foreach (ToolStripItem item in ss.Items)
                    {
                        item.ForeColor = colors.StatusBarText;
                        if (item is ToolStripComboBox cb)
                        {
                            cb.BackColor = colors.ControlBackground;
                            cb.ForeColor = colors.TextColor;
                        }
                    }
                    break;

                case Label lbl:
                    lbl.ForeColor = colors.TextColor;
                    break;

                case ComboBox combo:
                    combo.BackColor = colors.ControlBackground;
                    combo.ForeColor = colors.TextColor;
                    break;
            }
        }

        private static void Lv_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var lv = (ListView)sender;
            var colors = lv.Tag as ThemeColors ?? LightColors;

            // Draw this column header cell
            using (var brush = new SolidBrush(colors.ColumnHeaderBackground))
                e.Graphics.FillRectangle(brush, e.Bounds);

            var textBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, lv.Font, textBounds, colors.ColumnHeaderText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            using (var pen = new Pen(colors.GridLineColor))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private static void Lv_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // Let DrawSubItem handle per-cell drawing
        }

        private static void Lv_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var lv = (ListView)sender;
            var colors = lv.Tag as ThemeColors ?? LightColors;

            // Fill background
            using (var brush = new SolidBrush(e.SubItem.BackColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            // Draw text using TextRenderer (GDI) — handles emoji/unicode correctly
            var textBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font ?? lv.Font, textBounds, e.SubItem.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

            // Draw grid lines
            using (var pen = new Pen(colors.GridLineColor))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
            }

            // Draw selection highlight
            if (e.Item.Selected)
            {
                using var highlightBrush = new SolidBrush(Color.FromArgb(60, colors.AccentColor));
                e.Graphics.FillRectangle(highlightBrush, e.Bounds);
            }
        }
    }

    internal class HeaderPainter : NativeWindow
    {
        private const int WM_ERASEBKGND = 0x0014;
        private const int WM_PAINT = 0x000F;
        private Color _bgColor;
        private ListView _listView;

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        public HeaderPainter(IntPtr headerHandle, Color bgColor, ListView listView)
        {
            _bgColor = bgColor;
            _listView = listView;
            AssignHandle(headerHandle);
        }

        public void UpdateColor(Color bgColor)
        {
            _bgColor = bgColor;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                // Fill entire header background with our color
                using var g = Graphics.FromHdc(m.WParam);
                using var brush = new SolidBrush(_bgColor);
                g.FillRectangle(brush, 0, 0, 10000, 200);
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT)
            {
                // After system paints (OwnerDraw columns + classic 3D effects),
                // paint flat color over the empty area to cover 3D artifacts
                int totalColWidth = 0;
                for (int i = 0; i < _listView.Columns.Count; i++)
                    totalColWidth += _listView.Columns[i].Width;

                GetClientRect(Handle, out var clientRect);
                int remaining = clientRect.Right - totalColWidth;

                if (remaining > 0)
                {
                    using var g = Graphics.FromHwnd(Handle);
                    using var brush = new SolidBrush(_bgColor);
                    g.FillRectangle(brush, totalColWidth, 0, remaining, clientRect.Bottom);
                }
            }
        }
    }
}
