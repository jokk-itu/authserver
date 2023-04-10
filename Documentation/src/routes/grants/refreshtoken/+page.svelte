<script lang="ts">
    import Diagram from "../../../components/Diagram.svelte";
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
</script>

<PageTitle title="Refresh token" />
<Section title="Introduction">
<p>
If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.
</p>
</Section>
<Section title="Specifications">
<ul class="list-disc">
    <li>
        <a href="https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08">OAuth 2.1</a>
    </li>
</ul>
</Section>
<Section title="Refresh token">
<Diagram>
{`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request refresh through token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`}
</Diagram>
</Section>