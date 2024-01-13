namespace Domain.Entity;
#nullable disable
public abstract class ClientToken : Token
{
    public Client Client { get; set; }
}
