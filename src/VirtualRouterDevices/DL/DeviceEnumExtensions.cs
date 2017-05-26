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
    public static class DeviceEnumExtensions
    {
        public static string ToResourceName(this DeviceEnum val)
        {
            var attributes = (DeviceResourceNameAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DeviceResourceNameAttribute), false);
            return attributes.Length > 0 ? attributes[0].ResourceName : string.Empty;
        }

        public static string ToDescriptionString(this DeviceEnum val)
        {
            var attributes = (DescriptionAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}