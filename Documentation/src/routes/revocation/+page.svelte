<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>

<PageTitle title="Revocation" />
<Section title="Introduction">
    <p>
        It is used to revoke a reference accesstoken or a refresh token. It is
        used by clients when it wants to revoke a token, if it should not be
        used any longer.
    </p>
</Section>
<Section title="Specifications">
    <ul class="list-disc">
        <li>
            <a href="https://www.rfc-editor.org/rfc/rfc7009">OAuth Revocation</a
            >
        </li>
    </ul>
</Section>
<Section title="Revocation">
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
                    <td>A token</td>
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
                    <td>Id of the client</td>
                    <td>ec1ff293-1629-4a97-9164-a6e7da4e4b9e</td>
                </tr>
                <tr class="border-b">
                    <td>client_secret</td>
                    <td>string</td>
                    <td>Secret of the client</td>
                    <td>kbogwnbogwne</td>
                </tr>
            </tbody>
        </table>
    </div>
</Section>
