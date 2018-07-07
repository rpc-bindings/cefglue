using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class PrimitiveTypeToV8BindingTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\primitive_tov8_tests.html")).ToString();

        public class TestClass
        {
            public Int64 TestInt64()
            {
                return -9223372036854775806;
            }

            public bool TestBool()
            {
                return true;
            }

            public int TestInt()
            {
                return -2147483647;
            }

            public uint TestUInt()
            {
                return 4294967295;
            }

            public ulong TestUInt64()
            {
                return 18446744073709551614;
            }

            public decimal TestDecimal()
            {
                return 300.5m;
            }

            public float TestSingle()
            {
                return 1.23456678f;
            }

            public double TestDouble()
            {
                return 1.23456789134565677890124556;
            }

            public byte TestByte()
            {
                return 2;
            }

            public sbyte TestSByte()
            {
                return -2;
            }

            public short TestInt16()
            {
                return -10000;
            }

            public ushort TestUInt16()
            {
                return 20000;
            }

            public char TestChar()
            {
                return 'a';
            }

            public DateTime TestDate()
            {
                return DateTime.MinValue.AddYears(2018).AddMonths(3).AddDays(31).AddHours(22).AddMinutes(12)
                    .AddSeconds(23).AddMilliseconds(450);
            }

            public int[] TestArray()
            {
                return new[] {1, 2, 3, 4};
            }
        }

        [Fact]
        public async Task Int64ResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testInt64Result");
            }
        }

        [Fact]
        public async Task BoolResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoolResult");
            }
        }

        [Fact]
        public async Task IntResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testIntResult");
            }
        }

        [Fact]
        public async Task UIntResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUIntResult");
            }
        }

        [Fact]
        public async Task UInt64ResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUInt64Result");
            }
        }

        [Fact]
        public async Task DecimalResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDecimalResult");
            }
        }

        [Fact]
        public async Task SingleResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testSingleResult");
            }
        }

        [Fact]
        public async Task DoubleResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDoubleResult");
            }
        }

        [Fact]
        public async Task ByteResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testByteResult");
            }
        }

        [Fact]
        public async Task SByteResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testSByteResult");
            }
        }

        [Fact]
        public async Task Int16ResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testInt16Result");
            }
        }

        [Fact]
        public async Task UInt16ResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUInt16Result");
            }
        }

        [Fact]
        public async Task CharResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testCharResult");
            }
        }

        [Fact]
        public async Task DateResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDateResult");
            }

        }

        [Fact]
        public async Task ArrayResultConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testArrayResult");
            }

        }
    }
}