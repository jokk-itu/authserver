<script lang=ts>
    import { onMount } from 'svelte'
    import mermaid from 'mermaid'
    let id = makeId();
    onMount(async () => {
        await mermaid.run({
            nodes: [document.getElementById(id)!],
        });
    });

    function makeId() {
        const length = 32;
        let result = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        const charactersLength = characters.length;
        let counter = 0;
        while (counter < length) {
            result += characters.charAt(Math.floor(Math.random() * charactersLength));
            counter += 1;
        }
        return result;
    }
</script>

<pre id={id}>
    <slot/>
</pre>