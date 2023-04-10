/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,js,svelte,ts}'],
  theme: {
    extend: {
      flexGrow: {
        3: 3
      }
    },
  },
  plugins: [],
}
