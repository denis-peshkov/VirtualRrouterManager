/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VirtualRouterManager.Themes
{
    public class TileView : ViewBase
    {
        private DataTemplate itemTemplate;
        public DataTemplate ItemTemplate
        {
            get { return itemTemplate; }
            set { itemTemplate = value; }
        }

        private Brush selectedBackground = Brushes.Transparent;
        public Brush SelectedBackground
        {
            get { return selectedBackground; }
            set { selectedBackground = value; }
        }

        private Brush selectedBorderBrush = Brushes.Black;
        public Brush SelectedBorderBrush
        {
            get { return selectedBorderBrush; }
            set { selectedBorderBrush = value; }
        }

        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "TileView"); }
        }

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "TileViewItem"); }
        }
    }
}
