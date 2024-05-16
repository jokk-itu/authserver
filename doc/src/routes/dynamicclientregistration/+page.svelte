<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
    import Diagram from "../../components/Diagram.svelte";
</script>

<PageTitle title="Dynamic Client Registration" />
<Section title="Introduction">
<p>Used to create, get, update and delete clients.</p>
</Section>

<Section title="Specifications">
    <ul class="list-disc">
        <li>
            <a href="https://openid.net/specs/openid-connect-registration-1_0.html">Dynamic Client Registration OIDC</a>
        </li>
        <li>
            <a href="https://datatracker.ietf.org/doc/html/rfc7592">Dynamic Client Registration Management OAuth</a>
        </li>
        <li>
            <a href="https://datatracker.ietf.org/doc/html/rfc7591">Dynamic Client Registration OAuth</a>
        </li>
    </ul>
</Section>

<Section title="Create a Client">
<Diagram>
{`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Post client registration endpoint
OpenIDProvider->>EndUser: Created 201 with client information
`}
</Diagram>
</Section>
<br/>
<Section title="Update a Client">
<Diagram>
{`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Put client configuration endpoint
OpenIDProvider->>EndUser: Ok 200 with client information
`}
</Diagram>
</Section>
<br/>
<Section title="Get a Client">
<Diagram>
{`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Get client endpoint with registration access token
OpenIDProvider->>EndUser: Ok 200 with client information
`}
</Diagram>
</Section>
<br/>
<Section title="Deleting a Client">
<Diagram>
{`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Delete client endpoint with registration access token
OpenIDProvider->>EndUser: No Content 204
`}
</Diagram>
</Section>