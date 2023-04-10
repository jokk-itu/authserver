<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="Token Endpoint"/>
<Section title="Introduction">

</Section>
<Section title="Specifications">
<ul class="list-disc">
    <li>
        <a href="https://openid.net/specs/openid-connect-core-1_0.html">OIDC 1.0</a>
    </li>
    <li>
        <a href="https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08">OAuth 2.1</a> 
    </li>
</ul>
</Section>

<Section title="Request">
    <table class="table-auto">
        <thead>
            <tr>
                <th>Parameter</th>
                <th>Data type</th>
                <th>Description</th>
                <th>Example</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>grant_type</td>
                <td>string</td>
                <td>Used to assess the method to grant authorization.</td>
                <td>authorization_code</td>
              </tr>
              <tr>
                <td>code</td>
                <td>string</td>
                <td>Used as the authorization_code return in the callback from the authorize endpoint.</td>
                <td>labwuigwbweogbnoiweg</td>
              </tr>
              <tr>
                <td>client_id</td>
                <td>string</td>
                <td>Used to identify a client.</td>
                <td>8BDB0EC9-32C0-4324-B836-B41538471A8E</td>
              </tr>
              <tr>
                <td>client_secret</td>
                <td>string</td>
                <td>Used to authenticate a client.</td>
                <td>sdbvisdbvsiudbvisvbsd</td>
              </tr>
              <tr>
                <td>redirect_uri</td>
                <td>string</td>
                <td>Used to match the redirect_uri in the authorize request.</td>
                <td>https://localhost:5001/callback</td>
              </tr>
              <tr>
                <td>scope</td>
                <td>space delimited string</td>
                <td>Used to assess the usage of the access token. Must include openid.</td>
                <td>openid identityprovider:userinfo weather:write</td>
              </tr>
              <tr>
                <td>code_verifier</td>
                <td>string</td>
                <td>Used to verify the code_challenge was generated from the code_verifier.</td>
                <td>isubvisvubiusebvsiev</td>
              </tr>
              <tr>
                <td>refresh_token</td>
                <td>string</td>
                <td>Used to get a new access_token in the refresh_token grant flow.</td>
                <td>ey</td>
              </tr>
        </tbody>
    </table>
</Section>