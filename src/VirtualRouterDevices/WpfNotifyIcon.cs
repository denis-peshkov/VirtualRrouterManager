/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VirtualRouterManager
{
    public partial class WpfNotifyIcon : Component
    {
        public WpfNotifyIcon()
        {
            InitializeComponent();
        }

        public WpfNotifyIcon(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public EventHandler DoubleClick;

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.DoubleClick != null)
            {
                this.DoubleClick(sender, e);
            }
        }

        public void Show()
        {
            this.notifyIcon1.Visible = true;
        }

        public void Hide()
        {
            this.notifyIcon1.Visible = false;
        }

        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.notifyIcon1.ContextMenuStrip;
            }
            set
            {
                this.notifyIcon1.ContextMenuStrip = value;
            }
        }

        public Icon Icon
        {
            get
            {
                return this.notifyIcon1.Icon;
            }
            set
            {
                this.notifyIcon1.Icon = value;
            }
        }

        public string Text
        {
            get
            {
                return this.notifyIcon1.Text;
            }
            set
            {
                this.notifyIcon1.Text = value;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
