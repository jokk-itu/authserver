using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

var test1 = () =>
{
  //Real example from app
  var accessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiIxNDU4YWYxOC02MTRmLTRjMGYtYmMwMS01NDEyZGE4N2M3MTciLCJhdWQiOiIgYXBpMSIsImlzcyI6Imh0dHA6Ly9hdXRoLWFwcDo4MCIsImlhdCI6IjA3LzE5LzIwMjIgMDg6MjU6MTggKzAwOjAwIiwiZXhwIjoiMDcvMTkvMjAyMiAwOToyNToxOCArMDA6MDAiLCJuYmYiOiIwNy8xOS8yMDIyIDA4OjI1OjE4ICswMDowMCIsImp0aSI6ImQ4YTc2MDVlLTY0NTUtNGEwYy1hZTA3LWE0MzY0M2MzNzc2MSIsInNjb3BlIjoiYXBpMSBvcGVuaWQgcHJvZmlsZSIsImNsaWVudF9pZCI6InRlc3QifQ.C2qtCGJ5LeQlTw6rFmh8OIMPlDKaKe2-Wb3t2pWglDht7ULiU-QHaMgESyrJPJv_EbrWsJeTf7okz1455AzcKKVeevOqteMwmXacgm9ts8Rk8QCWEQo_gpVAqcStE8LIrhj-4YNHwRuX0EPRlUrCxGSX8Ki7WUgMH0zatpug7S_wSf9WAcpNedusK-bcFp_MurWGZAh5mtetahr8xPdVonckNdMk9_PghuKlPLHwQy5W5Bs2W2496Y-c0Br3ETKgbfWV-cosF4ozNIKLuUlzUWUjsP2B_rehbVQA06M7zuHcCAbT8fZOPLpweYdvF3PUr2_0BFk2GeIhd_ZHsilsik5ok4xav9J14RARhiWGq-cGRAfTYUBCQ7P-JqFTraVjTSIxdnU2FpjUsRPyxJSpwFsK900jJyYoXPWv9r_w9seS9vCWNViNOKpOkbZ8yA7OynQ9a8THcucRo_fmuSBM9G16UU0ncZL9z21Xue4NxRy_ZR0l9ZcnDYLgWy3E6kyeCZc5MBSJxDDzrwQiBxY6nPC6Iqfh7VR1xexeiE-XsoFXLP0sWJp86pmwuNIctZVSMOA0TrrW21HVQ_ELJkHBGVw03k1MI3iXBfJh3fEXvtSwtoqvEEw4GvWCpkNORhzgN8DmLpk3ArVxoXUw655E3erWXiwy9ecA67H7a4zTCuA";
  var e = "AQAB";
  var n = "77-9ET_vv71G77-92K9zH--_ve-_ve-_vQZV77-977-9Dihz77-977-977-9De-_vUPvv70l77-9LEhjcgw-Zu-_ve-_ve-_vWR50Ybvv71B77-977-977-9Se-_vTrvv73vv702DjIHaQTvv70C77-9FxFq77-9BwQ477-9Cu-_vUHvv73KiE0d77-977-977-977-9y5RoZX0WEe-_vcS177-977-977-9bu-_vXvvv719PEbvv73vv73vv71T77-977-9bO-_ve-_vTDvv73vv73vv71dLl3vv73vv71s77-9OQxnZe-_ve-_vTPvv70i77-9Se-_ve-_ve-_vRXvv73vv71977-9bzvvv73vv70yXO-_vXPvv71A77-977-9UndiOmrvv73vv73vv73vv73vv70LYe-_ve-_vU7vv718SO-_vTNdIu-_vQgX77-9bHJIF--_vdK2WibVgBXvv73vv71kDWPvv73vv73vv73vv73vv70377-9Thdd77-9Ze-_ve-_ve-_ve-_vQTvv73vv73vv73vv73vv70vSO-_ve-_ve-_vSzvv73vv713fe-_vTBQ77-9D--_vRrvv70wBSJl77-977-9aO-_vT3vv70Q77-9ZHsiUu-_vS3vv73vv73vv73vv70BOGMk77-977-977-9R9OF77-9L1jvv73vv73vv71bXO-_vWQD77-9HO-_vUlw77-977-9Xe-_vRF8DO-_ve-_ve-_vX7vv70DCO-_vVbvv70hRBLvv73vv70iD--_vURp77-9P--_vUvvv73vv73vv70P77-977-9KBvvv71MHnfvv71Y77-977-9Ce-_ve-_vQZz77-977-977-9W04577-9O13vv70877-9CO-_vXQpSEzvv71P77-9Du-_vXlI77-9Ke-_vUPvv70zTGBQ77-977-9OO-_vWTvv73ak1Pvv71M77-9dVBM77-977-9UBd977-9V3QR77O877-9R--_ve-_vUZIQe-_ve-_ve-_vWwGJTzvv73vv70R77-9GkLvv73vv71RR2g8bXEfHe-_vXfvv73vv73vv73vv70i77-9Gu-_vUTvv73vv73vv71x77-977-977-977-93JI32JZfT--_vQ8G77-977-9G--_vUjvv73vv71Pce-_ve-_vQTvv73fnkIfdkdGWH_vv73vv70XQe-_ve-_vWBubhNALu-_vQFvFO-_vQAWRu-_vV7vv73vv70paQ";

  var rsaParameters = new RSAParameters
  {
    Modulus = Encoding.UTF8.GetBytes(Base64UrlEncoder.Decode(n)),
    Exponent = Encoding.UTF8.GetBytes(Base64UrlEncoder.Decode(e))
  };
  var tokenValidationParameters = new TokenValidationParameters
  {
    IssuerSigningKey = new RsaSecurityKey(rsaParameters) { KeyId = "1" },
    ValidAudience = "api1",
    ValidateLifetime = false
  };
  var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);
  Console.WriteLine($"Claim one: {claimsPrincipal.Claims.First()}");
};

var test2 = () => 
{
  //Create token and public key
  var rsa = new RSACryptoServiceProvider(4096);
  var key = new RsaSecurityKey(rsa)
  {
    KeyId = "1"
  };
  var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256) 
  {
    CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
  };
  var securityToken = new JwtSecurityToken(
    issuer: "http://auth-app:80", 
    audience: "api1", 
    notBefore: DateTime.UtcNow.AddDays(-1), 
    expires: DateTime.UtcNow.AddDays(1), 
    signingCredentials: signingCredentials);

  var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

  //Validate token using public key
  var rsaParameters = new RSAParameters()
  {
    Exponent = rsa.ExportParameters(false).Exponent,
    Modulus = rsa.ExportParameters(false).Modulus
  };
  var jwtHandler = new JwtSecurityTokenHandler();
  var validationParameters = new TokenValidationParameters 
  {
    ValidIssuer = "http://auth-app:80",
    ValidAudience = "api1",
    IssuerSigningKey = new RsaSecurityKey(rsaParameters) 
    {
      KeyId = "1"
    },
    IssuerSigningKeyValidatorUsingConfiguration = (securityKey, securityToken, validationParameters, configuration) => 
    {
      return true;
    }
  };
  var claimsPrincipal = jwtHandler.ValidateToken(token, validationParameters, out var validatedToken);
  Console.WriteLine($"Claim one: {claimsPrincipal.Claims.First()}");
};

IdentityModelEventSource.ShowPII = true;
test1();