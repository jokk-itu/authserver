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