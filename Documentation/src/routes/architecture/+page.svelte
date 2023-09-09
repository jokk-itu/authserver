<script lang="ts">
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
    import Diagram from "../../components/Diagram.svelte";
</script>
<PageTitle title="Architecture"/>
<Section title="Introduction">
<p>
The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.
</p>
</Section>

<Section title="API">
The API consists of endpoints, where each endpoint builds the received contract,
using the <code>IContextAccessor</code>. The query or command send using Mediator,
is built and then a <code>Response</code> is received.
Mediator consists of a pipeline including a Validator pipe and a Logging pipe.
<br />
The Validator pipe is responsible for invoking all registered validators for the query or command.
The Logging pipe is responsible for logging.
<br/>
When the response is received, it is checked for errors,
and an appropiate HTTP response is finally returned.
</Section>
<Diagram>
    {`
        flowchart LR
        A(IEndpoint)
        B(IMediator)
        C(ValidatorPipeline)
        D(LoggingPipeline)
        E(Handler)
        A --> B --> C --> D --> E
    `}
</Diagram>

<Section title="PKCE">
This is required during authorize grants, also for confidential clients.
code_challenge method must not be plain text.
</Section>

<Section title="Client Configuration">
Secret must be hashed at the OP and only the hash is stored.
Therefore, the secret is not returned during dynamic client registration GET requests.
</Section>

<Section title="UserInfo">
Scope <code>identityprovider:userinfo</code> is required.
It is to limit the authorization of the access token.
</Section>

