using System;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public static class CefValueExtensions
    {
        private static readonly DateTime DateTime = new DateTime(1970, 1, 1).ToUniversalTime();

        public static bool IsType(this CefValue @this, CefTypes type)
        {
            return IsType(() => @this, type);
        }

        public static bool IsType(this CefListValue @this, int index, CefTypes type)
        {
            return IsType(() => @this.GetValue(index), type);
        }

        public static bool IsType(this CefDictionaryValue @this, string index, CefTypes type)
        {
            return IsType(() => @this.GetValue(index), type);
        }

        public static void SetTime(this CefValue @this, DateTime value)
        {
            SetTime(_ => @this.SetBinary(_), value);
        }

        public static void SetTime<TIndex>(this CefValue @this, DateTime value, TIndex index = default(TIndex))
        {
            SetTime(_ =>
            {
                var valueType = @this.GetValueType();
                switch (valueType)
                {
                    case CefValueType.List:
                        using (var listValue = @this.GetList())
                        {
                            if (typeof(TIndex) == typeof(int))
                            {
                                listValue.SetBinary((int)Convert.ChangeType(index, typeof(int)), _);
                            }
                        }
                        break;
                    case CefValueType.Dictionary:
                        using (var dictValue = @this.GetDictionary())
                        {
                            if (typeof(TIndex) == typeof(string))
                            {
                                dictValue.SetBinary((string)Convert.ChangeType(index, typeof(string)), _);
                            }
                        }
                        break;
                    default:
                        @this.SetBinary(_);
                        break;
                }
            }, value);
        }

        public static void SetTime(this CefListValue @this, int index, DateTime value)
        {
            SetTime(_ => @this.SetBinary(index, _), value);
        }

        public static void SetTime(this CefDictionaryValue @this, string index, DateTime value)
        {
            SetTime(_ => @this.SetBinary(index, _), value);
        }

        public static DateTime GetTime(this CefValue @this)
        {
            return GetTime(() => @this);
        }

        public static DateTime GetTime(this CefListValue @this, int index)
        {
            return GetTime(() => @this.GetValue(index));
        }

        public static DateTime GetTime(this CefDictionaryValue @this, string index)
        {
            return GetTime(() => @this.GetValue(index));
        }

        public static long GetInt64(this CefValue @this)
        {
            return GetInt64(() => @this);
        }

        public static long GetInt64(this CefListValue @this, int index)
        {
            return GetInt64(() => @this.GetValue(index));
        }

        public static long GetInt64(this CefDictionaryValue @this, string index)
        {
            return GetInt64(() => @this.GetValue(index));
        }

        public static void SetInt64(this CefValue @this, long value)
        {
            SetInt64(_ => @this.SetBinary(_), value);
        }

        public static void SetInt64(this CefListValue @this, int index, long value)
        {
            SetInt64(_ => @this.SetBinary(index, _), value);
        }

        public static void SetInt64(this CefDictionaryValue @this, string index, long value)
        {
            SetInt64(_ => @this.SetBinary(index, _), value);
        }

        private static bool IsType(Func<CefValue> getValue, CefTypes type)
        {
            var @this = getValue();
            if (@this.GetValueType() != CefValueType.Binary)
                return false;

            using (var cefBinaryValue = @this.GetBinary())
            {
                var buffer = new byte[1];
                cefBinaryValue.GetData(buffer, 1, 0);

                return type == (CefTypes) buffer[0];
            }
        }

        private static void SetTime(Action<CefBinaryValue> setValue, DateTime value)
        {
            var totalSecondsBytes = BitConverter.GetBytes(value.ToBinary());
            var buffer = new byte[totalSecondsBytes.Length + 1];
            buffer[0] = (byte) CefTypes.Time;
            Array.Copy(totalSecondsBytes, 0, buffer, 1, totalSecondsBytes.Length);

            using (var binaryValue = CefBinaryValue.Create(buffer))
            {
                setValue(binaryValue);
            }
        }

        private static DateTime GetTime(Func<CefValue> getValue)
        {
            var @this = getValue();
            if (@this.GetValueType() != CefValueType.Binary)
                return default(DateTime);

            using (var binaryValue = @this.GetBinary())
            {
                var buffer = new byte[binaryValue.Size];
                binaryValue.GetData(buffer, binaryValue.Size, 0);

                return DateTime.FromBinary(BitConverter.ToInt64(buffer, 1));
            }
        }

        private static long GetInt64(Func<CefValue> getValue)
        {
            var @this = getValue();
            if (@this.GetValueType() != CefValueType.Binary)
                return 0L;

            using (var binaryValue = @this.GetBinary())
            {
                var buffer = new byte[binaryValue.Size];
                binaryValue.GetData(buffer, binaryValue.Size, 0);

                return BitConverter.ToInt64(buffer, 1);
            }
        }

        private static void SetInt64(Action<CefBinaryValue> setValue, long value)
        {
            var buffer= new byte[sizeof(long) + 1];
            buffer[0] = (byte) CefTypes.Int64;
            var int64Bytes = BitConverter.GetBytes(value);
            Array.Copy(int64Bytes, 0, buffer, 1, int64Bytes.Length);

            using (var binaryValue = CefBinaryValue.Create(buffer))
            {
                setValue(binaryValue);
            }
        }
    }
}
