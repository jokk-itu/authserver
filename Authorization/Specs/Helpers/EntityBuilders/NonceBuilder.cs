using Domain;
using Infrastructure.Helpers;

namespace Specs.Helpers.EntityBuilders;
public class NonceBuilder
{
  private readonly Nonce _nonce;

  private NonceBuilder(string id)
  {
    _nonce = new Nonce
    {
      Id = id,
      Value = CryptographyHelper.GetRandomString(16)
    };
  }

  public static NonceBuilder Instance(string id)
  {
    return new NonceBuilder(id);
  }

  public Nonce Build()
  {
    return _nonce;
  }
}