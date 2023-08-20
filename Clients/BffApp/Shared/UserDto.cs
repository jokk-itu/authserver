namespace Shared;
public class UserDto
{
  public IEnumerable<ClaimDto> Claims { get; init; } = new List<ClaimDto>();
}
