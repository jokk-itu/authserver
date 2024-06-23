<script lang="ts">
  import { base } from '$app/paths';
</script>

<!-- flex div -->
<div class="flex flex-row">

    <aside class="sticky pr-8">
        <ul>
            <!-- Client Authentication -->
            <li>
                <a
                    href="{base}/documentation/client-authentication"
                    class="text-lg"
                >
                Client Authentication
                </a>
                <ul class="list-disc list-inside">
                    <li><a href="{base}/documentation/client-authentication/client-secret">Client Secret</a></li>
                    <li><a href="{base}/documentation/client-authentication/client-assertion">Client Assertion</a></li>
                </ul>
            </li>

            <!--Token endpoint-->
            <li>
                <a href="{base}/documentation/token" class="text-lg">
                    Token
                </a>
                <ul class="list-disc list-inside">
                    <li><a href="{base}/documentation/token/authorization-code">Authorization Code</a></li>
                    <li><a href="{base}/documentation/token/refresh-token">Refresh Token</a></li>
                    <li><a href="{base}/documentation/token/client-credentials">Client Credentials</a></li>
                </ul>
            </li>

            <!-- Authorize endpoint -->
            <li>
                <a href="{base}/documentation/authorize" class="text-lg">Authorize</a>
            </li>

            <!-- Introspection endpoint -->
            <li>
                <a href="{base}/documentation/introspection" class="text-lg">Introspection</a>
            </li>

            <!-- Revocation endpoint -->
            <li>
                <a href="{base}/documentation/revocation" class="text-lg">Revocation</a>
            </li>

            <!-- Discovery endpoint -->
            <li>
                <a href="{base}/documentation/discovery" class="text-lg">Discovery</a>
            </li>

            <!-- Registration endpoint -->
            <li>
                <a href="{base}/documentation/registration" class="text-lg">Registration</a>
            </li>

            <!-- Userinfo endpoint -->
            <li>
                <a href="{base}/documentation/userinfo" class="text-lg">Userinfo</a>
            </li>
        </ul>
    </aside>

    <!-- Content (must be growable) -->
    <article>
        <slot />
    </article>
</div>
