# 🧪 Guía de Testing

Esta guía proporciona instrucciones paso a paso para probar todas las funcionalidades del servicio de autenticación.

---

## 🚀 Pruebas con Swagger

### 1. Iniciar la aplicación
```bash
cd AuthService.API
dotnet run
```

Acceder a: `http://localhost:5228/swagger`

---

### 2. Flujo de Pruebas Básicas

#### ✅ PASO 1: Registrar un nuevo usuario

**Endpoint:** `POST /api/auth/register`
```json
{
  "email": "test@example.com",
  "password": "Test123!@#",
  "fullName": "Usuario de Prueba"
}
```

**Resultado esperado:**
```json
{
  "id": "...",
  "email": "test@example.com",
  "fullName": "Usuario de Prueba",
  "role": "User",
  "isActive": true,
  "createdAt": "..."
}
```

---

#### ✅ PASO 2: Iniciar sesión

**Endpoint:** `POST /api/auth/login`
```json
{
  "email": "admin@authservice.com",
  "password": "Admin123!",
  "deviceName": "Web Browser"
}
```

**Resultado esperado:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456...",
  "expiresAt": "2026-01-08T18:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@authservice.com",
    "fullName": "System Administrator",
    "role": "Admin",
    "isActive": true,
    "lastLoginAt": "2026-01-08T17:45:00Z",
    "createdAt": "2024-06-17T12:00:00Z"
  }
}
```

**📋 Acción:** Copiar el `accessToken`

---

#### ✅ PASO 3: Autorizar en Swagger

1. Click en el botón **"Authorize"** (🔒 arriba a la derecha)
2. Ingresar: `Bearer {tu-access-token-aqui}`
3. Click **"Authorize"**
4. Click **"Close"**

---

#### ✅ PASO 4: Ver sesiones activas

**Endpoint:** `GET /api/sessions/active`

**Resultado esperado:**
```json
[
  {
    "id": "...",
    "deviceName": "Web Browser",
    "ipAddress": "::1",
    "userAgent": "Mozilla/5.0...",
    "lastActivityAt": "2026-01-08T17:45:00Z",
    "status": "Active",
    "createdAt": "2026-01-08T17:45:00Z",
    "isCurrent": false
  }
]
```

---

#### ✅ PASO 5: Refrescar token

**Endpoint:** `POST /api/auth/refresh`
```json
{
  "refreshToken": "abc123def456..."
}
```

**Resultado esperado:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9... (NUEVO)",
  "refreshToken": "xyz789ghi012... (NUEVO)",
  "expiresAt": "2026-01-08T18:15:00Z",
  "user": { ... }
}
```

⚠️ **Importante:** El refresh token anterior queda **invalidado**.

---

#### ✅ PASO 6: Cambiar contraseña

**Endpoint:** `POST /api/auth/change-password`
```json
{
  "currentPassword": "Admin123!",
  "newPassword": "NewAdmin123!@#"
}
```

**Resultado esperado:**
```json
{
  "message": "Password changed successfully. Please login again on all devices."
}
```

⚠️ **Importante:** Todos los tokens quedan **invalidados**. Necesitas hacer login de nuevo.

---

#### ✅ PASO 7: Revocar sesión específica

**Endpoint:** `DELETE /api/sessions/{sessionId}`

Reemplazar `{sessionId}` con un ID real de "Get Active Sessions".

**Resultado esperado:**
```json
{
  "message": "Session revoked successfully"
}
```

---

#### ✅ PASO 8: Cerrar sesión en todos los dispositivos

**Endpoint:** `DELETE /api/sessions/revoke-all`

**Resultado esperado:**
```json
{
  "message": "All sessions revoked successfully"
}
```

---

#### ✅ PASO 9: Cerrar sesión

**Endpoint:** `POST /api/auth/logout`
```json
{
  "refreshToken": "xyz789ghi012..."
}
```

**Resultado esperado:**
```json
{
  "message": "Logged out successfully"
}
```

---

## 📬 Pruebas con Postman

### 1. Importar Collection

1. Abrir Postman
2. Click en **"Import"**
3. Seleccionar `AuthService.postman_collection.json`
4. Click **"Import"**

---

### 2. Configurar Variables

Las variables ya están preconfiguradas:
- `base_url`: `http://localhost:5228/api`
- `access_token`: Se guarda automáticamente
- `refresh_token`: Se guarda automáticamente

Si tu API usa otro puerto, editar la variable `base_url`.

---

### 3. Flujo de Pruebas en Postman

#### 🔐 Autenticación

1. **Login**
   - Ejecutar → Los tokens se guardan automáticamente
   - Verificar respuesta 200 OK

2. **Register**
   - Cambiar email a uno único
   - Ejecutar → Usuario creado

