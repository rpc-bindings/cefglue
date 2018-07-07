using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class PrimitiveTypeSerializer : ITypeSerializer, ITypeDeserializer
    {
        private static readonly HashSet<CefValueType> SupportedTypes = new HashSet<CefValueType>
        {
            CefValueType.Int,
            CefValueType.Double,
            CefValueType.String,
            CefValueType.Bool,
            CefValueType.Binary
        };

        public bool CanHandle(Type type)
        {
            return type?.IsPrimitive == true && type != typeof(IntPtr) && type !=  typeof(UIntPtr);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            var cefValueType = cefValue.GetValueType();
            return (targetType?.IsPrimitive == true && SupportedTypes.Contains(cefValueType)) ||
                   (cefValueType == CefValueType.Int && targetType == typeof(object)) ||
                   (cefValueType == CefValueType.Double && targetType == typeof(object)) ||
                   (cefValueType == CefValueType.Bool && targetType == typeof(object));
        }

        public CefValue Serialize(object obj, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            var type = obj.GetType();
            if (!CanHandle(type))
            {
                throw new InvalidOperationException();
            }

            var result = CefValue.Create();
            if (type == typeof(char))
            {
                result.SetString(new string((char)obj, 1));
            }
            else if (type == typeof(byte))
            {
                result.SetInt((byte)obj);
            }
            else if (type == typeof(sbyte))
            {
                result.SetInt((sbyte)obj);
            }
            else if (type == typeof(short))
            {
                result.SetInt((short)obj);
            }
            else if (type == typeof(ushort))
            {
                result.SetInt((ushort)obj);
            }
            else if (type == typeof(int))
            {
                result.SetInt((int)obj);
            }
            else if (type == typeof(uint))
            {
                result.SetDouble((uint)obj);
            }
            else if (type == typeof(long))
            {
                result.SetInt64((long)obj);
            }
            else if (type == typeof(ulong))
            {
                result.SetDouble((ulong)obj);
            }
            else if (type == typeof(float))
            {
                result.SetDouble((float)obj);
            }
            else if (type == typeof(double))
            {
                result.SetDouble((double)obj);
            }
            else if (type == typeof(bool))
            {
                result.SetBool((bool)obj);
            }

            return result;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            object result = null;
            var type = value.GetValueType();
            switch (type)
            {
                case CefValueType.Bool:
                    if (targetType == typeof(bool) || targetType == typeof(object))
                    {
                        result = value.GetBool();
                    }
                    break;
                case CefValueType.Binary:
                    if (value.IsType(CefTypes.Int64) && (targetType == typeof(object) || targetType == typeof(long)))
                    {
                        result = value.GetInt64();
                    }
                    break;
                case CefValueType.Int:
                    var intVal = value.GetInt();
                    if (targetType == typeof(byte))
                    {
                        result = Convert.ToByte(intVal);
                    }
                    else if (targetType == typeof(sbyte))
                    {
                        result = Convert.ToSByte(intVal);
                    }
                    else if (targetType == typeof(short))
                    {
                        result = Convert.ToInt16(intVal);
                    }
                    else if (targetType == typeof(ushort))
                    {
                        result = Convert.ToUInt16(intVal);
                    }
                    else if (targetType == typeof(int) || targetType == typeof(object))
                    {
                        result = intVal;
                    }
                    else if (targetType == typeof(uint))
                    {
                        result = Convert.ToUInt32(intVal);
                    }
                    else if (targetType == typeof(long))
                    {
                        result = Convert.ToInt64(intVal);
                    }
                    else if (targetType == typeof(ulong))
                    {
                        result = Convert.ToUInt64(intVal);
                    }
                    else if (targetType == typeof(double))
                    {
                        result = Convert.ToDouble(intVal);
                    }
                    else if (targetType == typeof(float))
                    {
                        result = Convert.ToSingle(intVal);
                    }
                    break;
                case CefValueType.Double:
                    var doubleVal = value.GetDouble();
                    if (targetType == typeof(byte))
                    {
                        result = Convert.ToByte(doubleVal);
                    }
                    else if (targetType == typeof(sbyte))
                    {
                        result = Convert.ToSByte(doubleVal);
                    }
                    else if (targetType == typeof(short))
                    {
                        result = Convert.ToInt16(doubleVal);
                    }
                    else if (targetType == typeof(ushort))
                    {
                        result = Convert.ToUInt16(doubleVal);
                    }
                    else if (targetType == typeof(int))
                    {
                        result = Convert.ToInt32(doubleVal);
                    }
                    else if (targetType == typeof(uint))
                    {
                        result = Convert.ToUInt32(doubleVal);
                    }
                    else if (targetType == typeof(long))
                    {
                        result = Convert.ToInt64(doubleVal);
                    }
                    else if (targetType == typeof(ulong))
                    {
                        result = Convert.ToUInt64(doubleVal);
                    }
                    else if (targetType == typeof(double) || targetType == typeof(object))
                    {
                        result = doubleVal;
                    }
                    else if (targetType == typeof(float))
                    {
                        result = Convert.ToSingle(doubleVal);
                    }
                    break;
                case CefValueType.String:
                    var strVal = value.GetString();
                    if (targetType == typeof(char) && !string.IsNullOrEmpty(strVal))
                    {
                        result = strVal.First();
                    }
                    break;
            }

            return result;
        }
    }
}
