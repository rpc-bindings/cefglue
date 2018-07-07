using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Contract;
using DSerfozo.RpcBindings.Contract.Marshaling;
using DSerfozo.RpcBindings.Extensions;
using DSerfozo.RpcBindings.Marshaling;
using Xilium.CefGlue;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class CallbackBindingTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\callback_tests.html")).ToString();

        public class TestClass
        {
            private long id;

            public long Id => id;

            public void TestCallback(ICallback callback, string input)
            {
                callback.ExecuteAsync(input + " World!");
            }

            public async Task<string> TestPromiseCallback(ICallback callback)
            {
                var obj = await callback.ExecuteAsync();
                return obj as string;
            }

            public void TestCallbackDispose(ICallback callback)
            {
                var cb = (Callback<CefValue>) callback;
                id = cb.Id;

                callback.Dispose();
            }
        }

        [Fact]
        public async Task TestSimpleCallback()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testCallback");
            }
        }

        [Fact]
        public async Task TestPromiseCallback()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testCallbackWithPromise");
            }
        }

        [Fact]
        public async Task TestCallbackDispose()
        {
            using (var browser = new Util.Browser())
            {
                var obj = new TestClass();
                browser.Repository.AddBinding("test", obj);

                await browser.LoadAsync(url);
                await browser.RunTest("testCallbackDispose");

                await Assert.ThrowsAsync<Exception>(() => new Callback<CefValue>(obj.Id, browser.BindingHost.CallbackExecutor,
                    contet => { }).ExecuteAsync());
            }
        }

        [Fact]
        public async Task TestCallbackBoundToJSObject()
        {
            using (var browser = new Util.Browser())
            {
                var obj = new TestClass();
                browser.Repository.AddBinding("test", obj);

                await browser.LoadAsync(url);
                await browser.RunTest("testCallbackBound");
            }
        }
    }
}
