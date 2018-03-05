﻿using System;
using IvorySharp.Aspects.Configuration;
using IvorySharp.Tests.Helpers;
using IServiceProvider = IvorySharp.Aspects.Dependency.IServiceProvider;

namespace IvorySharp.Tests.WeavingSettings
{
    public class ExplicitAspectWeavingSettings : IAspectsWeavingSettings
    {
        public Type ExplicitWeavingAttributeType { get; } = typeof(EnableWeavingAttribute);
        public IServiceProvider ServiceProvider { get; }
    }
}