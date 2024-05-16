<script>
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
    import Diagram from "../../../components/Diagram.svelte";
</script>
<PageTitle title="Backchannel Logout"/>
<Section title="Introduction">
<p>
Functionality to send a request to a logout endpoint to all clients where a user has authorized access.
It allows for a single sign out functionality and it is more secure since it occurs through the backchannel.
</p>
</Section>

<Section title="Specifications">
<ul class="list-disc">
<li>
    <a href="https://openid.net/specs/openid-connect-backchannel-1_0.html">Backchannel Logout</a>
</li>
</ul>
</Section>

<Section title="Logout">
<p>The user logging out, will have its session identified.
Then for each active authorizationgrant in that session,
a client will be deduced.<br/>
Each client that has registered a backchannel_logout_uri
during registration, will receive a request.
</p>
<Diagram>
{`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
OpenIDProvider->>RelyingParty: Requests backchannel logout uri
RelyingParty->>OpenIDProvider: Response 200
`}
</Diagram>
The request is sent using the POST method, and parameters
are encoded using application/x-www-for-urlencoded.
<br/>
It will contain a logout_token as a parameter.
The logout_token is a JWT token, which might be encrypted using
the registered method for id_token during client registration.
<br/><br/>
It contains the following claims:
<br/>
<b>iss</b><br/>
Issuer url.<br/><br/>
<b>sub</b>
Subject identifier.<br/><br/>
<b>aud</b><br/>
ClientId.<br/><br/>
<b>iat</b><br/>
Current DateTime.<br/><br/>
<b>jti</b><br/>
Id of token as a Guid.<br/><br/>
<b>events</b><br/>
Is an object with an empty property named http://schemas.openid.net/event/backchannel-logout
<br/><br/>
Example of a request can look like the following:<br/>
<pre>
POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk
Content-Type: application/x-www-form-urlencoded
  
logout_token=eyJhbGci.eyJpc3MiT3BlbklE
</pre>
</Section>