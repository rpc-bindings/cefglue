using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.Browser;
using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Extensions;
using Xilium.CefGlue;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class DynamicBinderTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\binding_tests.html")).ToString();

        private class TestClass1
        {
            public void TestFunc()
            {
                
            }

            public string TestFuncString(string input)
            {
                return input + " World!";
            }
        }

        [Fact]
        public async Task BindingInterfacePresent()
        {
            using (var browser = new Util.Browser())
            {
                await browser.LoadAsync(url);
                await browser.RunTest("testBindingObjectExists");
            }
        }

        [Fact]
        public async Task RequireReturnsPromise()
        {
            using (var browser = new Util.Browser())
            {
                await browser.LoadAsync(url);
                await browser.RunTest("requireReturnsPromise");
            }
        }

        [Fact]
        public async Task ObjectBound()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new object());

                await browser.LoadAsync(url);
                await browser.RunTest("testObjectBound");
            }
        }

        [Fact]
        public async Task ObjectBoundWithFunction()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass1());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoundObjectFunctionPresent");
            }
        }

        [Fact]
        public async Task BoundFunctionReturnsPromise()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass1());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoundObjectFunctionReturnsPromise");
            }
        }

        [Fact]
        public async Task MissingDynamicObjectFails()
        {
            using (var browser = new Util.Browser())
            {
                await browser.LoadAsync(url);
                await browser.RunTest("testBoundObjectNotExists");
            }
        }


        [Fact]
        public async Task SimpleStringParameterAndResultWorks()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass1());

                await browser.LoadAsync(url);
                await browser.RunTest("testSimpleStringConcatFunction");
            }
        }
    }
}
