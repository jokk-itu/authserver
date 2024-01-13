namespace Domain;

public class Role
{
  public int Id { get; set; }
  public string Value { get; set; } = null!;
  public ICollection<User> Users { get; set; } = new List<User>();
}