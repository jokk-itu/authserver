<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="Authorize Endpoint"/>
<Section title="Introduction">
<p>The authorize endpoint is used to authenticate an enduser.
It only supports the GET method.</p>
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
            <td>scope</td>
            <td>space delimited string</td>
            <td>Used to assess the usage of the access token. Must include openid.</td>
            <td>openid identityprovider:userinfo weather:write</td>
          </tr>
          <tr>
            <td>id_token_hint</td>
            <td>string</td>
            <td>Used to identity a session and an end user.</td>
            <td>ey</td>
          </tr>
          <tr>
            <td>prompt</td>
            <td>space delimited string</td>
            <td>Used to assess the intention of the request, and can contain multiple values.</td>
            <td>login consent</td>
          </tr>
          <tr>
            <td>client_id</td>
            <td>string</td>
            <td>Used to identify a client.</td>
            <td>8BDB0EC9-32C0-4324-B836-B41538471A8E</td>
          </tr>
          <tr>
            <td>redirect_uri</td>
            <td>string</td>
            <td>Used to redirect the response back to the client.</td>
            <td>https://localhost:5001/callback</td>
          </tr>
          <tr>
            <td>code_challenge</td>
            <td>string</td>
            <td>Used when performing a request with PKCE.</td>
            <td>bairbglabvirevbir</td>
          </tr>
          <tr>
            <td>code_challenge_method</td>
            <td>string</td>
            <td>Used to assess the method when creating the code_challenge.</td>
            <td>S256</td>
          </tr>
          <tr>
            <td>response_type</td>
            <td>string</td>
            <td>Used to assess how to redirect data back to the clint.</td>
            <td>form_post</td>
          </tr>
          <tr>
            <td>nonce</td>
            <td>string</td>
            <td>Used by the client and identity provider to deny reprocessing attacks.</td>
            <td>niasnbviubvisudvb</td>
          </tr>
          <tr>
            <td>max_age</td>
            <td>integer with a minimum value of 0</td>
            <td>Used to assess the maximum age of an authentication in seconds.</td>
            <td>3600</td>
          </tr>
          <tr>
            <td>state</td>
            <td>string</td>
            <td>Used to maintain state between the authorize request and the callback request received from the identity provider.</td>
            <td>nisdbvisdubvisdbvsiuv</td>
          </tr>
        </tbody>
      </table>
</Section>