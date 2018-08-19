﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using IvorySharp.Aspects.Components;
using IvorySharp.Aspects.Selection;
using IvorySharp.Core;
using IvorySharp.Extensions;
using JetBrains.Annotations;

namespace IvorySharp.Aspects.Weaving
{
    /// <summary>
    /// Базовый класс предиката возможности применения аспектов.
    /// </summary>
    [PublicAPI, EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class BaseWeavePredicate : IAspectWeavePredicate
    {
        /// <summary>
        /// Провайдер стратегии выбора аспектов.
        /// </summary>
        protected IComponentProvider<IAspectSelector> AspectSelectorProvider { get; }

        /// <summary>
        /// Инициализирует экземпляр <see cref="BaseWeavePredicate"/>.
        /// </summary>
        /// <param name="selectorProvider">Провайдера стратегии выбора аспектов.</param>
        protected BaseWeavePredicate(IComponentProvider<IAspectSelector> selectorProvider)
        {
            AspectSelectorProvider = selectorProvider;
        }

        /// <inheritdoc />
        public abstract bool IsWeaveable(Type declaringType, Type targetType);

        /// <inheritdoc />
        public abstract bool IsWeaveable(IInvocation invocation);

        /// <summary>
        /// Возвращает признак того, что применение аспектов запрещено.
        /// </summary>
        protected static bool IsWeavingSuppressed(Type type)
        {
            return !type.IsInterceptable() || type.GetCustomAttributes<SuppressAspectsWeavingAttribute>(inherit: false).Any();
        }

        /// <summary>
        /// Возвращает признак того, что применение аспектов запрещено.
        /// </summary>
        protected static bool IsWeavingSuppressed(MethodInfo method)
        {
            return !method.IsInterceptable() || method.GetCustomAttributes<SuppressAspectsWeavingAttribute>(inherit: false).Any();
        }
    }
}