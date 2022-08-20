namespace Domain;

#nullable disable
public class Code
{
  public long Id { get; set; }

  public bool IsRedeemed { get; set; }

  public CodeType CodeType { get; set; }

  public string Value { get; set; }

  public Client Client { get; set; }
}