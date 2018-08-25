using IvorySharp.Aspects.Pipeline.Async;
using IvorySharp.Caching;
using IvorySharp.Core;

namespace IvorySharp.Aspects.Pipeline
{  
    /// <summary>
    /// Реализация фабрики компонентов пайплайна. Выполняет определение компонентов
    /// на основе того, является ли метод асинхронным.
    /// </summary>
    internal sealed class AsyncDeterminingPipelineFactory : IInvocationPipelineFactory
    {
        private readonly IMethodInfoCache _cache;
        
        /// <summary>
        /// Инициализирует экземпляр <see cref="AsyncDeterminingPipelineFactory"/>.
        /// </summary>
        /// <param name="methodInfoCache">Кеш методов.</param>
        internal AsyncDeterminingPipelineFactory(IMethodInfoCache methodInfoCache)
        {
            _cache = methodInfoCache;
        }
        
        /// <inheritdoc />
        public IInvocationPipeline CreatePipeline(
            IInvocationSignature signature,
            MethodBoundaryAspect[] boundaryAspects, 
            MethodInterceptionAspect interceptionAspect)
        {
            if (_cache.IsAsync(signature.TargetMethod))
                return new AsyncInvocationPipeline(boundaryAspects, interceptionAspect);
            
            return new InvocationPipeline(boundaryAspects, interceptionAspect);
        }

        /// <inheritdoc />
        public IInvocationPipelineExecutor CreateExecutor(IInvocationSignature signature)
        {
            if (_cache.IsAsync(signature.TargetMethod))
                return AsyncInvocationPipelineExecutor.Instance;
            
            return InvocationPipelineExecutor.Instance;
        }
    }
}