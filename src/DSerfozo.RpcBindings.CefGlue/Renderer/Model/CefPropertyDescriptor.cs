using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.RpcBindings.Model;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Model
{
    public class CefPropertyDescriptor : PropertyDescriptor
    {
        public ICefValue ListValue { get; }

        public CefPropertyDescriptor(long id, string name, ICefValue value)
        {
            Id = id;
            ListValue = value;
            Name = name;
        }
    }
}
