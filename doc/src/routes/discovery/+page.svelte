<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="Discovery Endpoint"/>
<Section title="Introduction">
Discovery is used for relying parties to fetch information
about the authorization server from a well known uri.
Information includes endpoints and supported features.
</Section>
<Section title="Specifications">
<ul class="list-disc">
    <li>
        <a href="https://datatracker.ietf.org/doc/html/rfc7518">JWA</a>
    </li>
    <li>
        <a href="https://datatracker.ietf.org/doc/html/rfc8414">OAuth Discovery</a>
    </li>
    <li>
        <a href="https://openid.net/specs/openid-connect-discovery-1_0.html">OIDC Discovery</a>
    </li>
</ul>
</Section>
<Section title="Discovery">
An endpoint used to provide metadata about the authorization server. It exposes data such as endpoints,
supported grants and algorithms used for signing tokens.
The following is available in the metadata document:<br/>
<b>issuer</b><br/>
The baseurl to the identity provider e.g: https://idp.authserver.dk
<br/><br/>
<b>authorization_endpoint</b><br/>
Absolute url to the authorize endpoint e.g: https://idp.authserver.dk/connect/authorize
<br/><br/>
<b>token_endpoint</b><br/>
Absolute url to the token endpoint e.g: https://idp.authserver.dk/connect/token
<br/><br/>
<b>userinfo_endpoint</b><br/>
Absolute url to the userinfo endpoint e.g: https://idp.authserver.dk/connect/userinfo
<br/><br/>
<b>jwks_uri</b><br/>
Absolute url to the jwks document e.g: https://idp.authserver.dk/.well-known/jwks
<br/><br/>
<b>registration_endpoint</b><br/>
Absolute url to the registration endpoint e.g: https://idp.authserver.dk/connect/register
<br/><br/>
<b>end_session_endpoint</b><br/>
Absolute url of the endsession endpoint e.g: https://idp.authserver.dk/connect/end-session
<br/><br/>
<b>scopes_supported</b><br/>
JSON array of supported scopes defined by the IdP.
For example openid and profile.
Custom scopes used by protected resources are not shown.
<br/><br/>
<b>response_types_supported</b><br/>
JSON array of supported response_type values,
used in the authorize endpoint. "code" is the only supported value.
<br/><br/>
<b>response_modes_supported</b><br/>
JSON array of supported response_mode values,
used in the authorize endpoint. "form_post" is the only supported value.
<br/><br/>
<b>grant_types_supported</b><br/>
JSON array of supported grant_type values,
used in the token endpoint. "authorization_code", "client_credentials" and "refresh_token" are supported.
<br/><br/>
<b>subject_types</b><br/>
JSON array of supported subject identifier types.
"public" is supported.
<br/><br/>
<b>id_token_signing_alg_values_supported</b><br/>
JSON array of supported alg values used for signing id tokens.
"RS256" is supported.
<br/><br/>
<b>token_endpoint_auth_methods_supported</b><br/>
JSON array of supported client authentication methods at the token endpoint.
"client_secret_basic" and "client_secret_post" are supported.
<br/><br/>
<b>token_endpoint_auth_signing_alg_values_supported</b><br/>
JSON array of supported alg methods for signing tokens issued at the token endpoint.
"RS256" is supported.
<br/><br/>
<b>revocation_endpoint</b><br/>
Absolute url to the revocation endpoint e.g: https://idp.authserver.dk/connect/revoke
<br/><br/>
<b>revocation_endpoint_auth_methods_supported</b><br/>
JSON array of supported client authentication methods at the revocation endpoint.
"client_secret_basic" and "client_secret_post" are supported.
<b>introspection_endpoint</b><br/>
Absolute url to the introspection endpoint e.g: https://idp.authserver.dk/connect/introspect
<br/><br/>
<b>introspection_endpoint_auth_methods_supported</b><br/>
JSON array of supported client authentication methods at the introspection endpoint.
"client_secret_basic" and "client_secret_post" are supported.
<br/><br/>
<b>code_challenge_methods_supported</b><br/>
JSON array of supported alg values used for PKCE.
"R256" is supported.
<br/><br/>
<b>claims_supported</b><br/>
JSON array of supported claims, which are found in id tokens and userinfo endpoint.
Claims include name, given_name, family_name, phone, email, address etc.
<br/><br/>
</Section>
<Section title="JWK">
The array always returns three keys.
The first is the newest key which has expired.
This is important for resources which have received a JWT which is signed by an expired key.
The second JWK is the currently used for signing JWT.
The third JWK is the next in line, when the current JWK expires.<br/>
The format of a JWK is as follows:
<pre>{`
{
    "kty": "RSA",
    "use": "sig",
    "kid": 1,
    "alg": "RS256",
    "n": "MODULUS VALUE",
    "e": "AQAB"
}
`}</pre>
</Section>