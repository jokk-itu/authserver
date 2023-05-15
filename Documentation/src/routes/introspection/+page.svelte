<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>

<PageTitle title="Introspection" />
<Section title="Introduction">
    <p>
        Is used to get a structured token, by passing a reference token to the
        endpoint. It is used by resources when a client accesses a protected
        endpoint, and passes a reference token. It is also used by clients to
        check the validity of tokens.
    </p>
</Section>
<Section title="Specifications">
    <ul class="list-disc">
        <li>
            <a href="https://datatracker.ietf.org/doc/html/rfc7662"
                >OAuth Introspection</a
            >
        </li>
    </ul>
</Section>
<Section title="Introspection">
    <div class="relative overflow-x-auto">
        <table>
            <thead>
                <tr>
                    <th>Parameter</th>
                    <th>Data type</th>
                    <th>Description</th>
                    <th>Example</th>
                </tr>
            </thead>
            <tbody>
                <tr class="border-b">
                    <td>token</td>
                    <td>string</td>
                    <td>A reference token</td>
                    <td>wæonvowbvobewvowe</td>
                </tr>
                <tr class="border-b">
                    <td>token_type_hint</td>
                    <td>string</td>
                    <td
                        >A hint of the token. Can be access_token or
                        refresh_token.</td
                    >
                    <td>access_token</td>
                </tr>
                <tr class="border-b">
                    <td>client_id</td>
                    <td>string</td>
                    <td>Id of the client or resource</td>
                    <td>ec1ff293-1629-4a97-9164-a6e7da4e4b9e</td>
                </tr>
                <tr class="border-b">
                    <td>client_secret</td>
                    <td>string</td>
                    <td>Secret of the client or resource</td>
                    <td>kbogwnbogwne</td>
                </tr>
            </tbody>
        </table>
    </div>
</Section>
