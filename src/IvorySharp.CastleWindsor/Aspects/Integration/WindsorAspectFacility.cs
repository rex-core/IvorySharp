﻿using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using IvorySharp.Aspects.Components.Weaving;
using IvorySharp.Aspects.Configuration;
using IvorySharp.CastleWindsor.Aspects.Weaving;

namespace IvorySharp.CastleWindsor.Aspects.Integration
{
    internal class WindsorAspectFacility : AbstractFacility
    {
        private readonly IComponentsStore _settings;

        /// <summary>
        /// Инициализирует экземпляр <see cref="WindsorAspectFacility"/>
        /// </summary>
        /// <param name="settings">Настройки аспектов.</param>
        public WindsorAspectFacility(IComponentsStore settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        protected override void Init()
        {
            Kernel.ComponentRegistered += OnComponentRegistered;
        }

        private void OnComponentRegistered(string key, IHandler handler)
        {
            var componentInterfaces = handler.ComponentModel.Implementation.GetInterfaces();
            if (componentInterfaces.Any(i => AspectWeaver.NotWeavableTypes.Contains(i)))
                return;

            foreach (var serviceType in handler.ComponentModel.Services)
            {
                if (!AspectWeaver.IsWeavable(serviceType, _settings))
                    continue;
                
                handler.ComponentModel.Interceptors.AddIfNotInCollection(
                    new InterceptorReference(typeof(AspectWeaverInterceptorAdapter)));
            }
        }
    }
}