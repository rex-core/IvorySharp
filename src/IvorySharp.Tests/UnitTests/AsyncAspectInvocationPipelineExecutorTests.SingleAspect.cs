using System;
using System.Threading.Tasks;
using IvorySharp.Aspects.Pipeline.Async;
using IvorySharp.Tests.Asserts;
using IvorySharp.Tests.Assets;
using IvorySharp.Tests.Assets.Aspects;
using IvorySharp.Tests.Utility;
using Xunit;

namespace IvorySharp.Tests.UnitTests
{
    /// <summary>
    /// Набор тестов для компонента <see cref="AsyncAspectInvocationPipelineExecutor"/>
    /// </summary>
    public partial class AsyncAspectInvocationPipelineExecutorTests
    {
        private readonly AsyncAspectInvocationPipelineExecutor _executor;

        public AsyncAspectInvocationPipelineExecutorTests()
        {
            _executor = AsyncAspectInvocationPipelineExecutor.Instance;
        }

        [Fact]
        public async Task SingleAspect_NormalFlow_AspectBoundariesCalled()
        {
            // Arrange            
            var aspect = new ObservableAspect();
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);
            
            // Assert
            Assert.Equal(new [] { new BoundaryState(BoundaryType.Entry)}, aspect.ExecutionStack);

            var result = await Unwrap<int>(pipeline.Invocation);
            
            Assert.Equal(10, result);
            Assert.Equal(_normalExecutionStack, aspect.ExecutionStack);
            Assert.Equal(10, pipeline.CurrentReturnValue);
            
            InvocationAssert.ProceedCalled(pipeline.Invocation);
        }

        [Fact]
        public async Task SingleAspect_ExceptionFlow_AspectBoundariesCalled()
        {
            // Arrange            
            var aspect = new ObservableAspect();
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.ThrowArgumentExceptionAsync), Args.Pack(aspect));

            // Act
            _executor.ExecutePipeline(pipeline);
            
            // Assert
            Assert.Equal(new [] { new BoundaryState(BoundaryType.Entry)}, aspect.ExecutionStack);

            await Assert.ThrowsAsync<ArgumentException>(async () => await Unwrap(pipeline.Invocation));

            Assert.IsType<ArgumentException>(pipeline.CurrentException);
            Assert.Null(pipeline.CurrentReturnValue);
            Assert.Equal(_exceptionExecutionStack, aspect.ExecutionStack);
            
            InvocationAssert.ProceedCalled(pipeline.Invocation);
        }

        [Fact]
        public async Task SingleAspect_ReturnOnEntry_BoundariesCalled()
        {
            // Arrange            
            var aspect = new ReturnValueAspect(BoundaryType.Entry, valueToReturn: 25);
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);
            var result = await Unwrap<int>(pipeline.Invocation);
            
            // Assert
            Assert.Equal(25, result);
            Assert.Equal(_normalExecutionStack, aspect.ExecutionStack);
            Assert.Equal(25, pipeline.CurrentReturnValue);
            
            InvocationAssert.ProceedNotCalled(pipeline.Invocation);        
        }

        [Fact]
        public async Task SingleAspect_UnhandledException_ShouldBreakPipeline()
        {
            // Arrange            
            var aspect = new ThrowAspect(typeof(ArgumentException), BoundaryType.Entry, throwAsUnhandled: true);
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);

            await Assert.ThrowsAsync<ArgumentException>(
                async () => await Unwrap<int>(pipeline.Invocation));
            
            // Assert
            Assert.Equal(new [] { new BoundaryState(BoundaryType.Entry)}, aspect.ExecutionStack);
            
            Assert.IsType<ArgumentException>(pipeline.CurrentException);          
            InvocationAssert.ProceedNotCalled(pipeline.Invocation);
        }

        [Fact]
        public async Task SingleAspect_HandledException_ShouldNotBreakPipeline()
        {
            // Arrange            
            var aspect = new ThrowAspect(typeof(ArgumentException), BoundaryType.Entry, throwAsUnhandled: false);
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);

            await Assert.ThrowsAsync<ArgumentException>(
                async () => await Unwrap<int>(pipeline.Invocation));
            
            // Assert
            Assert.Equal(new []
            {
                new BoundaryState(BoundaryType.Exit), 
                new BoundaryState(BoundaryType.Entry)
            }, aspect.ExecutionStack);
            
            Assert.IsType<ArgumentException>(pipeline.CurrentException);          
            InvocationAssert.ProceedNotCalled(pipeline.Invocation);
        }

        [Fact]
        public async Task SingleAspect_CallReturn_AfterMethodExecution_ShouldChangeResult()
        {
            // Arrange            
            var aspect = new IncrementReturnValueOnSuccess();
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);
            
            var result = await Unwrap<int>(pipeline.Invocation);
            
            // Assert
            Assert.Equal(11, result);
            Assert.Equal(_normalExecutionStack, aspect.ExecutionStack);
            Assert.Equal(11, pipeline.CurrentReturnValue);
            
            InvocationAssert.ProceedCalled(pipeline.Invocation);  
        }

        [Fact]
        public async Task SingleAspect_CallThrow_AfterMethodExecution_ShouldThrow()
        {
            // Arrange            
            var aspect = new ThrowAspect(typeof(ArgumentException), BoundaryType.Success, throwAsUnhandled: false);
            var pipeline = CreatePipeline<IService>(
                new Service(), nameof(IService.IdentityAsync), Args.Pack(aspect), Args.Box(10));
            
            // Act
            _executor.ExecutePipeline(pipeline);

            await Assert.ThrowsAsync<ArgumentException>(
                async () => await Unwrap<int>(pipeline.Invocation));
            
            // Assert
            Assert.Equal(new []
            {
                new BoundaryState(BoundaryType.Exit), 
                new BoundaryState(BoundaryType.Success), 
                new BoundaryState(BoundaryType.Entry)
            }, aspect.ExecutionStack);
            
            Assert.IsType<ArgumentException>(pipeline.CurrentException);          
            InvocationAssert.ProceedCalled(pipeline.Invocation);
        }
    }
}