# 🔐 Enterprise Authentication & Authorization Service

Servicio de autenticación y autorización construido con **Clean Architecture**, **CQRS** y prácticas modernas de seguridad.

---

## 🎯 Descripción del Proyecto

Este es un **servicio de autenticación** diseñado para demostrar:

- **Seguridad de nivel empresarial** (JWT, gestión de sesiones, rate limiting)
- ✅ **Clean Architecture** con clara separación de responsabilidades
- ✅ **Patrón CQRS** con MediatR para manejo escalable de comandos y consultas
- ✅ **Domain-Driven Design** con modelos de dominio ricos
- ✅ **Enfoque security-first** con mitigación de múltiples amenazas

**Este NO es un tutorial básico de login/register.** Este servicio implementa características de seguridad reales usadas en sistemas empresariales.

---

## ✨ Características Principales

### 🔒 Autenticación
- **Autenticación basada en JWT** con algoritmo HS256
- **Sistema de doble token**: Access tokens de corta duración (15 min) + Refresh tokens de larga duración (7 días)
- **Rotación automática de tokens** al refrescar para prevenir ataques de reuso
- **Hashing de contraseñas con BCrypt** (factor de costo 12)
- **Validación estricta de contraseñas**: mínimo 8 caracteres, mayúsculas, minúsculas, números y caracteres especiales

### 🛡️ Autorización
- **Control de acceso basado en roles**: Admin, Manager, User, Guest
- **Autorización basada en claims** con políticas personalizables
- **Protección a nivel de endpoint** con decoradores `[Authorize]`
- **Validación de permisos granulares** según el rol del usuario

### 📱 Gestión de Sesiones
- **Seguimiento multi-dispositivo**: Administra sesiones en múltiples dispositivos
- **Identificación de dispositivos** con IP, User-Agent y nombre de dispositivo
- **Revocación manual de sesiones** (cerrar sesión en un dispositivo específico)
- **Revocación masiva** (cerrar sesión en todos los dispositivos)
- **Detección de brechas de seguridad**: Revoca todas las sesiones automáticamente al detectar reuso de tokens

### 🚨 Seguridad Avanzada
- **Rate Limiting**: 100 requests/minuto por IP para prevenir fuerza bruta
- **Account Lockout**: Bloqueo de cuenta por 15 minutos después de 5 intentos fallidos
- **Detección de token reusado**: Identifica y mitiga ataques de replay
- **Security Headers**: Protección contra XSS, Clickjacking, MIME sniffing
- **Request Logging**: Trazabilidad completa con X-Request-Id
- **Auditoría automática**: Registro de todos los eventos de seguridad

---

## 🏗️ Arquitectura


El sistema utiliza **Clean Architecture**, separando claramente responsabilidades y asegurando que la lógica de negocio sea independiente de frameworks y bases de datos.

Las solicitudes entran por la **API**, se procesan en la **capa de aplicación** mediante comandos y consultas (**CQRS**), ejecutan las reglas de negocio en el **dominio**, y finalmente acceden a recursos externos desde **infraestructura**.  
Todas las dependencias apuntan hacia el dominio, manteniendo el sistema **escalable**, **mantenible** y **testeable**.

---

**Patrones Implementados:**
- Clean Architecture (Onion Architecture)
- CQRS (Command Query Responsibility Segregation)
- Repository Pattern + Unit of Work
- Domain-Driven Design (DDD)
- Result Pattern (manejo de errores sin excepciones)
- Specification Pattern (queries reutilizables)

**Principios SOLID:**
- **S** - Single Responsibility: Cada clase tiene una única responsabilidad (handlers, validators, repositories)
- **O** - Open/Closed: Extensible mediante interfaces (IRepository, ITokenService, etc)
- **L** - Liskov Substitution: Herencia respetando contratos (BaseEntity, AuditableEntity)
- **I** - Interface Segregation: Interfaces específicas (IPasswordHasher, IDateTime, IAuditService)
- **D** - Dependency Inversion: Dependencias a través de abstracciones, no implementaciones concretas

---

## 🛠️ Stack Tecnológico

### Backend
- **.NET 9.0** - Framework principal
- **ASP.NET Core Web API** - API RESTful
- **Entity Framework Core 9** - ORM con migraciones
- **PostgreSQL 14+** - Base de datos relacional
- **MediatR** - Implementación de CQRS
- **FluentValidation** - Validaciones declarativas
- **BCrypt.Net** - Hashing de contraseñas
- **System.IdentityModel.Tokens.Jwt** - Generación y validación de JWT

