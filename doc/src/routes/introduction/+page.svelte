<h1 class="text-4xl mb-2">What is Authserver?</h1>
<p class="mb-4">
    An IdentityProvider implementing the core OAuth 2.1 specification, with the
    OpenId Connect specification on top.
    <br />
    The only thing AuthServer does not have is UI and user persistance, it is recommended
    to use AspNetCore.Identity for that.
    <br />
    Everything else has already been made to make auth(orization) and auth(entication)
    easy.
    <br /><br />
    Authserver has been designed to be distributed with caching, configurable without
    sacrificing security and implements strict security by default, so that recommendations
    become requirements.
</p>

<h2 class="text-xl mb-4">What does it do for you?</h2>

<div class="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
    <!-- Token -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M16.5 6v.75m0 3v.75m0 3v.75m0 3V18m-9-5.25h5.25M7.5 15h3M3.375 5.25c-.621 0-1.125.504-1.125 1.125v3.026a2.999 2.999 0 0 1 0 5.198v3.026c0 .621.504 1.125 1.125 1.125h17.25c.621 0 1.125-.504 1.125-1.125v-3.026a2.999 2.999 0 0 1 0-5.198V6.375c0-.621-.504-1.125-1.125-1.125H3.375Z"
            />
        </svg>
        <div class="font-bold text-xl mb-2">Tokens</div>
        <p class="text-gray-800 text-base">
            Choose typed JWT with signatures and optionally encryption and
            sender constraints. Or use Opaque tokens with Introspection.
        </p>
    </div>

    <!-- Client -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M9 17.25v1.007a3 3 0 0 1-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0 1 15 18.257V17.25m6-12V15a2.25 2.25 0 0 1-2.25 2.25H5.25A2.25 2.25 0 0 1 3 15V5.25m18 0A2.25 2.25 0 0 0 18.75 3H5.25A2.25 2.25 0 0 0 3 5.25m18 0V12a2.25 2.25 0 0 1-2.25 2.25H5.25A2.25 2.25 0 0 1 3 12V5.25"
            />
        </svg>
        <div class="font-bold text-xl mb-2">Clients</div>
        <p class="text-gray-800 text-base">
            Setup clients dynamically using Dynamic Client Registration or have
            them statically defined.
        </p>
    </div>

    <!-- Entity Framework Core -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375m16.5 0v3.75m-16.5-3.75v3.75m16.5 0v3.75C20.25 16.153 16.556 18 12 18s-8.25-1.847-8.25-4.125v-3.75m16.5 0c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125"
            />
        </svg>
        <div class="font-bold text-xl mb-2">Entity Framework Core</div>
        <p class="text-gray-800 text-base">
            Authserver uses EF Core for persistance and uses distributed caching
            for fast retrieval of common entities without stale entries.
        </p>
    </div>

    <!-- Distributed -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M5.25 14.25h13.5m-13.5 0a3 3 0 0 1-3-3m3 3a3 3 0 1 0 0 6h13.5a3 3 0 1 0 0-6m-16.5-3a3 3 0 0 1 3-3h13.5a3 3 0 0 1 3 3m-19.5 0a4.5 4.5 0 0 1 .9-2.7L5.737 5.1a3.375 3.375 0 0 1 2.7-1.35h7.126c1.062 0 2.062.5 2.7 1.35l2.587 3.45a4.5 4.5 0 0 1 .9 2.7m0 0a3 3 0 0 1-3 3m0 3h.008v.008h-.008v-.008Zm0-6h.008v.008h-.008v-.008Zm-3 6h.008v.008h-.008v-.008Zm0-6h.008v.008h-.008v-.008Z"
            />
        </svg>
        <div class="font-bold text-xl mb-2">Distributed</div>
        <p class="text-gray-800 text-base">
            Designed to be a distributed application with multiple instances,
            enabling scalability for your specific needs.
        </p>
    </div>

    <!-- Dependency Injection -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="m21 7.5-9-5.25L3 7.5m18 0-9 5.25m9-5.25v9l-9 5.25M3 7.5l9 5.25M3 7.5v9l9 5.25m0-9v9"
            />
        </svg>

        <div class="font-bold text-xl mb-2">Dependency Injection</div>
        <p class="text-gray-800 text-base">
            Easily setup your custom Authserver using depdency injection in your
            AspNet Core application.
        </p>
    </div>

    <!-- Identity -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 0 0 2.25-2.25V6.75A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25v10.5A2.25 2.25 0 0 0 4.5 19.5Zm6-10.125a1.875 1.875 0 1 1-3.75 0 1.875 1.875 0 0 1 3.75 0Zm1.294 6.336a6.721 6.721 0 0 1-3.17.789 6.721 6.721 0 0 1-3.168-.789 3.376 3.376 0 0 1 6.338 0Z"
            />
        </svg>

        <div class="font-bold text-xl mb-2">Identity</div>
        <p class="text-gray-800 text-base">
            Usage of claims in userinfo and id tokens. Identifies users by
            subject identifiers of type public and pairwise.
        </p>
    </div>

    <!-- Discovery -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15.75 5.25a3 3 0 0 1 3 3m3 0a6 6 0 0 1-7.029 5.912c-.563-.097-1.159.026-1.563.43L10.5 17.25H8.25v2.25H6v2.25H2.25v-2.818c0-.597.237-1.17.659-1.591l6.499-6.499c.404-.404.527-1 .43-1.563A6 6 0 1 1 21.75 8.25Z"
            />
        </svg>

        <div class="font-bold text-xl mb-2">Discovery</div>
        <p class="text-gray-800 text-base">
            Identify endpoints and supported features through the discovery
            endpoint, and identify signature and encryption keys through the
            Jwks endpoint.
        </p>
    </div>

    <!-- Open Telemetry -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M9.75 3.104v5.714a2.25 2.25 0 0 1-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 0 1 4.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0 1 12 15a9.065 9.065 0 0 0-6.23-.693L5 14.5m14.8.8 1.402 1.402c1.232 1.232.65 3.318-1.067 3.611A48.309 48.309 0 0 1 12 21c-2.773 0-5.491-.235-8.135-.687-1.718-.293-2.3-2.379-1.067-3.61L5 14.5"
            />
        </svg>

        <div class="font-bold text-xl mb-2">Open Telemetry</div>
        <p class="text-gray-800 text-base">
            Observe usage of your application through metrics, traces and logs.
        </p>
    </div>

    <!-- OAuth 2.1 -->
    <div class="max-w-sm rounded overflow-hidden shadow-lg px-6 py-4">
        <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="#000000"
            viewBox="0 0 1034 1034"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-6 mb-2"
        >
            <path
                d="M504 228q-16 0 -33 1q-33 2 -70 11q-35 9 -60 20q-38 17 -77 44q-44 31 -78 75t-55 96l-5 12q-9 22 -12 33q-13 45 -13 98q0 49 9 93q22 100 82 171q52 62 139 108q48 25 109.5 33.5t124.5 -1t115 -35.5q90 -46 152 -139q31 -46 48 -100q19 -60 19 -124q0 -57 -21 -119
             q-17 -52 -46 -97q-51 -81 -128 -127q-88 -53 -200 -53zM470 418h54q16 0 29 9.5t18 24.5l106 320q7 20 -2.5 38.5t-28.5 24.5q-8 2 -16 2q-16 0 -29 -9t-18 -25l-23 -73h-120l-23 73q-5 15 -18 24.5t-29 9.5q-8 0 -15 -2q-20 -6 -29.5 -24t-3.5 -38l101 -320q5 -16 18 -25.5
             t29 -9.5z"
            />
        </svg>

        <div class="font-bold text-xl mb-2">OAuth 2.1</div>
        <p class="text-gray-800 text-base">
            Implements OAuth 2.1 and OpenId Connect.
        </p>
    </div>
</div>