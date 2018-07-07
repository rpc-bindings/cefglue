using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Model
{
    public class CefPropertyDescriptor : PropertyDescriptor
    {
        public CefValue ListValue { get; }

        public CefPropertyDescriptor(long id, string name, CefValue value)
        {
            Id = id;
            ListValue = value;
            Name = name;
        }
    }
}
