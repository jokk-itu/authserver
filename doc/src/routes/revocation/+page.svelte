<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>

<PageTitle title="Revocation" />
<Section title="Introduction">
It is used to revoke a reference accesstoken or a refresh token. It is
used by clients when it wants to revoke a token, if it should not be
used any longer.
</Section>
<Section title="Specifications">
    <ul class="list-disc">
        <li>
            <a href="https://www.rfc-editor.org/rfc/rfc7009">OAuth Revocation</a
            >
        </li>
    </ul>
</Section>
<Section title="Revocation">
Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using an authentication method which is supported,
at the discovery endpoint atrevocation_endpoint_auth_methods_supported.<br/>
Access tokens can only be revoked if it is a reference token,
and Refresh tokens can always be revoked.<br/>
The following parameters are allowed:<br/>
<b>token</b><br/>
REQUIRED. The token which should be revoked.
<br/><br/>
<b>token_type_hint</b><br/>
OPTIONAL. The type of token, the client wants revoked.
The allowed values are: access_token and refresh_token
<br/><br/>
The request can look like the following using Basic client authentication:<br/>
<pre>
    POST /connect/revoke HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=45ghiukldjahdnhzdauz&token_type_hint=refresh_token
</pre><br/><br/>
If the requested token is successfully revoked or invalid,
the response contains a status code 200.<br/>
Invalid token_type_hints are ignored completetly.<br/>
If client authentication fails, an invalid_client error is returned,
with status code 400.
</Section>
