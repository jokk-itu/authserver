<script lang="ts">
    import Diagram from "../../components/Diagram.svelte";
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="UserInfo Endpoint" />
<Section title="Introduction">
<p>An endpoint to get claims about a user.</p>
</Section>
<Section title="Specifications">
<ul class="list-disc">
    <li>
        <a href="https://openid.net/specs/openid-connect-core-1_0.html#UserInfo">OpenId Connect 1.0</a>
    </li>
</ul>
</Section>
<Section title="UserInfo">
The endpoint accepts GET and POST methods.
It requires an Authorization header with an issued access token.
The access token must contain the <code>identityprovider:userinfo</code> scope.
The response is a json body of claims, which are identified
from the sub claim in the access token.
<Diagram>
{`
sequenceDiagram
participant OpenIDProvider as OP
participant RelyingParty as RP
RelyingParty->>OpenIDProvider: Get request userinfo
OpenIDProvider->>RelyingParty: 200 response userinfo
`}
</Diagram>
<br/>
Example of a GET request:<br/>
<pre>
    GET /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
</pre>
<br/>
Example of a POST request:<br/>
<pre>
    POST /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
</pre>
<br/>
Example of a successful response:<br/>
<pre>
    HTTP/1.1 200 OK
    Content-Type: application/json
  
    {`{
     "sub": "248289761001",
     "name": "Jane Doe",
     "given_name": "Jane",
     "family_name": "Doe",
     "email": "janedoe@example.com"
    }`}
</pre>
<br/>
Example of an error response:<br/>
<pre>
    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: error="invalid_token",
      error_description="The Access Token expired"
</pre>
</Section>