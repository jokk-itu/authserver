<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="Token Endpoint"/>
<Section title="Introduction">
The token endpoint is used by the OP to issue access, refresh and id tokens to RPs by redeeming grants.
</Section>
<Section title="Specifications">
<ul class="list-disc">
    <li>
        <a href="https://openid.net/specs/openid-connect-core-1_0.html">OIDC 1.0</a>
    </li>
    <li>
        <a href="https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-09">OAuth 2.1</a> 
    </li>
</ul>
</Section>

<Section title="Token">
Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using the authentication method registered in the token_endpoint_auth_method.<br/>
The following parameters are allowed:<br/>
<b>grant_type</b><br/>
REQUIRED. The grant being redeemed for token(s).
The supported grants can be found at the discovery endpoint under grant_types.
<br/><br/>
<b>code</b><br/>
REQUIRED. If the grant_type is authorization_code.
It is the authorization_code issued by the OP.
<br/><br/>
<b>client_id</b><br/>
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authentication method.
<br/><br/>
<b>client_secret</b><br/>
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authenticatino method.
<br/><br/>
<b>redirect_uri</b><br/>
REQUIRED. If the grant_type is authorization_code
and the authorize request included a redirect_uri parameter.
If included, it must match a registered redirect_uri,
and it must match the same redirect_uri given at the authorize endpoint, if one was provided.
<br/><br/>
<b>scope</b><br/>
REQUIRED. If the grant_type is client_credentials.
It can be useful during refresh_token grant,
if the access token should only contain a subset of the authorized scope in the initial grant.
If not provided, the consented scope from the grant is used, as long as it is registered at a resource identified by the resource parameter.
<br/><br/>
<b>code_verifier</b><br/>
REQUIRED. If the grant_type is authorization_code.
It will be validated against the code, since it contains the code_challenge from the authorize request.
It must be between 43 and 128 characters long.
<br/><br/>
<b>refresh_token</b><br/>
REQUIRED. If the grant_type is refresh_token.
It will be used and then invalidated. A new refresh_token is returned in the response.
<br/><br/>
<b>resource</b><br/>
REQUIRED. It will be used as the audience of the access_token returned in the response.
Multiple resource parameters can be given and each must be a resource Uri registered at the OP.
Any resource must also expect at least one provided scope.
<br/><br/>
The request for authorization_code grant can look like the following:
<pre>
    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=authorization_code
    &code=fgukaoirnenvsoidnv
    &redirect_uri=https://webapp.authserver.dk/signin-callback
    &code_verifier=saeoginsoivn...
    &resource=https://weather.authserver.dk
</pre><br/><br/>
The request for refresh_token grant can look like the following:
<pre>
    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=refresh_token
    &refresh_token=fgukaoirnenvsoidnv
    &scope=openid%20identityprovider:userinfo
</pre><br/><br/>
The response for authorization_code and refresh_token grant can look like the following:
<pre>
    HTTP/1.1 200 OK
    Content-Type: application/json

    {`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600,
     "refresh_token": "lauribvidbvdfv";
     "id_token": "aleryubvksjdv",
     "scope": "openid identityprovider:userinfo"
    }`}
</pre><br/><br/>
The request for client_credentials grant can look like the following:
<pre>
    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=client_credentials&scope=weather%3Aread
</pre><br/><br/>
The response for client_credentials grant can look like the following:
<pre>
    HTTP/1.1 200 OK
    Content-Type: application/json

    {`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600,
     "scope": "weather:read"
    }`}
</pre>
</Section>