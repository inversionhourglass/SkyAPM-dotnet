using Microsoft.AspNetCore.Mvc;
using SkyApm.Tracing;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Sample.Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TestController : Controller
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITracingContext _tracingContext;

        public TestController(IHttpClientFactory factory, ITracingContext tracingContext)
        {
            _factory = factory;
            _tracingContext = tracingContext;
        }

        public Task<string> Normal()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                await BackendDelayAsync(200);
                await BackendDelayAsync(300);
            });
        }

        public Task<string> Parallel()
        {
            return Timing(async () =>
            {
                var t1 = BackendDelayAsync(3000);
                var t2 = BackendDelayAsync(1000);
                var t3 = BackendDelayAsync(2000);

                await Task.WhenAll(t1, t2, t3);
            });
        }

        public Task<string> Background()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                BackendDelayAsync(3000);
                //_tracingContext.ScopeSegmentAsync(async () => await BackendDelayAsync(3000));
                await BackendDelayAsync(300);
            });
        }

        public Task<string> BackgroundTimeout()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                BackendDelayAsync(10000);
                //_tracingContext.ScopeSegmentAsync(async () => await BackendDelayAsync(10000));
                await BackendDelayAsync(300);
            });
        }

        public Task<string> BgTask()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                Task.Run(async () => await BackendDelayAsync(3000));
                //SkyApmTask.Run(async () => await BackendDelayAsync(3000));
                await BackendDelayAsync(300);
            });
        }

        public Task<string> BgThread()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                new Thread(() => BackendDelayAsync(3000).GetAwaiter().GetResult()).Start();
                //new SkyApmThread(() => BackendDelayAsync(3000).GetAwaiter().GetResult()).Start();
                await BackendDelayAsync(300);
            });
        }

        public Task<string> BgThreadPool()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                ThreadPool.QueueUserWorkItem(state => BackendDelayAsync(3000).GetAwaiter().GetResult());
                //SkyApmThreadPool.QueueUserWorkItem(state => BackendDelayAsync(3000).GetAwaiter().GetResult());
                await BackendDelayAsync(300);
            });
        }

        public Task<string> BgTimer()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                var timer = new Timer(state => BackendDelayAsync(3000).GetAwaiter().GetResult());
                //var timer = new SkyApmTimer(state => BackendDelayAsync(3000).GetAwaiter().GetResult());
                timer.Change(100, Timeout.Infinite);
                await BackendDelayAsync(300);
            });
        }

        public Task<string> DelayStart()
        {
            return Timing(async () =>
            {
                await BackendDelayAsync(100);
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(10000);
                    await BackendDelayAsync(200);
                });
                //Task.Factory.StartSkyApmNew(async () =>
                //{
                //    await Task.Delay(10000);
                //    await BackendDelayAsync(200);
                //});
                await BackendDelayAsync(300);
            });
        }

        private async Task<string> Timing(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();

            await action();
            
            stopwatch.Stop();
            return $"cost {stopwatch.ElapsedMilliseconds}ms.";
        }

        private async Task<string> BackendDelayAsync(int ms)
        {
            using var client = _factory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5002/api/delay/{ms}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
