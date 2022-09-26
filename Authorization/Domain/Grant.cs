using Domain.Enums;

namespace Domain;

#nullable disable
public class Grant
{
  public int Id { get; set; }

  public GrantType Name { get; set; }

  public ICollection<Client> Clients { get; set; }
}