<script lang="ts">
    import Hashtag from '../components/Hashtag.svelte';
    export let title: String;
</script>

<div class="mb-4 word" id={title.toLowerCase()}>
    <div class="inline-flex">
        <a class="ref-link" href="#{title.toLowerCase()}"><Hashtag /></a>
        <h3 class="text-xl">{title}</h3>
    </div>
    <hr class="mb-4" />
    <slot />
</div>