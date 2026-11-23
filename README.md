# USB Exfiltration Detector – API

**ASP.NET Core 8 Web API** for enterprise USB data-exfiltration detection.

As a requirement for the partial fulfilment of an MSc. Cybersecurity Engineering at the University of East London, U.K. It is required that this project be built, tested against established test-cases.

This service is the central backend of the USB Exfiltration Detector platform. It receives structured telemetry from Windows Worker Services running on endpoints, reconstructs and stores USB sessions with explainable risk scores, manages organisations and auditors, and exposes authenticated endpoints for the React dashboard.

---

## Overview

| Component | Role |
|-----------|------|
| **Windows Worker Service** | Runs on endpoints, detects USB activity, scores risk, sends payloads |
| **This API** | Ingests payloads, persists data, authenticates auditors, triggers notifications |
| **React Frontend** | Organisation-scoped dashboard for security auditors |

The API is **multi-tenant**. Every organisation is identified by a unique `CorpCode`. All data access is strictly scoped by organisation.

---

## Key Features

- **Ingest endpoint** – Receives batched, structured USB session payloads from Worker Services
- **Explainable risk scoring** – Stores human-readable reasons that cite Windows Event Record IDs
- **Multi-tenant isolation** – Full tenant separation via `CorpCode` / `OrganizationId`
- **Auditor authentication** – JWT (access + refresh tokens) with role-based access (`Admin`, `Auditor`, `Viewer`)
- **Allowed devices** – Per-organisation whitelist of approved USB serial numbers
- **Notifications** – Email / webhook alerts for high-risk sessions and missing audit settings
- **Subscriptions** – Free-trial by default; plan status and feature flags enforced
- **Offline-friendly** – Designed to accept delayed batches when endpoints reconnect

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | PostgreSQL (recommended) or SQL Server |
| Auth | JWT + refresh tokens |
| Password hashing | Argon2id (preferred) or bcrypt |
| Email (dev) | Resend / Brevo / Mailtrap |

---

## Solution Structure (suggested)

```
UsbExfiltrationDetector.Api/
├── Controllers/          # or Minimal API endpoints
├── Data/                 # DbContext, entities, migrations
├── DTOs/
├── Services/             # Ingest, notification, subscription logic
├── Auth/                 # JWT, password hashing
├── Middleware/
├── appsettings.json
└── Program.cs
```

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL (or SQL Server)
- An email provider API key (for notifications in development)

### 1. Clone & restore

```bash
git clone <your-repo-url>
cd UsbExfiltrationDetector.Api
dotnet restore
```

### 2. Configuration

Copy `appsettings.Development.json.example` to `appsettings.Development.json` (or use User Secrets / environment variables) and set:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=usb_detector;Username=...;Password=..."
  },
  "Jwt": {
    "Issuer": "UsbExfiltrationDetector",
    "Audience": "UsbExfiltrationDetector",
    "Key": "<long-random-secret>",
    "AccessTokenMinutes": 30,
    "RefreshTokenDays": 7
  },
  "ServiceAuth": {
    "ApiKey": "<shared-secret-for-worker-services>"
  },
  "Email": {
    "Provider": "Resend",
    "ApiKey": "<your-key>",
    "FromAddress": "alerts@yourdomain.com"
  }
}
```

**Never commit real secrets.** Use environment variables or a secret store in production.

### 3. Database

```bash
dotnet ef database update
```

### 4. Run

```bash
dotnet run
```

The API will listen on the URLs configured in `launchSettings.json` (typically `https://localhost:7xxx`).

---

## Authentication

### Auditor (human) login

```
POST /api/auth/login
Content-Type: application/json

{
  "corpCode": "ACME-UK-001",
  "username": "j.smith",
  "password": "..."
}
```

Returns an access token and refresh token. Subsequent requests must include:

```
Authorization: Bearer <access-token>
```

### Worker Service authentication

Worker Services authenticate with a shared API key (or client-credentials grant):

```
X-Api-Key: <service-api-key>
```

or the equivalent configured header/scheme.

---

## Main Endpoints

### Ingestion (Worker → API)

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/ingest/batch` | Accept one or more structured USB session payloads |

### Authentication

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/auth/login` | Auditor login |
| `POST` | `/api/auth/refresh` | Refresh access token |
| `POST` | `/api/auth/logout` | Invalidate refresh token |

### Sessions & Alerts

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/sessions` | List sessions (filterable) |
| `GET` | `/api/sessions/{id}` | Full session detail + reasons + events |
| `GET` | `/api/alerts` | Recent high-severity alerts |

### Allowed Devices

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/devices` | List allowed devices |
| `POST` | `/api/devices` | Add device |
| `PUT` | `/api/devices/{id}` | Update device |
| `DELETE` | `/api/devices/{id}` | Soft-delete / deactivate |

