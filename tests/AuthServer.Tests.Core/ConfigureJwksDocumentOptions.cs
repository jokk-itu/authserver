﻿using AuthServer.Core.Discovery;
using Microsoft.Extensions.Options;

namespace AuthServer.Tests.Core;
internal class ConfigureJwksDocumentOptions : IPostConfigureOptions<JwksDocument>
{
    public void PostConfigure(string? name, JwksDocument options)
    {}
}