<script>
    import "../app.css";
    import Navbar from "../components/Navbar.svelte";
    import Footer from "../components/Footer.svelte";
</script>

<Navbar />
<main class="container mx-auto mt-8">
    <slot />
</main>
<Footer />
