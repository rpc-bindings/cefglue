using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Common;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public class ObjectSerializer
    {
        protected sealed class SeenDisposable : IDisposable
        {
            private readonly Stack<object> stack;
            private readonly bool pop;

            public SeenDisposable(Stack<object> stack, bool pop)
            {
                this.stack = stack;
                this.pop = pop;
            }

            public void Dispose()
            {
                if (pop)
                {
                    stack.Pop();
                }
            }
        }

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

        public ICefValue Serialize(object obj)
        {
            return Serialize(obj, new Stack<object>());
        }

        public ICefValue Serialize(object obj, Stack<object> seen)
        {
            ICefValue result;

            var type = obj?.GetType();
            var serializer = Serializers.FirstOrDefault(s => s.CanHandle(type));

            using (HandleSeen(seen, obj, type, out var canSerialize))
            {
                if (serializer != null && canSerialize)
                {
                    result = serializer.Serialize(obj, seen, this);
                }
                else
                {
                    result = CefValue.Create();
                    result.SetNull();
                }
            }

            return result;
        }

        public object Deserialize(ICefValue value, Type targetType, int? index = null, string key = null)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                targetType = underlyingType;
            }

            var deserializer = Deserializers.FirstOrDefault(s => s.CanHandle(value, targetType));
            return deserializer?.Deserialize(value, targetType, this);
        }

        protected virtual IDisposable HandleSeen(Stack<object> seen, object current, Type currentType, out bool go)
        {
            var pushed = false;
            go = true;
            if (currentType?.IsPrimitive == false && !currentType.IsValueType && currentType != typeof(string))
            {
                go = !seen.Contains(current);
                if (go)
                {
                    seen.Push(current);
                    pushed = true;
                }
            }

            return new SeenDisposable(seen, pushed);
        }
    }
}
