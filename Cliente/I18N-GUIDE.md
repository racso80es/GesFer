# Guía de Internacionalización (i18n)

## Estructura de Páginas

**IMPORTANTE**: Todas las páginas nuevas deben crearse dentro de la carpeta `app/[locale]/` para que funcionen correctamente con el sistema de internacionalización.

### Estructura Correcta

```
app/
  [locale]/
    layout.tsx          # Layout principal con soporte i18n
    page.tsx            # Página raíz
    login/
      page.tsx          # Página de login
    dashboard/
      page.tsx          # Página de dashboard
    empresas/
      page.tsx          # Lista de empresas
      [id]/
        page.tsx        # Detalle de empresa
    # ... más páginas
```

### ❌ Estructura Incorrecta

```
app/
  login/
    page.tsx            # ❌ NO crear aquí
  dashboard/
    page.tsx            # ❌ NO crear aquí
```

## Crear una Nueva Página

### 1. Crear el archivo en `app/[locale]/`

Ejemplo: Para crear una página de "productos", crear:
```
app/[locale]/productos/page.tsx
```

### 2. Usar traducciones en componentes

#### En componentes cliente:

```tsx
"use client";

import { useTranslations } from 'next-intl';

export default function MiPagina() {
  const t = useTranslations('common'); // o el namespace que necesites
  
  return (
    <div>
      <h1>{t('welcome')}</h1>
      <button>{t('save')}</button>
    </div>
  );
}
```

#### En componentes servidor:

```tsx
import { useTranslations } from 'next-intl/server';

export default async function MiPagina() {
  const t = await useTranslations('common');
  
  return (
    <div>
      <h1>{t('welcome')}</h1>
    </div>
  );
}
```

### 3. Parámetros dinámicos

Si tu página tiene parámetros dinámicos (como `[id]`), asegúrate de incluir `locale` en el tipo:

```tsx
export default function MiPagina({
  params,
}: {
  params: Promise<{ id: string; locale: string }> | { id: string; locale: string };
}) {
  // Tu código aquí
}
```

### 4. Navegación

Las rutas funcionan automáticamente con el locale. El middleware se encarga de agregar el locale a la URL.

```tsx
import { useRouter } from "next/navigation";

const router = useRouter();
router.push("/dashboard"); // Funciona automáticamente con el locale actual
```

## Agregar Nuevas Traducciones

### 1. Editar los archivos de mensajes

Edita los archivos en `messages/`:
- `messages/es.json` - Español
- `messages/en.json` - Inglés
- `messages/ca.json` - Catalán

### 2. Estructura de mensajes

```json
{
  "common": {
    "welcome": "Bienvenido",
    "save": "Guardar"
  },
  "miModulo": {
    "titulo": "Mi Título",
    "descripcion": "Mi Descripción"
  }
}
```

### 3. Usar en componentes

```tsx
const t = useTranslations('miModulo');
t('titulo'); // "Mi Título"
```

## Detección de Idioma

El sistema detecta automáticamente el idioma del usuario basándose en:
1. `userLanguageId` (idioma del usuario)
2. `companyLanguageId` (idioma de la empresa)
3. `countryLanguageId` (idioma del país)
4. `effectiveLanguageId` (idioma efectivo)
5. **Español (por defecto)** si ninguno está configurado

## Idiomas Soportados

- `es` - Español (idioma por defecto)
- `en` - Inglés
- `ca` - Catalán

## Notas Importantes

1. **Siempre crear páginas en `app/[locale]/`** - No crear páginas directamente en `app/`
2. **Usar `useTranslations`** para todos los textos que se muestran al usuario
3. **Las rutas funcionan automáticamente** - No es necesario agregar el locale manualmente en las rutas
4. **El middleware maneja las redirecciones** - Se encarga de agregar el locale correcto a las URLs

## Ejemplo Completo

```tsx
"use client";

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { ProtectedRoute } from '@/components/auth/protected-route';
import { MainLayout } from '@/components/layout/main-layout';

export default function ProductosPage() {
  const router = useRouter();
  const t = useTranslations('productos');
  const tCommon = useTranslations('common');

  return (
    <ProtectedRoute>
      <MainLayout>
        <div>
          <h1>{t('title')}</h1>
          <button onClick={() => router.push('/productos/nuevo')}>
            {tCommon('create')}
          </button>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}
```

