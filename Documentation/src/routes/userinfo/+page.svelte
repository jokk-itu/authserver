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
<Diagram>
{`
sequenceDiagram
participant OpenIDProvider as OP
participant RelyingParty as RP
RelyingParty->>OpenIDProvider: Get request userinfo
OpenIDProvider->>RelyingParty: 200 response userinfo
`}
</Diagram>
</Section>