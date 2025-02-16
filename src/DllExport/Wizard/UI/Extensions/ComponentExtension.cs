﻿/*!
 * Copyright (c) Denis Kuzmin <x-3F@outlook.com> github/3F
 * Copyright (c) DllExport contributors https://github.com/3F/DllExport/graphs/contributors
 * Licensed under the MIT License (MIT).
 * See accompanying LICENSE.txt file or visit https://github.com/3F/DllExport
*/

// Based on version from https://github.com/3F/vsSolutionBuildEvent
// 3b82a028fe955947ab05b1396375ca84696afff5

using System;
using System.Windows.Forms;
using net.r_eg.DllExport.Wizard.UI.Components;

#if NET40
using System.Drawing;
#endif

namespace net.r_eg.DllExport.Wizard.UI.Extensions
{
    internal static class ComponentExtension
    {
        public static float GetSysDpi(this Control src)
        {
#if !NET40
            return src.DeviceDpi;
#else
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            return g.DpiX;
#endif
        }

        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <param name="size">The default, 96 dots per inch.</param>
        /// <returns>Final value according to current DPI via <see cref="GetSysDpi(Control)"/></returns>
        public static int GetValueUsingDpi(this Control src, int value, float size = 96)
        {
            return (int)Math.Ceiling(value / size * src.GetSysDpi());
        }

        internal static void RemoveRow(this DataGridViewExt dgv, DataGridViewCellEventArgs e, DataGridViewButtonColumn btn)
        {
            if(e.RowIndex == -1 || e.ColumnIndex == -1) return;

            if(e.ColumnIndex == dgv.Columns.IndexOf(btn) && e.RowIndex < dgv.Rows.Count - 1)
            {
                dgv.Rows.Remove(dgv.Rows[e.RowIndex]);
            }
        }
    }
}
