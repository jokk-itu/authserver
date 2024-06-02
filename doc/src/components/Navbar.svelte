<script lang="ts">
  let isMobileMenuInactive: boolean = true;
</script>

<nav class="bg-gray-100">
  <div class="max-w-6xl mx-auto px-4">
    <div class="flex justify-between">
      <!-- logo -->
      <div class="flex space-x-4">
        <a
          href="/"
          class="flex items-center py-5 px-2 text-gray-700 hover:text-gray-900"
        >
          <svg
            class="h-6 w-6 mr-1 fill-openid"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M14.54 .887L10.91 2.66V20.83C6.76 20.31 3.64 18.05 3.64 15.33C3.64 12.75 6.44 10.58 10.27 9.92V7.61C4.42 8.32 0 11.5 0 15.33C0 19.29 4.74 22.57 10.91 23.11L14.54 21.4V.886M15.18 7.61V9.92C16.61 10.17 17.89 10.62 18.94 11.23L16.97 12.34L24 13.87L23.5 8.66L21.63 9.72C19.89 8.66 17.67 7.91 15.18 7.61Z"
            />
          </svg>
          <span class="font-bold">Authserver</span>
        </a>
      </div>

      <!-- primary nav -->
      <div class="hidden md:flex items-center space-x-1">
        <a href="/introduction" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Introduction</a
        >
        <a href="/quick-start" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Quick Start</a
        >
        <a href="/documentation" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Documentation</a
        >
      </div>

      <!-- secondary nav -->
      <div class="hidden md:flex items-center space-x-1">
        <a href="https://github.com/jokk-itu/authserver" target="_blank" class="py-5 px-3">
          <img src="/github.svg" alt="Github" height="20px" width="20px" />
        </a>
      </div>

      <!-- mobile button goes here -->
      <div class="md:hidden flex items-center">
        <button
          type="button"
          data-fc-target="mobile-menu"
          data-fc-type="offcanvas"
        >
          <svg
            class="w-6 h-6"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>
        </button>
      </div>
    </div>
  </div>

  <!-- mobile menu -->
  <div
    id="mobile-menu"
    class="fc-offcanvas-open:translate-x-0 hidden -translate-x-full fixed top-0 left-0 transition-all duration-300 transform h-full max-w-xs w-full z-50 bg-white border-r dark:bg-gray-800 dark:border-gray-700"
    tabindex="-1"
  >
    <div
      class="flex justify-between items-center py-2 px-4 border-b dark:border-gray-700"
    >
      <h3 class="font-medium">Authserver</h3>
      <button
        class="inline-flex flex-shrink-0 justify-center items-center h-8 w-8 rounded-md text-gray-500 hover:text-gray-700 text-sm dark:text-gray-500 dark:hover:text-gray-400"
        data-fc-dismiss
        type="button"
      >
        <span class="material-symbols-rounded">close</span>
      </button>
    </div>
    <div class="flex items-center space-x-1">
      <a href="/" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Introduction</a
        >
        <a href="/" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Quickstarts</a
        >
        <a href="/" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Documentation</a
        >
    </div>
  </div>
</nav>
