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
<p>
    The API consists of endpoints, where each endpoint builds the received contract,
    using the <code>IContextAccessor</code>. The query or command send using Mediator,
    is built and then a <code>Response</code> is received.
    Mediator consists of a pipeline including a Validator pipe and a Logging pipe.
</p>
<br />
<p>
    The Validator pipe is responsible for invoking all registered validators for the query or command.
    The Logging pipe is responsible for logging.
</p>
<br/>
<p>
    When the response is received, it is checked for errors,
    and an appropiate HTTP response is finally returned.
</p>
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