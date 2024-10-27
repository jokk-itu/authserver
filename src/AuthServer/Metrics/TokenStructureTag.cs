using System.ComponentModel;

namespace AuthServer.Metrics;
internal enum TokenStructureTag
{
    [Description("jwt")]
    Jwt,

    [Description("reference")]
    Reference
}
