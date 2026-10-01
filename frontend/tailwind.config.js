/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // ── VM3D brand ──────────────────────────────────────
        brand: {
          purple:      '#9B59B6',
          blue:        '#3498DB',
          cyan:        '#00BCD4',
          'purple-dark': '#7D3C98',
          'cyan-dark':   '#0097A7',
        },

        // ── primary → alias para o gradiente VM3D (compatibilidade) ──
        primary: {
          50:  '#f5f0fb',
          100: '#ede0f7',
          200: '#d9c2ef',
          300: '#c09de4',
          400: '#a872d6',
          500: '#9B59B6',   // roxo VM3D
          600: '#7D3C98',
          700: '#652e7d',
          800: '#4e2361',
          900: '#3a1948',
        },

        // ── dark (modo escuro e UI escura) ──────────────────
        dark: {
          50:  '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          // dark mode surfaces
          bg:      '#0f0f1a',
          surface: '#1a1a2e',
          card:    '#22223b',
          border:  '#2d2d44',
          text:    '#e2e8f0',
          muted:   '#94a3b8',
        },

        // ── superfícies light ────────────────────────────────
        surface: {
          DEFAULT: '#FFFFFF',
          100: '#F8F9FA',
          200: '#F1F3F5',
          300: '#E9ECEF',
          border: '#DEE2E6',
        },
      },

      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },

      backgroundImage: {
        'vm3d':       'linear-gradient(135deg, #9B59B6, #3498DB, #00BCD4)',
        'vm3d-hover': 'linear-gradient(135deg, #7D3C98, #2980B9, #0097A7)',
        'vm3d-text':  'linear-gradient(90deg, #9B59B6, #00BCD4)',
      },

      boxShadow: {
        'card':      '0 2px 8px rgba(0,0,0,0.08)',
        'card-hover':'0 4px 16px rgba(0,0,0,0.12)',
        'card-dark': '0 2px 8px rgba(0,0,0,0.4)',
        'vm3d':      '0 4px 15px rgba(155,89,182,0.35)',
      },

      animation: {
        'slide-in':  'slideIn 0.4s ease-out',
        'fade-in':   'fadeIn 0.3s ease-out',
        'ticker':    'ticker 30s linear infinite',
      },

      keyframes: {
        slideIn:  { from: { transform: 'translateX(100%)', opacity: '0' }, to: { transform: 'translateX(0)', opacity: '1' } },
        fadeIn:   { from: { opacity: '0' }, to: { opacity: '1' } },
        ticker:   { from: { transform: 'translateX(0)' }, to: { transform: 'translateX(-50%)' } },
      },
    },
  },
  plugins: [],
}