### Auditors (Admin only)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/auditors` | List auditors |
| `POST` | `/api/auditors` | Create auditor |
| `PUT` | `/api/auditors/{id}` | Update auditor |

### Subscription & Dashboard

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/subscription` | Current plan status |
| `GET` | `/api/dashboard/summary` | Aggregated counts for home screen |

All organisation-scoped endpoints enforce tenant isolation via the authenticated `CorpCode` / `OrganizationId`.

---

## Ingest Payload Shape

The Worker Service must send payloads that match this contract (see the Worker Service specification for full details):

```json
{
  "corpCode": "ACME-UK-001",
  "host": {
    "hostName": "LAPTOP-JSMITH",
    "deviceId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
    "loggedInUser": "DOMAIN\\j.smith",
    "osVersion": "10.0.22631"
  },
  "session": {
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "deviceSerial": "4C530001A2B3C4D5",
    "vendor": "SanDisk",
    "model": "Cruzer",
    "connectedAt": "2026-11-17T14:22:01Z",
    "disconnectedAt": null,
    "totalBytesWritten": 1876543210,
    "riskScore": 78,
    "reasons": [
      {
        "rule": "LargeVolumeWrite",
        "weight": 30,
        "description": "1.8 GB written to removable volume",
        "eventRecordIds": [412, 413, 414]
      }
    ]
  },
  "events": [ ... ],
  "auditSettingsStatus": {
    "pnpActivityEnabled": true,
    "removableStorageEnabled": false,
    "detectionComplete": false
  }
}
```

---

## Core Data Model

- **Organizations** – tenant root (`CorpCode`, contact details)
- **Auditors** – dashboard users (hashed passwords, roles)
- **AllowedDevices** – approved USB serial numbers per organisation
- **Subscriptions** – plan + status (FreeTrial default)
- **Hosts** – reporting endpoints
- **UsbSessions** – reconstructed sessions with risk score
- **SessionReasons** – explainable rule hits (with Event Record IDs)
- **RawEvents** – optional forensic detail
- **NotificationsLog** – email / webhook delivery status

---

## Security Notes

- Passwords are hashed with Argon2id (or bcrypt). Never stored in plaintext.
- All data queries are filtered by organisation. Cross-tenant access is forbidden.
- JWT access tokens are short-lived; refresh tokens are rotated.
- Rate limiting is applied on the login endpoint.
- Secrets must not appear in source control or logs.
- CORS is restricted to the frontend origin.
- The service-to-API authentication path is separate from human auditor authentication.

---

## Notifications

When a high-risk session is ingested or required Windows audit settings are reported as missing, the API can:

1. Send an email to the organisation’s security contacts
2. Fire a configured webhook

Delivery attempts are recorded in `NotificationsLog`.

---

## Subscriptions

- New organisations start on the **FreeTrial** plan.
- Feature flags (max hosts, retention, webhooks, etc.) are evaluated against the current plan.
- Stripe (or similar) payment collection is intentionally out of scope for the first version; only the data model and enforcement logic are required.

---

## Related Components

| Component | Description |
|-----------|-------------|
| Windows Worker Service | Endpoint agent that detects USB activity and sends payloads to this API |
| React Frontend | Auditor dashboard that consumes the endpoints documented above |

See the companion specification documents for full contracts:

- `01_PROJECT_OVERVIEW_AND_ARCHITECTURE.md`
- `02_WINDOWS_WORKER_SERVICE.md`
- `03_BACKEND_API.md` (this service)
- `04_FRONTEND_AND_INTEGRATION.md`

---

## Development Roadmap (suggested)

1. Database + EF Core models + migrations  
2. Organisation / Auditor / AllowedDevice CRUD + authentication  
3. Ingest endpoint + session/reason/event persistence  
4. Notification pipeline (email first)  
5. Subscription model + basic feature-flag checks  
6. Dashboard summary endpoint  

---

## Licence

Specify your licence here (e.g. MIT, proprietary, etc.).

---

## Contributing

1. Create a feature branch  
2. Follow the existing coding and naming conventions  
3. Ensure all new endpoints respect tenant isolation  
4. Open a pull request with a clear description of the change  

---

**Note on naming**  
The original working title `USBInfilteratorDetectorServiceAPI` contained a typo. This README uses the clearer name **USB Exfiltration Detector – API**, which matches the research and product goal (detection of data *exfiltration* via USB).
