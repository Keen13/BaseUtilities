﻿/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLMessageBox
    {
        public enum MessageBoxButtons
        {
            OK = 0,
            OKCancel = 1,
            AbortRetryIgnore = 2,
            YesNoCancel = 3,
            YesNo = 4,
            RetryCancel = 5
        }

        public GLMessageBox( GLBaseControl c, Action<GLMessageBox, DialogResult> callback, string text, string caption, MessageBoxButtons but = MessageBoxButtons.OK )
        {
            callbackfunc = callback;

            GLFormConfigurable c1 = new GLFormConfigurable();
            c1.TopMost = true;
            int butpos1 = 300, butpos2= 400, butpos3 = 500;
            int butwidth = 80;
            int butheight = 20;
            int butline = 300;

            if (but == MessageBoxButtons.AbortRetryIgnore)
            {
                c1.Add(new GLFormConfigurable.Entry("Ignore", typeof(GLButton), "Ignore", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.Ignore));
                c1.Add(new GLFormConfigurable.Entry("Retry", typeof(GLButton), "Retry", new Point(butpos2, butline), new Size(butwidth, butheight), null, DialogResult.Retry));
                c1.Add(new GLFormConfigurable.Entry("OK", typeof(GLButton), "OK", new Point(butpos1, butline), new Size(butwidth, butheight), null, DialogResult.OK) { taborder = 0 });
            }
            else if (but == MessageBoxButtons.OKCancel)
            {
                c1.Add(new GLFormConfigurable.Entry("Cancel", typeof(GLButton), "Cancel", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.Cancel) { taborder = 1 });
                c1.Add(new GLFormConfigurable.Entry("OK", typeof(GLButton), "OK", new Point(butpos2, butline), new Size(butwidth, butheight), null, DialogResult.OK) { taborder = 0 });
            }
            else if (but == MessageBoxButtons.RetryCancel)
            {
                c1.Add(new GLFormConfigurable.Entry("Retry", typeof(GLButton), "Retry", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.Retry));
                c1.Add(new GLFormConfigurable.Entry("OK", typeof(GLButton), "OK", new Point(butpos2, butline), new Size(butwidth, butheight), null, DialogResult.OK) { taborder = 0 });
            }
            else if (but == MessageBoxButtons.YesNo)
            {
                c1.Add(new GLFormConfigurable.Entry("No", typeof(GLButton), "No", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.No));
                c1.Add(new GLFormConfigurable.Entry("Yes", typeof(GLButton), "Yes", new Point(butpos2, butline), new Size(butwidth, butheight), null, DialogResult.Yes) { taborder = 0 });
            }
            else if (but == MessageBoxButtons.YesNo)
            {
                c1.Add(new GLFormConfigurable.Entry("Cancel", typeof(GLButton), "Cancel", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.Cancel));
                c1.Add(new GLFormConfigurable.Entry("No", typeof(GLButton), "No", new Point(butpos2, butline), new Size(butwidth, butheight), null, DialogResult.No));
                c1.Add(new GLFormConfigurable.Entry("Yes", typeof(GLButton), "Yes", new Point(butpos1, butline), new Size(butwidth, butheight), null, DialogResult.Yes) { taborder = 0 });
            }
            else 
            {
                c1.Add(new GLFormConfigurable.Entry("OK", typeof(GLButton), "OK", new Point(butpos3, butline), new Size(butwidth, butheight), null, DialogResult.OK));
            }

            GLMultiLineTextBox tb = new GLMultiLineTextBox("MLT", new Rectangle(10, 10, butpos3 + butwidth - 10, butline - 20),text);
            tb.BackColor = Color.Transparent;
            tb.ForeColor = GLBaseControl.DefaultFormTextColor;
            tb.ReadOnly = true;
            c1.Add(new GLFormConfigurable.Entry(tb));

            c1.Init(new Point(200, 200), caption);
            c1.DialogCallback = FormCallBack;
            c1.Tag = this;
            c1.Trigger += (cfg, en, ctrlname, args) =>
            {
                if (ctrlname == "Escape")
                {
                    c1.DialogResult = DialogResult.Abort;
                    c1.Close();
                }
                else
                {
                    c1.DialogResult = (DialogResult)en.tag;
                    c1.Close();
                }
            };

            c.FindDisplay().Add(c1);
        }

        private void FormCallBack(GLForm p, DialogResult r)
        {
            GLMessageBox m = p.Tag as GLMessageBox;
            callbackfunc?.Invoke(m, r);
        }

        private Action<GLMessageBox, DialogResult> callbackfunc;
    }
}

