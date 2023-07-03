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
<Diagram>
{`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
OpenIDProvider->>RelyingParty: Requests backchannel logout uri
RelyingParty->>OpenIDProvider: Response 200
`}
</Diagram>
</Section>