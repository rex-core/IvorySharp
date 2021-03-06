﻿using System.Threading.Tasks;

namespace IvorySharp.Benchmark.Services
{
    public class ServiceForBenchmark : IServiceForBenchmark
    {
        public T IdenitityGeneric<T>(T arg)
        {
            return arg;
        }

        public int Identity(int value)
        {
            return value;
        }

        public async Task<int> IdentityAsync(int value)
        {
            return await Task.FromResult(value)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public int InterceptedIdentity(int value)
        {
            return value;
        }
    }
}