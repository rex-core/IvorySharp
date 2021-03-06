﻿using IvorySharp.Aspects.Integration;
using JetBrains.Annotations;

namespace IvorySharp.Aspects.Configuration
{
    /// <summary>
    /// Стартовая точка конфигурации аспектов.
    /// </summary>
    [PublicAPI]
    public static class AspectsConfigurator
    {
        /// <summary>
        /// Устанавливает контейнер для настройки аспектов.
        /// </summary>
        /// <param name="container">Контейнер аспектов.</param>
        /// <typeparam name="T">Тип контейнера.</typeparam>
        /// <returns>Синтаксис конфигурации аспектов.</returns>
        public static AspectsFluentConfigurationSyntax UseContainer<T>(T container)
            where T : AspectsContainer
        {
            return new AspectsFluentConfigurationSyntax(container);
        }
    }
}