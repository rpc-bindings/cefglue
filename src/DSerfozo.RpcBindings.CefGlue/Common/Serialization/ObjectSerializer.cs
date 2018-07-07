using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public class ObjectSerializer
    {
        public const string TypeIdPropertyName = "TypeId";
        public const string ValuePropertyName = "Value";
        public const string DictionaryTypeId = "25CDD082-4A67-48A0-A342-1FAB77BC6450";

        public IList<ITypeSerializer> Serializers { get; } = new List<ITypeSerializer>()
        {
            new CefTypesSerializer(),
            new DictionarySerializer(),
            new ArraySerializer(),
            new PrimitiveTypeSerializer(),
            new StringSerializer(),
            new CefPropertyDescriptorSerializer(),
            new ComplexTypeSerializer(),
            new ValueTypeSerializer()
        };

        public IList<ITypeDeserializer> Deserializers { get; } = new List<ITypeDeserializer>
        {
            new CefTypesSerializer(),
            new DictionarySerializer(),
            new ArraySerializer(),
            new PrimitiveTypeSerializer(),
            new StringSerializer(),
            new CefPropertyDescriptorSerializer(),
            new ComplexTypeSerializer(),
            new ValueTypeSerializer()
        };

        public bool IsKnownType(string typeId)
        {
            return ComplexTypeSerializer.KnownTypes.ContainsKey(typeId);
        }

        public CefValue Serialize(object obj, HashSet<object> seen)
        {
            CefValue result;

            var type = obj?.GetType();
            var serializer = Serializers.FirstOrDefault(s => s.CanHandle(type));

            if (serializer != null && HandleSeen(seen, obj, type))
            {
                result = serializer.Serialize(obj, seen, this);
            }
            else
            {
                result = CefValue.Create();
                result.SetNull();
            }

            return result;
        }

        public object Deserialize(CefValue value, Type targetType, int? index = null, string key = null)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                targetType = underlyingType;
            }

            var deserializer = Deserializers.FirstOrDefault(s => s.CanHandle(value, targetType));
            return deserializer?.Deserialize(value, targetType, this);
        }

        protected virtual bool HandleSeen(HashSet<object> seen, object current, Type currentType)
        {
            var result = true;
            if (!currentType.IsPrimitive && !currentType.IsValueType && currentType != typeof(string))
            {
                result = !seen.Contains(current);
                if (result)
                {
                    seen.Add(current);
                }
            }

            return result;
        }
    }
}