---

## 📂 Estructura del Proyecto
```
AuthService/
├── AuthService.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── SessionsController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── RateLimitingMiddleware.cs
│   │   ├── SecurityHeadersMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Extensions/
│   │   ├── MiddlewareExtensions.cs
│   │   └── AuthorizationExtensions.cs
│   ├── Services/
│   │   └── CurrentUserService.cs
│   └── Program.cs
│
├── AuthService.Application/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── Login/
│   │   │   ├── Register/
│   │   │   ├── RefreshToken/
│   │   │   ├── Logout/
│   │   │   └── ChangePassword/
│   │   └── Queries/
│   ├── Sessions/
│   │   ├── Commands/
│   │   │   ├── RevokeSession/
│   │   │   └── RevokeAllSessions/
│   │   └── Queries/
│   │       └── GetActiveSessions/
│   ├── Common/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   └── Behaviors/
│   └── DependencyInjection.cs
│
├── AuthService.Domain/
│   ├── Entities/
│   │   ├── Common/
│   │   ├── User.cs
│   │   ├── RefreshToken.cs
│   │   ├── Session.cs
│   │   └── AuditLog.cs
│   ├── Enums/
│   │   ├── UserRole.cs
│   │   ├── SessionStatus.cs
│   │   └── AuditEventType.cs
│   ├── ValueObjects/
│   │   ├── Email.cs
│   │   └── HashedPassword.cs
│   └── Exceptions/
│       ├── InvalidCredentialsException.cs
│       ├── AccountLockedException.cs
│       ├── InvalidTokenException.cs
│       └── TokenReusedException.cs
│
└── AuthService.Infrastructure/
    ├── Persistence/
    │   ├── ApplicationDbContext.cs
    │   ├── ApplicationDbContextFactory.cs
    │   ├── ApplicationDbContextSeed.cs
    │   ├── Configurations/
    │   ├── Interceptors/
    │   └── Migrations/
    ├── Identity/
    │   ├── JwtSettings.cs
    │   ├── TokenService.cs
    │   ├── PasswordHasher.cs
    │   ├── DateTimeService.cs
    │   └── AuditService.cs
    ├── Repositories/
    │   ├── Repository.cs
    │   └── UnitOfWork.cs
    └── DependencyInjection.cs
```

---

## 🚀 Instalación y Ejecución

