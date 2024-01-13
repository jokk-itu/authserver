using Domain;

namespace Specs.Helpers.EntityBuilders;
public class PairwiseIdentifierBuilder
{
  private readonly PairwiseIdentifier _pairwiseIdentifier;

  private PairwiseIdentifierBuilder()
  {
    _pairwiseIdentifier = new PairwiseIdentifier();
  }

  public static PairwiseIdentifierBuilder Instance()
  {
    return new PairwiseIdentifierBuilder();
  }

  public PairwiseIdentifier Build()
  {
    return _pairwiseIdentifier;
  }

  public PairwiseIdentifierBuilder AddUser(User user)
  {
    _pairwiseIdentifier.User = user;
    return this;
  }

  public PairwiseIdentifierBuilder AddClient(Client client)
  {
    _pairwiseIdentifier.Client = client;
    return this;
  }
}