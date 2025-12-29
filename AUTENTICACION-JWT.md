# Sistema de Autenticación JWT - GesFer

Este documento explica cómo está configurado el sistema de autenticación JWT entre el Backend ASP.NET Core y el Frontend Next.js.

## 📋 Índice

1. [Arquitectura General](#arquitectura-general)
2. [Backend (ASP.NET Core)](#backend-aspnet-core)
3. [Frontend (Next.js + Auth.js v5)](#frontend-nextjs--authjs-v5)
4. [Configuración de Variables de Entorno](#configuración-de-variables-de-entorno)
5. [Uso del Sistema](#uso-del-sistema)

## 🏗️ Arquitectura General

El sistema utiliza:
- **Backend**: JWT Bearer Authentication con tokens que incluyen el Cursor ID como `ClaimTypes.NameIdentifier`
- **Frontend**: Auth.js v5 (NextAuth) con CredentialsProvider que se comunica con la API de C#
- **Comunicación**: El frontend adjunta automáticamente el Bearer Token en cada petición al backend

## 🔧 Backend (ASP.NET Core)

### Configuración JWT

La configuración JWT se encuentra en `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "GesFerApi",
    "Audience": "GesFerClient",
    "ExpirationMinutes": 60
  }
}
```

### Componentes Principales

#### 1. JwtService (`Api/src/Infrastructure/Services/JwtService.cs`)

Servicio que genera tokens JWT con:
- **Cursor ID** como `ClaimTypes.NameIdentifier` (UserId convertido a string)
- **Username** como `ClaimTypes.Name`
- **UserId** como claim personalizado
- **Permisos** como claims individuales

#### 2. LoginCommandHandler

Modificado para generar el JWT usando `JwtService` y devolver:
- `Token`: El JWT generado
- `CursorId`: El UserId convertido a string

#### 3. Program.cs

Configuración de autenticación JWT:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});
```

#### 4. Controller Protegido de Ejemplo

`ProfileController` muestra cómo:
- Usar `[Authorize]` para proteger endpoints
- Extraer el Cursor ID: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- Acceder a otros claims del usuario

```csharp
[HttpGet("me")]
[Authorize]
public IActionResult GetMyProfile()
{
    var cursorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = User.FindFirst(ClaimTypes.Name)?.Value;
    var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
    // ...
}
```

## 🎨 Frontend (Next.js + Auth.js v5)

### Configuración de Auth.js

El archivo `auth.ts` configura:
- **CredentialsProvider**: Autentica contra `/api/auth/login` de la API de C#
- **Callbacks JWT**: Persiste `accessToken` y `cursorId` en el token de NextAuth
- **Callbacks Session**: Expone los datos en `session.user` y `session.accessToken`

### Componentes Principales

#### 1. auth.ts

Configuración principal de NextAuth con:
- `authorize`: Hace fetch al endpoint de login y retorna el usuario con token
- `jwt`: Almacena `accessToken` y `cursorId` en el token JWT de NextAuth
- `session`: Expone los datos para Server y Client Components

#### 2. middleware.ts

Protege rutas automáticamente:
- Rutas públicas: `/login`, `/api/auth`
- Rutas protegidas: `/dashboard`, `/usuarios`, `/clientes`, `/empresas`
- Redirige a `/login` si no está autenticado
- Redirige a `/dashboard` si está autenticado y accede a `/login`

#### 3. Clientes API

**Para Server Components:**
```typescript
import { serverApiClient } from "@/lib/api/client-server";

// Automáticamente adjunta el Bearer Token de la sesión
const data = await serverApiClient.get("/api/users");
```

**Para Client Components:**
```typescript
"use client";
import { useApiClient } from "@/lib/api/client-client";

function MyComponent() {
  const api = useApiClient();
  // El token se adjunta automáticamente desde la sesión
}
```

**Cliente API genérico:**
```typescript
import { apiClient } from "@/lib/api/client";

// Puedes pasar el token manualmente
apiClient.setAccessToken(token);
const data = await apiClient.get("/api/users");
```

## 🔐 Configuración de Variables de Entorno

### Frontend (.env.local)

Crea un archivo `.env.local` en `Cliente/`:

```env
# URL de la API de ASP.NET Core
NEXT_PUBLIC_API_URL=http://localhost:5000

# Secret para NextAuth (genera uno con: openssl rand -base64 32)
AUTH_SECRET=your-super-secret-key-change-in-production

# Entorno
NEXT_PUBLIC_ENV=development
```

### Backend (appsettings.json)

Asegúrate de tener configurado:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "GesFerApi",
    "Audience": "GesFerClient",
    "ExpirationMinutes": 60
  }
}
```

**⚠️ IMPORTANTE**: En producción:
- Usa variables de entorno para `JwtSettings:SecretKey`
- Genera una clave secreta fuerte (mínimo 32 caracteres)
- No commitees las claves secretas al repositorio

## 📖 Uso del Sistema

### 1. Login en el Frontend

El login se realiza automáticamente a través de Auth.js:

```typescript
import { signIn } from "@/auth";

await signIn("credentials", {
  empresa: "Empresa Demo",
  usuario: "admin",
  contraseña: "admin123",
  redirect: true,
});
```

### 2. Obtener Sesión en Server Components

```typescript
import { auth } from "@/auth";
import { getCursorId } from "@/lib/api/auth-helper";

export default async function MyPage() {
  const session = await auth();
  const cursorId = await getCursorId();
  
  if (!session) {
    redirect("/login");
  }
  
  return <div>Hola {session.user.username}</div>;
}
```

### 3. Obtener Sesión en Client Components

```typescript
"use client";
import { useSession } from "@/lib/hooks/use-session";

export default function MyComponent() {
  const { data: session, accessToken, cursorId } = useSession();
  
  if (!session) {
    return <div>No autenticado</div>;
  }
  
  return <div>Hola {session.user.username}</div>;
}
```

### 4. Hacer Peticiones al Backend

**Server Components:**
```typescript
import { serverApiClient } from "@/lib/api/client-server";

const users = await serverApiClient.get("/api/users");
```

**Client Components:**
```typescript
"use client";
import { useApiClient } from "@/lib/api/client-client";

function MyComponent() {
  const api = useApiClient();
  
  useEffect(() => {
    api.get("/api/users").then(setUsers);
  }, []);
}
```

### 5. Proteger Endpoints en el Backend

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación JWT
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        // Extraer Cursor ID
        var cursorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Extraer otros claims
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
        
        return Ok(new { cursorId, username, permissions });
    }
}
```

## 🔒 Seguridad

1. **Tokens JWT**: Tienen expiración de 60 minutos (configurable)
2. **HTTPS**: Usa HTTPS en producción
3. **Secret Keys**: Nunca commitees las claves secretas
4. **CORS**: Configurado en el backend para permitir el frontend
5. **Validación**: El backend valida issuer, audience, firma y expiración

## 🐛 Troubleshooting

### El token no se adjunta en las peticiones

- Verifica que `AUTH_SECRET` esté configurado en `.env.local`
- Asegúrate de que la sesión esté activa: `await auth()` en Server Components
- En Client Components, verifica que `useSession()` retorne `status === "authenticated"`

### Error 401 Unauthorized

- Verifica que el token JWT sea válido
- Comprueba que `JwtSettings` en el backend coincida con la configuración
- Verifica que el token no haya expirado (60 minutos por defecto)

### El middleware no protege las rutas

- Verifica que `auth.ts` esté en la raíz del proyecto
- Asegúrate de que el matcher en `middleware.ts` incluya las rutas que quieres proteger
- Comprueba que `AUTH_SECRET` esté configurado

## 📚 Referencias

- [NextAuth.js v5 Documentation](https://authjs.dev/)
- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [JWT.io](https://jwt.io/) - Para debuggear tokens JWT

