using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Extensions;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class ObjectV8BindingTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\object_tests.html")).ToString();

        public class InnerPoco
        {
            [DataMember(Name = "num")]
            public double Num { get; set; }
        }

        public class Poco
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "value")]
            public int Value { get; set; }

            [DataMember(Name = "obj")]
            public InnerPoco Obj { get; set; }
        }

        public class LoopedPoco
        {
            public string Name { get; set; }

            public object Loop { get; set; }
        }

        public class TestClass
        {
            public bool TestPocoInput(Poco poco)
            {
                return poco.Name == "name" && poco.Value == 2 && poco.Obj != null && Math.Abs(poco.Obj.Num - 2.3) < 0.00001;
            }

            public bool TestLoopInput(LoopedPoco poco)
            {
                return poco.Name == "name" && poco.Loop == null;
            }

            public LoopedPoco TestLoopResult()
            {
                var obj = new LoopedPoco {Name = "name"};
                obj.Loop = obj;

                return obj;
            }

            public Poco TestPocoResult()
            {
                return new Poco
                {
                    Name = "name",
                    Value = 2,
                    Obj = new InnerPoco
                    {
                        Num = 2.3
                    }
                };
            }
        }

        [Fact]
        public async Task PocoInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testPocoInput");
            }
        }

        [Fact]
        public async Task PocoResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testPocoResult");
            }
        }

        [Fact]
        public async Task LoopedObjectGraphNotCausesStackOveflow()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testLoopPocoResult");
            }
        }

        [Fact]
        public async Task LoopedV8ObjectGraphNotCausesStackOverflow()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testLoopPocoInput");
            }
        }
    }
}
