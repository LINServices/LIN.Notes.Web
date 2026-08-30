/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["../**/*{html,razor,js,cs}", "D:/LIN/LIN Services/Components/LIN.Notes.Shared/**/*{html,razor,js,cs}"],
    theme: {
        screens: {
            'sm': '640px',
            'md': '768px',
            'dl': '910px',
            'lg': '1024px',
            'xl': '1280px',
            '2xl': '1536px',
        },
        extend: {
            fontFamily: {
                sans: ['Gilroy', 'Gilroy Fallback', 'ui-sans-serif', 'system-ui', 'sans-serif'],
            },
            // Los tokens llegan como canales RGB sueltos desde app.css, por eso van
            // envueltos en rgb(... / <alpha-value>): es lo que habilita los
            // modificadores de opacidad (bg-current-800/40, bg-ink/90, ...).
            colors: {
                canvas: 'rgb(var(--gim-canvas) / <alpha-value>)',
                shell: 'rgb(var(--gim-shell) / <alpha-value>)',
                surface: {
                    DEFAULT: 'rgb(var(--gim-surface) / <alpha-value>)',
                    alt: 'rgb(var(--gim-surface-alt) / <alpha-value>)',
                },
                ink: {
                    DEFAULT: 'rgb(var(--gim-ink) / <alpha-value>)',
                    2: 'rgb(var(--gim-ink-2) / <alpha-value>)',
                },
                muted: 'rgb(var(--gim-muted) / <alpha-value>)',
                line: {
                    DEFAULT: 'rgb(var(--gim-line) / <alpha-value>)',
                    strong: 'rgb(var(--gim-line-strong) / <alpha-value>)',
                },
                track: 'rgb(var(--gim-track) / <alpha-value>)',
                ghost: 'rgb(var(--gim-ghost) / <alpha-value>)',
                danger: {
                    50: 'rgb(var(--gim-danger-50) / <alpha-value>)',
                    100: 'rgb(var(--gim-danger-100) / <alpha-value>)',
                    500: 'rgb(var(--gim-danger-500) / <alpha-value>)',
                    700: 'rgb(var(--gim-danger-700) / <alpha-value>)',
                },
                warn: {
                    50: 'rgb(var(--gim-warn-50) / <alpha-value>)',
                    100: 'rgb(var(--gim-warn-100) / <alpha-value>)',
                    500: 'rgb(var(--gim-warn-500) / <alpha-value>)',
                    700: 'rgb(var(--gim-warn-700) / <alpha-value>)',
                },
                // Nota: 'current' NO define la clave DEFAULT a propósito,
                // para no pisar la utilidad nativa `text-current` (currentColor).
                'current': {
                    '50': 'rgb(var(--gim-current-50) / <alpha-value>)',
                    '100': 'rgb(var(--gim-current-100) / <alpha-value>)',
                    '200': 'rgb(var(--gim-current-200) / <alpha-value>)',
                    '300': 'rgb(var(--gim-current-300) / <alpha-value>)',
                    '400': 'rgb(var(--gim-current-400) / <alpha-value>)',
                    '500': 'rgb(var(--gim-current-500) / <alpha-value>)',
                    '600': 'rgb(var(--gim-current-600) / <alpha-value>)',
                    '700': 'rgb(var(--gim-current-700) / <alpha-value>)',
                    '800': 'rgb(var(--gim-current-800) / <alpha-value>)',
                    '900': 'rgb(var(--gim-current-900) / <alpha-value>)',
                    '950': 'rgb(var(--gim-current-950) / <alpha-value>)',
                },
            },
            fontSize: {
                display: ['4.5rem', { lineHeight: '0.95', letterSpacing: '-0.03em', fontWeight: '500' }],
                'metric-xl': ['2.75rem', { lineHeight: '1', letterSpacing: '-0.02em', fontWeight: '500' }],
                'metric-lg': ['2.125rem', { lineHeight: '1', letterSpacing: '-0.02em', fontWeight: '500' }],
                heading: ['1.25rem', { lineHeight: '1.2', letterSpacing: '-0.01em', fontWeight: '500' }],
                title: ['0.875rem', { lineHeight: '1.3', fontWeight: '500' }],
                body: ['0.8125rem', { lineHeight: '1.5' }],
                value: ['0.8125rem', { lineHeight: '1.2', fontWeight: '500' }],
                label: ['0.6875rem', { lineHeight: '1.2' }],
            },
            boxShadow: {
                float: '0 1px 2px rgb(0 0 0 / 0.04)',
                overlay: '0 24px 60px -20px rgb(0 0 0 / 0.18)',
            },
            transitionTimingFunction: {
                brand: 'cubic-bezier(0.2, 0, 0, 1)',
            },
        },
    },
    plugins: []
}