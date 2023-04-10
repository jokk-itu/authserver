<script>
    import Diagram from "../../../components/Diagram.svelte";
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
</script>

<PageTitle title="AuthorizationCode" />
<Section title="Introduction">
    <p>
    The standard grant type and preferred in all use cases when using OIDC.
    In short the client redirects the user to the authorize endpoint,
    after login an authorization code is sent back to the client,
    at last the client requests the token endpoint for tokens.
    </p>
</Section>

<Section title="Specification">
    <ul class="list-disc">
        <li>
            <a href="https://openid.net/specs/openid-connect-core-1_0.html#CodeFlowAuth">OpenId Connect v1.0</a>
        </li>
        <li>
            <a href="https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-05">OAuth 2.1</a>
        </li>
    </ul>
</Section>

<Section title="Authorization Code">
<Diagram>
{`
sequenceDiagram
actor EndUser
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Authorize endpoint
EndUser->>OpenIDProvider: Login
OpenIDProvider->>RelyingParty: Formpost authorization code
RelyingParty->>OpenIDProvider: Post token endpoint
OpenIDProvider->>RelyingParty: Ok 200 with tokens
`}
</Diagram>
</Section>