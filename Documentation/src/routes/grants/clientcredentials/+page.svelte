<script>
    import Diagram from "../../../components/Diagram.svelte";
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
</script>
<PageTitle title="Client Credentials" />
<Section title="Introduction">
    <p>
        Allows confidential clients which do not use resource owners,
        to allow themselves authorization without interaction,
        by simply requesting the token endpoint for access tokens using client credentials.
    </p>
</Section>

<Section title="Specifications">
   <ul class="list-disc">
        <li>
            <a href="https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08">OAuth 2.1</a>
        </li>
   </ul>
</Section>

<Section title="Client Credentials">
<Diagram>
{`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`}
</Diagram>
</Section>