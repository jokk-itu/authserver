using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;

public enum SigningAlg
{
    [Description(JwsAlgConstants.EcdsaSha256)]
    EcdsaSha256,

    [Description(JwsAlgConstants.EcdsaSha384)]
    EcdsaSha384,

    [Description(JwsAlgConstants.EcdsaSha512)]
    EcdsaSha512,

    [Description(JwsAlgConstants.RsaSha256)]
    RsaSha256,

    [Description(JwsAlgConstants.RsaSha384)]
    RsaSha384,

    [Description(JwsAlgConstants.RsaSha512)]
    RsaSha512,

    [Description(JwsAlgConstants.RsaSsaPssSha256)]
    RsaSsaPssSha256,

    [Description(JwsAlgConstants.RsaSsaPssSha384)]
    RsaSsaPssSha384,

    [Description(JwsAlgConstants.RsaSsaPssSha512)]
    RsaSsaPssSha512
}