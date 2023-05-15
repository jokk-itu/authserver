using System.Security.Cryptography;

namespace Domain;

#nullable disable
public class Resource
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; }
  public string Secret { get; set; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
  public ICollection<Scope> Scopes { get; set; } = new List<Scope>();
}