<h1 class="text-4xl mb-2">Developer Documentation</h1>
<p class="mb-4">
    Describes how to implement your own custom IdentityProvider using AuthServer.
    <br/>
    How AuthServer functions and its architectural principles.
    <br/>
    The features supported by AuthServer, with integration design.
</p>

<h2 class="text-xl mb-4">Custom IdentityProvider</h2>
<p>TODO</p>
<br/>

<h2 class="text-xl mb-4">Architecture</h2>
<br/>
<h3 class="text-lg mb-4">Request architecture</h3>
<p>Component diagram</p>
<br/>

<h3 class="text-lg mb-4">Client caching</h3>
<p>
    Client data is fetched and utilized at almost every endpoint, and clients are rarely updated.
    Therefore, clients are stored in a configurable distributed cache, to allow for fast retrieval.
    If a client is updated, then upon commit, the cache for that client is flushed.
</p>
<br/>

<h3 class="text-lg mb-4">Telemetry</h3>
<p>The Open Telemetry standard is used to expose data on the system.</p>
<br/>

<h3 class="text-lg mb-4">Datamodel</h3>
<p>ER diagram</p>
<br/>

<h2 class="text-xl mb-4">Authorization Code grant</h2>
<p>Introduction</p>
<p>Authorize request</p>
<p>Authorize response</p>
<p>Token request</p>
<p>Token response</p>


