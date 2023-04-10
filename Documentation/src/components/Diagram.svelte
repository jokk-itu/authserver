<script>
    import { onMount } from 'svelte'
    import mermaid from 'mermaid'
    onMount(async () => {
        await mermaid.run();
    });
</script>

<pre class="mermaid">
    <slot/>
</pre>