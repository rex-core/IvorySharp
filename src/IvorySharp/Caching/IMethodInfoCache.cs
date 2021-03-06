using System;
using System.Reflection;
using JetBrains.Annotations;

namespace IvorySharp.Caching
{
    /// <summary>
    /// Кеш информации о методе.
    /// </summary>
    internal interface IMethodInfoCache
    {
        /// <summary>
        /// Возвращает метод в типе <paramref name="targetType"/>, соответствующий методу <paramref name="interfaceMethod"/>.
        /// </summary>
        /// <param name="targetType">Тип класса.</param>
        /// <param name="interfaceMethod">Метод в интерфейсе.</param>
        /// <returns>Метод, соответствующий <paramref name="interfaceMethod"/>.</returns>
        [CanBeNull] MethodInfo GetMethodMap([NotNull] Type targetType, [NotNull] MethodInfo interfaceMethod);

        /// <summary>
        /// Возвращает признак того, что метод является асинхронным.
        /// </summary>
        /// <param name="method">Метод.</param>
        /// <returns>Признак того, что метод является асинхронным.</returns>
        bool IsAsync([NotNull] MethodInfo method);
    }
}