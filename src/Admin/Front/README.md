# GesFer Admin Frontend

Frontend administrativo para GesFer, desarrollado con Next.js 14 (App Router), TypeScript y Tailwind CSS.

## Estructura

- `app/`: Rutas y páginas (App Router)
- `components/`: Componentes React reutilizables
- `lib/`: Utilidades y helpers
- `@shared/*`: Componentes compartidos desde `src/Shared/Front`

## Configuración

### Variables de Entorno

- `NEXT_PUBLIC_API_URL`: URL de la API Admin (por defecto: `http://localhost:5001`)

### Alias de Importación

- `@/*`: Apunta a la raíz del proyecto Admin/Front
- `@shared/*`: Apunta a `src/Shared/Front/*`

## Desarrollo

```bash
npm install
npm run dev
```

La aplicación estará disponible en `http://localhost:3001` (o el puerto configurado).
