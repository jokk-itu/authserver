<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>

<PageTitle title="Introspection" />
<Section title="Introduction">
Is used to get a structured token, by passing a reference token to the
endpoint. It is used by resources when a client accesses a protected
endpoint, and passes a reference token. It is also used by clients to
check the validity of tokens.
</Section>
<Section title="Specifications">
    <ul class="list-disc">
        <li>
            <a href="https://datatracker.ietf.org/doc/html/rfc7662"
                >OAuth Introspection</a
            >
        </li>
    </ul>
</Section>
<Section title="Introspection">
Requests are sent using the POST method,
and the request body is encoded as form-urlcencoded.
Authentication is also needed for the requester,
and the endpoint can be utilized by both clients
and protected resources. The authentication method must be supported,
and are listed in the discovery endpoint at introspection_endpoint_auth_methods_supported. 
<br/>
The following parameters are allowed:<br/>
<b>token</b><br/>
REQUIRED. A reference token.
<br/><br/>
<b>token_type_hint</b><br/>
OPTIONAL. Can be one of the following values: access_token and refresh_token.
<br/><br/>
The request can look like the following using Basic client authentication:<br/>
<pre>
    POST /connect/introspect HTTP/1.1
    Host: idp.authserver.dk
    Accept: application/json
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=mF9B5f41JqM&token_type_hint=access_token
</pre>
<br/>
The request is validated by authenticating the caller.
If the protected resource or client is unauthenticated,
a statuscode 401 is returned. If the protected resource
or client is requesting a token, which they are not authorized
to request or it is unknown, a successful response is returned stating the token is inactive.
Unrecognized token_type_hints are ignored completely.
<br/>
A successful response with an inactive token can look like the following:
<pre>
    HTTP/1.1 200 OK
    Content-Type: application/json

    {`{
     "active": false
    }`}
</pre>
<br/>
A successful response with an active token can look like the following:
<pre>
    HTTP/1.1 200 OK
    Content-Type: application/json

    {`{
     "active": true,
     "client_id": "adeaf3f2-3459-4c50-89b5-cc1c446525c5",
     "username": "jdoe",
     "jti": "7690c465-2234-4df2-914d-ca42a2001939",
     "token_type": "access_token",
     "scope": "openid profile weather:read",
     "sub": "75e24e31-647a-4011-8a17-106c7fe82db1",
     "aud": "https://weather.authserver.dk",
     "iss": "https://idp.authserver.dk",
     "exp": 1419356238,
     "iat": 1419350238,
     "nbf": 1419350238
    }`}
</pre>
<br/>
An error response for an unauthenticated protected resource or client can look like the following:
<pre>
    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: Bearer error="invalid_token",
                      error_description="The access token expired"
</pre>
</Section>