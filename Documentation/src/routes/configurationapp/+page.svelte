<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="ConfigurationApp"/>
<Section title="ConfigurationApp">
<p>
The console app is built as a tool to provide for quick actions given a command.
</p>
</Section>
<Section title="Commands">
    <b>resource</b> is used to insert resources defined in the app and supposed to be run when creating a fresh database.
    <b>scope</b> is used to insert scopes defined in the app and supposed to be run when creating a fresh database.
    <b>rotate</b> is used to rotate JWK used for signing, it does so by inserting one new JWK in the database.
</Section>