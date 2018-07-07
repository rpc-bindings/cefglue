using DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util;
using DSerfozo.RpcBindings.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests
{
    [Collection(InitializeCollection.Definition)]
    public class PrimitiveTypeFromV8BindingTests
    {
        private static readonly string url =
            new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Html\\primitive_fromv8_tests.html")).ToString();

        public class TestClass
        {
            public bool TestInt64(long val)
            {
                return val == -922337203685477;
            }

            public bool TestBool(bool val)
            {
                return val == true;
            }

            public bool TestInt(int val)
            {
                return val == -2147483647;
            }

            public bool TestUInt(uint val)
            {
                return val == 4294967295;
            }

            public bool TestUInt64(ulong val)
            {
                return val == 1844674407370955;
            }

            public bool TestDecimal(decimal val)
            {
                return val == 300.5m;
            }

            public bool TestSingle(float val)
            {
                return Math.Abs(val - 1.23456678f) < 0.0000001;
            }

            public  bool TestDouble(double val)
            {
                return Math.Abs(val - 1.23456789134565677890124556) < 0.000000000000001;
            }

            public bool TestByte(byte val)
            {
                return val == 2;
            }

            public bool TestSByte(sbyte val)
            {
                return val == -2;
            }

            public bool TestInt16(short val)
            {
                return val == -10000;
            }

            public bool TestUInt16(ushort val)
            {
                return val == 20000;
            }

            public bool TestChar(char val)
            {
                return val == 'a';
            }

            public bool TestDate(DateTime val)
            {
                return val == DateTime.MinValue.AddYears(2018).AddMonths(3).AddDays(31).AddHours(22).AddMinutes(12)
                    .AddSeconds(23).AddMilliseconds(450).ToUniversalTime();
            }

            public bool TestArray(int[] arr)
            {
                return arr.SequenceEqual(new[] {1, 2, 3, 4});
            }

            public bool TestNullable(int? val)
            {
                return val.HasValue && val.Value == 2;
            }
        }

        [Fact]
        public async Task Int64InputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testInt64Input");
            }
        }

        [Fact]
        public async Task BoolInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testBoolInput");
            }
        }

        [Fact]
        public async Task IntInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testIntInput");
            }
        }

        [Fact]
        public async Task UIntInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUIntInput");
            }
        }

        [Fact]
        public async Task UInt64InputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUInt64Input");
            }
        }

        [Fact]
        public async Task DecimalInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDecimalInput");
            }
        }

        [Fact]
        public async Task SingleInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testSingleInput");
            }
        }

        [Fact]
        public async Task DoubleInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDoubleInput");
            }
        }

        [Fact]
        public async Task ByteInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testByteInput");
            }
        }

        [Fact]
        public async Task SByteInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testSByteInput");
            }
        }

        [Fact]
        public async Task Int16InputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testInt16Input");
            }
        }

        [Fact]
        public async Task UInt16InputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testUInt16Input");
            }
        }

        [Fact]
        public async Task CharInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testCharInput");
            }
        }

        [Fact]
        public async Task DateInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testDateInput");
            }

        }

        [Fact]
        public async Task ArrayInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testArrayInput");
            }
        }

        [Fact]
        public async Task NullableInputConverted()
        {
            using (var browser = new Util.Browser())
            {
                browser.Repository.AddBinding("test", new TestClass());

                await browser.LoadAsync(url);
                await browser.RunTest("testNullableInput");
            }
        }
    }
}