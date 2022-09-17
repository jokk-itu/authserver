namespace Domain;

#nullable disable
public class Contact
{
  public int Id { get; set; }

  public string Email { get; set; }

  public ICollection<Client> Clients { get; set; }
}