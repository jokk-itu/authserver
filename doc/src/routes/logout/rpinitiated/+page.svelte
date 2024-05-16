<script>
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
</script>
<PageTitle title="RP Initiated Logout"/>
<Section title="Introduction">
<p>RP Initiated logout is used to initiate logout for the enduser.
It then triggers a session logout through backchannel logout.
</p>
</Section>
<Section title="Specifications">
<ul class="list-disc">
<li>
<a href="https://openid.net/specs/openid-connect-rpinitiated-1_0.html">RP Initiated Logout</a>
</li>
</ul>
</Section>

<Section title="End Session">
An endpoint is exposed at the OP, for the RP to initiate logout.
The endpoint is named end_session_endpoint.

The endpoint supports both GET and POST.
Parameters are serialized as a query string, if the RP uses GET.
Parameters are serialized as form url encoded, if the RP uses POST.

If GET is used, a page is returned allowing the enduser
to logout at the OP as well.

If POST is used, the enduser is logged out at the OP.

The following parameters are recognised:<br/><br/>

<b>id_token_hint</b><br/>
RECOMMENDED. Used to identify the end-user at the OP.
The idToken will be validated on the following parameters:<br/>
iss is the OP.<br/>
exp is ignored.<br/>
sid must correspond to a session in the session table.<br/><br/>

<b>logout_hint</b><br/>
OPTIONAL. Used to identify the enduser.
Useful if the user agent has multiple active sessions.
It can only be a session id.<br/><br/>

<b>client_id</b><br/>
OPTIONAL. Required if providing id_token_hint
or post_logout_redirect_uri.<br/><br/>

<b>post_logout_redirect_uri</b><br/>
OPTIONAL. Used as a uri to redirect to after logout.
<br/><br/>

<b>state</b><br/>
OPTIONAL. Required if the post_logout_redirect_uri is provided.
<br/><br/>
<p>A POST example request can look like the following:</p><br/>
<pre>
POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session
Content-Type: application/x-www-form-urlencoded
      
id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&
logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&
client_id=bd6682a8-aa50-409b-9ae2-68841a356294&
post_logout_redirect_uri=https://webapp.authserver.dk&
state=fkoijbksdkbjfdj
</pre><br/><br/>
<p>A GET example request can look like the following:</p>
<pre>
GET /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session?id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&client_id=bd6682a8-aa50-409b-9ae2-68841a356294&post_logout_redirect_uri=https://webapp.authserver.dk&state=fkoijbksdkbjfdj
</pre>
</Section>