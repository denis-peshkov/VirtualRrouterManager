/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;

namespace VirtualRouterDevices.DL
{
    public class DeviceResourceNameAttribute : Attribute
    {
        public DeviceResourceNameAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        public string ResourceName { get; private set; }
    }
}