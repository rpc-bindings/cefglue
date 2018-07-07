using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Extensions;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class CacheTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\cache_tests.html")).ToString();

        public class TestObject
        {
            private readonly int value;

            public TestObject(int value)
            {
                this.value = value;
            }

            public int GetValue()
            {
                return value;
            }
        }

        [Fact]
        public async Task TestV8Cache()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("cachetest1", new object());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoundObjectCache");
            }
        }

        [Fact]
        public async Task TestV8CacheInterFrame()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("cachetest2", new object());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoundObjectNotCacheInterFrame");
            }
        }

        [Fact]
        public async Task TestObjectDescriptorCache()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("cachetest3", new object());

                int count = 0;
                browser.CefClient.ProcessMessageReceived += (sender, args) =>
                {
                    if(args.Message.Name == Messages.RpcResponseMessage)
                    {
                        count++;
                    }
                };

                await browser.LoadAsync(url);
                await browser.RunTest("testObjectDescriptorCache");
                await browser.Reload();
                await browser.RunTest("testObjectDescriptorCache");

                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task TestPopupCache()
        {
            using (var browser = new Util.Browser())
            {
                var popupBrowserTask = new TaskCompletionSource<Util.Browser>();
                browser.NewBrowser += b =>
                {
                    popupBrowserTask.TrySetResult(b);
                    b.Repository.AddBinding("cachetest4", new TestObject(2));
                };

                browser.Repository.AddBinding("cachetest4", new TestObject(1));

                await browser.LoadAsync(url);
                await browser.RunTest("testCachePerBrowser");
            }
        }
    }
}
