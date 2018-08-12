using IvorySharp.Core;
using IvorySharp.Extensions;

namespace IvorySharp.Aspects.Pipeline.Synchronous
{
    /// <summary>
    /// Базовая модель синхронного пайплайна вызова метода.
    /// </summary>
    internal abstract class SyncInvocationPipeline : InvocationPipelineBase
    {
        /// <inheritdoc />
        public override object CurrentReturnValue
        {
            get => Invocation.ReturnValue;
            set => Invocation.ReturnValue = value;
        }

        /// <summary>
        /// Инициализирует экземпляр <see cref="SyncInvocationPipeline"/>.
        /// </summary>
        /// <param name="invocation">Модель вызова метода.</param>
        protected SyncInvocationPipeline(IInvocation invocation) : base(invocation)
        {
        }
        
        /// <inheritdoc />
        public override void Return()
        {
            CurrentReturnValue = Context.Method.ReturnType.GetDefaultValue();
            FlowBehavior = FlowBehavior.Return;
        }

        /// <inheritdoc />
        public override void ReturnValue(object returnValue)
        {
            CurrentException = null;
            FlowBehavior = FlowBehavior.Return;        
            CurrentReturnValue = returnValue;
        }
    }
}