3. **Refresh Token**
   - Ejecutar → Nuevos tokens generados automáticamente

4. **Change Password**
   - Requiere estar autenticado
   - Todos los tokens se invalidan

5. **Logout**
   - Revoca el refresh token actual

---

#### 📱 Sesiones

1. **Get Active Sessions**
   - Ver todas las sesiones activas
   - Identificar dispositivos

2. **Revoke Session**
   - Copiar un `sessionId` de la respuesta anterior
   - Pegar en la URL: `/api/sessions/{sessionId}`
   - Cerrar sesión en ese dispositivo específico

3. **Revoke All Sessions**
   - Cierra sesión en TODOS los dispositivos
   - Útil al detectar actividad sospechosa

---

## 🧪 Escenarios de Prueba

### ✅ Happy Path (Flujo Normal)
```
1. Register → 200 OK
2. Login → 200 OK (tokens recibidos)
3. Get Active Sessions → 200 OK (1 sesión)
4. Refresh Token → 200 OK (nuevos tokens)
5. Logout → 200 OK
```

---

### ❌ Escenarios de Error

#### **Error 1: Credenciales incorrectas**

**Request:**
```json
{
  "email": "admin@authservice.com",
  "password": "WrongPassword123"
}
```

**Respuesta:**
```json
{
  "error": "Invalid email or password"
}
```

---

#### **Error 2: Token expirado**

Esperar 15 minutos después del login y ejecutar `GET /api/sessions/active`.

**Respuesta:**
```
401 Unauthorized
{
  "error": "Token has expired"
}
```

---

#### **Error 3: Reutilización de refresh token**

1. Hacer login → Guardar refresh token
2. Refrescar token → Guardar NUEVO refresh token
3. Intentar usar el refresh token ANTIGUO

**Respuesta:**
```
401 Unauthorized
{
  "error": "Token has been reused - possible security breach detected"
}
```

**Efecto secundario:** Todas las sesiones del usuario quedan **revocadas automáticamente**.

---

#### **Error 4: Cuenta bloqueada**

Hacer 5 intentos de login con contraseña incorrecta.

**Respuesta:**
```
403 Forbidden
{
  "error": "Account is locked due to multiple failed login attempts",
  "details": "Account locked until 2026-01-08 18:00:00 UTC"
}
```

---

#### **Error 5: Validación de contraseña débil**

**Request:**
```json
{
  "email": "test@test.com",
  "password": "weak",
  "fullName": "Test"
}
```

**Respuesta:**
```
400 Bad Request
{
  "error": "Validation failed",
  "details": "Password must be at least 8 characters; Password must contain at least one uppercase letter; ..."
}
```

---

#### **Error 6: Acceso sin autenticación**

Ejecutar `GET /api/sessions/active` sin token.

**Respuesta:**
```
401 Unauthorized
```

---

## 🔒 Pruebas de Seguridad

### 1. Rate Limiting

**Objetivo:** Verificar límite de 100 requests/minuto

**Pasos:**
1. Ejecutar login 120 veces seguidas (usar script o herramienta)
2. Verificar que después de ~100 requests, la API responda con `429 Too Many Requests`

**Herramienta recomendada:** Apache Bench (ver sección de Kali Linux)

---

### 2. Token Rotation

**Objetivo:** Verificar que los refresh tokens se invalidan después de usarse

**Pasos:**
1. Login → Guardar `refreshToken1`
2. Refresh → Recibir `refreshToken2`
3. Intentar usar `refreshToken1` de nuevo
4. Verificar error: "Token has been reused"

---

### 3. Session Security

**Objetivo:** Verificar gestión de sesiones multi-dispositivo

**Pasos:**
1. Login desde 3 "dispositivos" (3 veces con diferentes `deviceName`)
2. Ver sesiones activas → Deberían ser 3
3. Revocar sesión 1
4. Ver sesiones activas → Deberían ser 2
5. Revocar todas las sesiones
6. Ver sesiones activas → Debería dar 401 (sin sesión activa)

---

### 4. Password Security

**Objetivo:** Verificar bloqueo de cuenta

**Pasos:**
1. Intentar login con password incorrecta 5 veces
2. Verificar respuesta `403 Forbidden` con "Account is locked"
3. Esperar 15 minutos (o cambiar manualmente en DB para pruebas rápidas)
4. Intentar login de nuevo → Debería funcionar

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT.

---

## 👨‍💻 Autor

**Josue Bolanos**

- GitHub: [@bolanosjosue](https://github.com/bolanosjosue)
- LinkedIn: [josuebolanos-dev](https://www.linkedin.com/in/josuebolanos-dev/)
- Email: josuebolanos2004@gmail.com

---


⭐ **Si este proyecto te fue útil, considera darle una estrella en GitHub**






