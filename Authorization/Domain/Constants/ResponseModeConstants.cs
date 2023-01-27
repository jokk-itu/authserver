namespace Domain.Constants;
public static class ResponseModeConstants
{
  public const string Query = "query";
  public const string FormPost = "form_post";
  public static readonly string[] ResponseModes = { Query, FormPost };
}
