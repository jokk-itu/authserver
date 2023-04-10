<script>
    import PageTitle from "../../components/PageTitle.svelte";
    import Section from "../../components/Section.svelte";
</script>
<PageTitle title="Discovery Endpoint"/>
<Section title="Introduction">
<p>Discovery is used for relying parties to fetch information
    about the authorization server from a well known uri.
    Information includes endpoints and supported features.
</p>
</Section>
<Section title="Specifications">
<ul>
    <li>
        <a class="ref-link" href="https://datatracker.ietf.org/doc/html/rfc7518">JWA</a>
    </li>
    <li>
        <a href="https://datatracker.ietf.org/doc/html/rfc8414">OAuth Discovery</a>
    </li>
    <li>
        <a href="https://openid.net/specs/openid-connect-discovery-1_0.html">OIDC Discovery</a>
    </li>
</ul>
</Section>
<Section title="Discovery">
<p>
    An endpoint used to provide metadata about the authorization server. It exposes data such as endpoints,
    supported grants and algorithms used for signing tokens.
</p>
</Section>
<Section title="JWK">
<p>
    The array always returns three keys.
    The first is the newest key which has expired.
    This is important for resources which have received a JWT which is signed by an expired key.
    The second JWK is the currently used for signing JWT.
    The third JWK is the next in line, when the current JWK expires.
</p>
<p>The format of a JWK is as follows:</p>
<pre class="pb-4"><code>{`
{
    "kty": "RSA",
    "use": "sig",
    "kid": 1,
    "alg": "RS256",
    "n": "MODULUS VALUE",
    "e": "AQAB"
}
`}</code></pre>
<table class="table-auto">
    <thead>
      <tr>
        <th>Property</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>kty</td>
        <td>Used to identify the type of the JWK</td>
      </tr>
      <tr>
        <td>use</td>
        <td>Used to identity the usage of the JWK</td>
      </tr>
      <tr>
        <td>kid</td>
        <td>Used as an identifier for the JWK</td>
      </tr>
      <tr>
        <td>alg</td>
        <td>Used to identify the used algorithm for signing JWT</td>
      </tr>
      <tr>
        <td>n</td>
        <td>Modulus of the RSA public key. It is Base64 Url encoded</td>
      </tr>
      <tr>
        <td>e</td>
        <td>Exponent of the RSA public key. it is Base64 Url encoded</td>
      </tr>
    </tbody>
  </table>
</Section>