### Prerrequisitos
- **.NET 9 SDK** ([Descargar](https://dotnet.microsoft.com/download))
- **PostgreSQL 14+** ([Descargar](https://www.postgresql.org/download/))
- **EF Core Tools**: `dotnet tool install --global dotnet-ef`

### 1. Clonar el repositorio
```bash
git clone https://github.com/bolanosjosue/enterprise-auth-service.git
cd enterprise-auth-service
```

### 2. Configurar la base de datos

Crear la base de datos en PostgreSQL:
```sql
CREATE DATABASE AuthServiceDb;
```

### 3. Configurar variables de entorno

Editar `AuthService.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AuthServiceDb;Username=postgres;Password=TU_PASSWORD"
  },
  "JwtSettings": {
    "Secret": "tu-secret-key-de-al-menos-32-caracteres-aqui-cambiar-en-produccion",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "Issuer": "AuthServiceAPI",
    "Audience": "AuthServiceClient"
  }
}
```

### 4. Aplicar migraciones
```bash
dotnet ef database update --project AuthService.Infrastructure --startup-project AuthService.API
```

Esto creará:
- ✅ Todas las tablas (Users, RefreshTokens, Sessions, AuditLogs)
- ✅ Índices y relaciones
- ✅ Datos de prueba (4 usuarios con diferentes roles)

### 5. Ejecutar la aplicación
```bash
cd AuthService.API
dotnet run
```

La API estará disponible en:
- **HTTP**: `http://localhost:5228`
- **Swagger**: `http://localhost:5228/swagger`

---

## 📚 Documentación de API (Swagger)

Acceder a: `http://localhost:5228/swagger`

### Autenticación en Swagger

1. Ejecutar el endpoint `POST /api/auth/login`
2. Usar credenciales:
```json
   {
     "email": "admin@authservice.com",
     "password": "Admin123!"
   }
```
3. Copiar el `accessToken` de la respuesta
4. Click en **"Authorize"** (🔒 arriba a la derecha)
5. Ingresar: `Bearer {tu-token-aqui}`
6. Probar endpoints protegidos

---

## 🔐 Credenciales de Prueba

El seed data incluye 4 usuarios con diferentes roles:
```
Admin:
Email: admin@authservice.com
Password: Admin123!
Rol: Admin (acceso total)

Manager:
Email: manager@authservice.com
Password: Manager123!
Rol: Manager

User:
Email: user@authservice.com
Password: User123!
Rol: User

Guest:
Email: guest@authservice.com
Password: Guest123!
Rol: Guest (solo lectura)
```

---

## 🔒 Endpoints Principales

### Authentication

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Registrar nuevo usuario | ❌ |
| POST | `/api/auth/login` | Iniciar sesión | ❌ |
| POST | `/api/auth/refresh` | Refrescar access token | ❌ |
| POST | `/api/auth/logout` | Cerrar sesión | ❌ |
| POST | `/api/auth/change-password` | Cambiar contraseña | ✅ |

### Sessions

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/sessions/active` | Listar sesiones activas | ✅ |
| DELETE | `/api/sessions/{id}` | Revocar sesión específica | ✅ |
| DELETE | `/api/sessions/revoke-all` | Revocar todas las sesiones | ✅ |

---

## 👥 Roles y Permisos

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| **Admin** | Administrador del sistema | Acceso total, gestión de usuarios |
| **Manager** | Supervisor | Acceso a recursos, gestión de sesiones |
| **User** | Usuario estándar | Acceso a su propia información |
| **Guest** | Invitado | Solo lectura |

---

## 🧪 Testing

### Postman Collection

Importar `AuthService.postman_collection.json` en Postman:

1. Open Postman → Import → Seleccionar archivo
2. Ejecutar requests en orden:
   - Login → Get Active Sessions → Refresh Token → Logout

Todos los tokens se guardan automáticamente en variables.

### Testing Manual

Ver guía completa en: [TESTING.md](TESTING.md)


---

## 🛡️ Pruebas de Seguridad

Este proyecto incluye pruebas de seguridad documentadas con Kali Linux:
Ver todas las pruebas completas y documentadas: [Security.md](Security.md)

## 📊 Pruebas de Seguridad Realizadas

### ✅ Prueba 1: Rate Limiting con Apache Bench
**Herramienta:** Apache Bench  
**Objetivo:** Verificar límite de requests por minuto desde misma IP  
**Resultado:** ✅ PASS - 120/120 requests bloqueados con 429 Too Many Requests

### ✅ Prueba 2: Account Lockout (Bloqueo de Cuenta)
**Herramienta:** curl (manual)  
**Objetivo:** Verificar bloqueo después de 5 intentos fallidos de login  
**Resultado:** ✅ PASS - Cuenta bloqueada por 15 minutos con 403 Forbidden

### ✅ Prueba 3: SQL Injection con sqlmap
**Herramienta:** sqlmap  
**Objetivo:** Detectar vulnerabilidades de inyección SQL en todos los parámetros  
**Resultado:** ✅ PASS - Ningún parámetro vulnerable, queries parametrizadas

### ✅ Prueba 4: JWT Token Tampering con jwt_tool
**Herramienta:** jwt_tool  
**Objetivo:** Verificar validación de firma JWT y prevención de privilege escalation  
**Resultado:** ✅ PASS - Token manipulado rechazado con 401 Unauthorized

### ✅ Prueba 5: Token Reuse Detection (Replay Attack)
**Herramienta:** curl (manual)  
**Objetivo:** Detectar reutilización de refresh tokens revocados  
**Resultado:** ✅ PASS - Token reusado detectado con mensaje de security breach

### ✅ Prueba 6: Nmap Vulnerability Scan & CVE Detection
**Herramienta:** Nmap NSE  
**Objetivo:** Detectar CVEs conocidos y configuraciones inseguras  
**Resultado:** ✅ PASS - Sin vulnerabilidades detectadas, security headers presentes

### ✅ Prueba 7: Fuzzing con wfuzz (632 Payloads)
**Herramienta:** wfuzz  
**Objetivo:** Probar resistencia contra SQL Injection, XSS y múltiples vectores de ataque  
**Resultado:** ✅ PASS - 632/632 payloads bloqueados, 0 ataques exitosos

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
