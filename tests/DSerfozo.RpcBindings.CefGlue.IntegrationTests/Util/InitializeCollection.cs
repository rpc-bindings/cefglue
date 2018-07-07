using Xunit;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util
{
    [CollectionDefinition(Definition)]
    public class InitializeCollection : ICollectionFixture<Initializer>
    {
        public const string Definition = "Cef Init";
    }
}
