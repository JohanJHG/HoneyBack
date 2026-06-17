# Product

## Register

product

## Users

Individuos latinoamericanos (mercado principal: Colombia) entre 22–38 años que gestionan sus finanzas personales y quieren claridad real sobre sus gastos, ahorros y deudas. Usan la app en el escritorio después del trabajo o en sesiones nocturnas, en un estado mental de revisión tranquila, no urgente. Nivel de alfabetización financiera: intermedio — entienden conceptos pero quieren que la app haga el trabajo duro.

## Product Purpose

HoneyBalance es un dashboard de finanzas personales que responde tres preguntas en segundos: ¿cuánto tengo?, ¿cuánto gané?, ¿en qué gasté? El éxito es que el usuario abra el dashboard, entienda su situación financiera de un vistazo, y salga con una decisión clara. No es un tracker de hábitos ni un coach. Es claridad financiera, entregada con calidez.

## Brand Personality

Cálido, preciso, tranquilo. Como un contador de confianza que también tiene buen gusto.

## Anti-references

- Mint.com / interfaces de banca tradicional: azul corporativo, tablas densas, sensación de obligación.
- Notion finance templates: demasiado burocrático, muchas columnas, sin calidez.
- SaaS cream backgrounds genéricos: el beige/sand "editorial" que todo agente produce por defecto.
- Gradients en texto y glassmorphism decorativo: reduce credibilidad financiera.
- Dashboards que parecen hechos con AI: grids de cards idénticos con icon+heading+text, eyebrows en cada sección, border-radius > 24px en cards.

## Design Principles

1. **Claridad sin esfuerzo.** El usuario no debería tener que buscar su situación financiera — debería aparecer en su cara.
2. **La calidez es sistémica, no decorativa.** El honey-amber (#FFD8A9) no es decoración; es el hilo que conecta el estado activo, los bordes, los acentos y los estados de foco en toda la app.
3. **Negro puro como espacio.** El fondo #000000 no es "dark mode por moda" — es el espacio en blanco del producto. Las cifras y el contenido flotan sobre él con peso.
4. **Datos primero, chrome mínimo.** Cualquier elemento UI que no ayude al usuario a entender su dinero es candidato a eliminación.
5. **Confianza por consistencia.** El sistema de tokens (--hb-*) existe para que el usuario nunca sienta que está en una app diferente entre secciones.

## Accessibility & Inclusion

- WCAG AA como piso. Texto de cuerpo: contraste mínimo 4.5:1.
- Soporte completo de `prefers-reduced-motion`: todas las animaciones tienen fallback de crossfade instantáneo.
- Sistema dual de temas (dark / light) ya implementado con tokens --hb-*.
- Formularios: labels siempre visibles (nunca placeholder-only).
