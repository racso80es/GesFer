/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "#09090b", // zinc-950
        surface: "#18181b", // zinc-900
        border: "#27272a", // zinc-800
        primary: "#f97316", // orange-500
        secondary: "#22c55e", // green-500
      }
    },
  },
  plugins: [],
}
