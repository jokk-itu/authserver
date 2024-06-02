<script>
    import "../app.css";
    import Navbar from "../components/Navbar.svelte";
    import Footer from "../components/Footer.svelte";
</script>

<Navbar />
<div class="mt-8 mx-6 flex flex-col sm:flex-row sm:justify-evenly">
    <slot />
</div>
<Footer />
