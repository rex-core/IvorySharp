using IvorySharp.Aspects.Components;
using IvorySharp.Core;

namespace IvorySharp.Aspects.Pipeline
{
    /// <summary>
    /// Фабрика компонентов пайплайна.
    /// </summary>
    public interface IInvocationPipelineFactory : IComponent
    {
        /// <summary>
        /// Создает модель пайплайна вызова метода.
        /// </summary>
        /// <param name="invocation">Модель вызова метода.</param>
        /// <param name="boundaryAspects">Аспекты типа <see cref="MethodBoundaryAspect"/>.</param>
        /// <param name="interceptionAspect">Аспект типа <see cref="MethodInterceptionAspect"/>.</param>
        /// <returns>Модель пайплайна.</returns>
        IInvocationPipeline CreatePipeline(IInvocation invocation,
            MethodBoundaryAspect[] boundaryAspects, 
            MethodInterceptionAspect interceptionAspect);

        /// <summary>
        /// Создает компонент выполнения пайплайна вызова метода.
        /// </summary>
        /// <param name="context">Контекст вызова.</param>
        /// <returns>Компонент выполнения пайплайна.</returns>
        IInvocationPipelineExecutor CreateExecutor(IInvocationContext context);
    }
}