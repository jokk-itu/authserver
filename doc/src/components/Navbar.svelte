<script lang="ts">
  import { base } from '$app/paths';
</script>

<nav class="bg-gray-100">
  <div class="max-w-6xl mx-auto px-4">
    <div class="flex justify-between">
      <!-- logo -->
      <div class="flex space-x-4">
        <a
          href="{base}/"
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
          <span class="font-bold hidden md:block">AuthServer</span>
        </a>
      </div>

      <!-- primary nav -->
      <div class="flex items-center space-x-1">
        <a href="{base}/intro" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Intro</a
        >
        <a href="{base}/demo" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Demo</a
        >
        <a href="{base}/developer" class="py-5 px-3 text-gray-700 hover:text-gray-900"
          >Developer</a>
      </div>

      <!-- secondary nav -->
      <div class="flex items-center space-x-1">
        <a href="https://github.com/jokk-itu/authserver" target="_blank" class="py-5 px-3">
          <img src="{base}/github.svg" alt="Github" height="20px" width="20px" />
        </a>
      </div>
    </div>
  </div>
</nav>
