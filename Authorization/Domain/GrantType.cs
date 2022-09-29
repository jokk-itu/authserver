using Domain.Enums;

namespace Domain;

#nullable disable
public class GrantType
{
  public int Id { get; set; }

  public Enums.GrantType Name { get; set; }

  public ICollection<Client> Clients { get; set; }
}