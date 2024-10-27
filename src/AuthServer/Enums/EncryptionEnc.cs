using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;

public enum EncryptionEnc
{
    [Description(JweEncConstants.Aes128CbcHmacSha256)]
    Aes128CbcHmacSha256,

    [Description(JweEncConstants.Aes192CbcHmacSha384)]
    Aes192CbcHmacSha384,

    [Description(JweEncConstants.Aes256CbcHmacSha512)]
    Aes256CbcHmacSha512,
}