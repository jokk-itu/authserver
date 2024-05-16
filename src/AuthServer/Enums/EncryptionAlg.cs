using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;

public enum EncryptionAlg
{
    [Description(JweAlgConstants.RsaPKCS1)]
    RsaPKCS1,

    [Description(JweAlgConstants.RsaOAEP)]
    RsaOAEP,

    [Description(JweAlgConstants.EcdhEsA128KW)]
    EcdhEsA128KW,

    [Description(JweAlgConstants.EcdhEsA192KW)]
    EcdhEsA192KW,

    [Description(JweAlgConstants.EcdhEsA256KW)]
    EcdhEsA256KW
}