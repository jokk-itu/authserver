using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AuthServer.Core;

internal static class Instrumentation
{
    public static readonly ActivitySource ActivitySource = new("AuthServer");

    // TODO Tokens built
    // TODO Tokens revoked
    // TODO Tokens fetched

    // TODO Created clients
    // TODO Updated clients
    // TODO Fetched clients
    // TODO Deleted clients

    // TODO Fetched Userinfo

    // TODO Authorize invoked (prompt, client_id)
}