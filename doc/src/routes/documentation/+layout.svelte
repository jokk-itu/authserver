<!-- flex div -->
<div class="flex flex-row">

    <aside class="sticky pr-8">
        <ul>
            <!-- Client Authentication -->
            <li>
                <a
                    href="documentation/token"
                    class="text-lg"
                >
                Client Authentication
                </a>
                <ul class="list-disc list-inside">
                    <li>Client Secret</li>
                    <li>Client Assertion</li>
                </ul>
            </li>

            <!--Token endpoint-->
            <li>
                <a href="documentation/token" class="text-lg">
                    Token
                </a>
                <ul class="list-disc list-inside">
                    <li>Authorization Code</li>
                    <li>Refresh Token</li>
                    <li>Client Credentials</li>
                </ul>
            </li>

            <!-- Authorize endpoint -->
            <li>
                <a href="documentation/authorize" class="text-lg">Authorize</a>
            </li>

            <!-- Introspection endpoint -->
            <li>
                <a href="documentation/introspection" class="text-lg">Introspection</a>
            </li>

            <!-- Revocation endpoint -->
            <li>
                <a href="documentation/revocation" class="text-lg">Revocation</a>
            </li>

            <!-- Discovery endpoint -->
            <li>
                <a href="documentation/discovery" class="text-lg">Discovery</a>
            </li>

            <!-- Registration endpoint -->
            <li>
                <a href="documentation/registration" class="text-lg">Registration</a>
            </li>

            <!-- Userinfo endpoint -->
            <li>
                <a href="documentation/userinfo" class="text-lg">Userinfo</a>
            </li>
        </ul>
    </aside>

    <!-- Content (must be growable) -->
    <article>
        <slot />
    </article>
</div>
