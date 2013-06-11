/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System.ComponentModel;

namespace VirtualRouterDevices.DL
{
    public enum DeviceEnum : int
    {
        [Description("Default"), DeviceResourceName("IconDefault")]
        Default = 0,
        [Description("Computer"), DeviceResourceName("IconComputer")]
        Computer = 1,
        [Description("Device"), DeviceResourceName("IconDevice")]
        Device = 2,
        [Description("iPhone"), DeviceResourceName("IconIPhone")]
        IPhone = 3,
        [Description("Mac Book Pro"), DeviceResourceName("IconMacBookPro")]
        MacBookPro = 4,
        [Description("Printer"), DeviceResourceName("IconPrinter")]
        Printer = 5,
        [Description("Mobile Phone"), DeviceResourceName("IconMobilePhone")]
        MobilePhone = 6,
        [Description("Zune"), DeviceResourceName("IconZune")]
        Zune = 7,
        [Description("Camera"), DeviceResourceName("IconCamera")]
        Camera = 8
    }
}
