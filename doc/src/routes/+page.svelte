<script lang="ts">
    import Terminal from "../components/Terminal.svelte";
</script>

<div class="flex flex-col sm:flex-row sm:justify-evenly">
    <div>
        <h1 class="text-4xl mb-2">Authorization made easy</h1>
        <h2 class="text-xl mb-4">
            AuthServer is built for easy setup with strict security requirements
        </h2>
        <ul class="pl-5 list-disc">
            <li class="py-2">OAuth 2.1 ready</li>
            <li class="py-2">Easily setup an IdentityProvider</li>
            <li class="py-2">Fast, reliable and scalable</li>
            <li class="py-2">Monitor capability using Open Telemetry</li>
        </ul>
        <button class="rounded bg-openid my-10 px-8 py-3">Learn more</button>
    </div>
    <div>
        <Terminal />
    </div>
</div>