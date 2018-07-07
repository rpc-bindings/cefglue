using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Contract;
using DSerfozo.RpcBindings.Contract.Marshaling;
using DSerfozo.RpcBindings.Extensions;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class CallBindingTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\call_binding_tests.html")).ToString();

        public class TestClass2
        {
            private readonly string str;

            public string Value => str;

            public TestClass2(string str)
            {
                this.str = str;
            }

            public string GetValue()
            {
                return str;
            }
        }

        public delegate Task Del([BindValue]TestClass2 obj);

        public class TestClass
        {
            [return:BindValue]
            public TestClass2 GetObj(string str)
            {
                return new TestClass2(str);
            }

            [return:BindValue(ExtractPropertyValues = true)]
            public TestClass2 GetPropObj(string str)
            {
                return new TestClass2(str);
            }

            public void TestCallback(Del func, string str)
            {
                func(new TestClass2(str));
            }
        }

        [Fact]
        public async Task ReturnValueBound()
        {
            using (var browser = new Util.Browser())
            {
                var obj = new TestClass();
                browser.Repository.AddBinding("bindingTest", obj);

                await browser.LoadAsync(url);
                await browser.RunTest("returnValueBound");
            }
        }

        [Fact]
        public async Task CallbackValueBound()
        {
            using (var browser = new Util.Browser())
            {
                var obj = new TestClass();
                browser.Repository.AddBinding("bindingTest2", obj);

                await browser.LoadAsync(url);
                await browser.RunTest("callbackValueBound");
            }
        }

        [Fact]
        public async Task ReturnValuePropertiesExtracted()
        {
            using (var browser = new Util.Browser())
            {
                var obj = new TestClass();
                browser.Repository.AddBinding("bindingTest3", obj);

                await browser.LoadAsync(url);
                await browser.RunTest("returnValuePropertiesExtracted");
            }
        }
    }
}
