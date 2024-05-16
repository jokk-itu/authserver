<script>
    import PageTitle from "../../../components/PageTitle.svelte";
    import Section from "../../../components/Section.svelte";
</script>
<PageTitle title="Worker"/>
<Section title="Introduction">
<p>
A Client using only OAuth, through the grant type Client Credentials.
It fetches an accesstoken and then periodically requests a protected resource,
with the accesstoken.
</p>
</Section>