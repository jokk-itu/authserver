using System.Collections.ObjectModel;

namespace Domain;

#nullable disable
public class ResponseType
{
  public int Id { get; set; }
  public string Name { get; set; }
  public ICollection<Client> Clients { get; set; } = new List<Client>();
}