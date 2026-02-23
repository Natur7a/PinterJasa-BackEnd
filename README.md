"# PinterJasa Backend

A service marketplace platform backend built with .NET 8 ASP.NET Core Web API, connecting customers with service providers.

## Tech Stack

- **Runtime:** .NET 8 / ASP.NET Core Web API
- **Database:** PostgreSQL 16 via Entity Framework Core (Npgsql)
- **Auth:** JWT Bearer Tokens with BCrypt password hashing
- **Docs:** Swagger/OpenAPI via Swashbuckle
- **Containerization:** Docker + Docker Compose

## Quick Start

### With Docker Compose

```bash
docker-compose up -d
```

The API will be available at `http://localhost:3000`. Swagger UI at `http://localhost:3000/swagger`.

### Local Development

**Prerequisites:** .NET 8 SDK, PostgreSQL 16

```bash
cd src/PinterJasa.API
dotnet run
```

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=pinterjasa;...` |
| `Jwt__Key` | JWT signing key (min 32 chars) | See appsettings.json |
| `Jwt__Issuer` | JWT issuer | `PinterJasa` |
| `Jwt__Audience` | JWT audience | `PinterJasa` |

## API Endpoints

### Auth (Public)
| Method | Path | Description |
|---|---|---|
| POST | `/api/auth/register` | Register user (customer or provider) |
| POST | `/api/auth/login` | Login, returns JWT token |

### Categories (Public)
| Method | Path | Description |
|---|---|---|
| GET | `/api/categories` | List all active categories |
| GET | `/api/categories/{id}` | Get single category |

### Services
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/services` | Public | List active services (optional `?categoryId=`) |
| GET | `/api/services/{id}` | Public | Get single service |
| POST | `/api/services` | Provider | Create service |
| GET | `/api/services/mine` | Provider | List my services |

### Orders
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/orders` | Customer | Create order |
| GET | `/api/orders/{id}` | Authenticated | Get order by ID |
| GET | `/api/orders/mine` | Customer | List my orders |
| GET | `/api/orders/provider` | Provider | List orders assigned to me |
| PATCH | `/api/orders/{id}/status` | Authenticated | Update order status |

### Payments
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/payments` | Customer | Create payment for order |
| PATCH | `/api/payments/{id}/confirm` | Admin | Confirm payment |
| GET | `/api/payments/order/{orderId}` | Authenticated | Get payment by order ID |

### Payouts
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/payouts/{orderId}` | Admin | Create payout for completed order |
| PATCH | `/api/payouts/{id}/process` | Admin | Mark payout as completed |
| GET | `/api/payouts/mine` | Provider | List my payouts |

### Reviews
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/reviews` | Customer | Submit review (one per order) |
| GET | `/api/reviews/provider/{providerId}` | Public | Get all reviews for a provider |

## Order Status Flow

```
created → awaiting_payment → paid → accepted → in_progress → completed
   ↓            ↓             ↓         ↓
cancelled    cancelled    cancelled  cancelled
                           ↓
                         refunded
```

## Money Flow

```
Customer pays → Payment (gross_amount)
                    ↓
             Payout created:
             gross_amount = order total
             commission_amount = gross * commission_rate (default 15%)
             net_amount = gross - commission
                    ↓
             Provider receives net_amount
```

## Roles

- **customer** — can place orders, make payments, write reviews
- **provider** — can create services, manage assigned orders, receive payouts
- **admin** — can confirm payments, create and process payouts

## Xendit Payment Gateway

PinterJasa uses [Xendit](https://xendit.co) as the payment gateway for processing payments and disbursements.

### Required Environment Variables

| Variable | Description |
|---|---|
| `Xendit__SecretApiKey` | Xendit secret API key |
| `Xendit__PublicApiKey` | Xendit public API key |
| `Xendit__WebhookVerificationToken` | Token to verify Xendit webhook requests |

### Webhook URLs (configure in Xendit Dashboard)

| Event | URL |
|---|---|
| Invoice (payment) | `https://pinterjasa.com/api/webhooks/xendit/invoice` |
| Disbursement (payout) | `https://pinterjasa.com/api/webhooks/xendit/disbursement` |

### Payment Flow

```
Customer creates order
    → Customer creates payment (POST /api/payments)
    → Backend creates Xendit invoice
    → Customer redirected to Xendit payment page (invoice_url)
    → Customer pays via Xendit (bank transfer, ewallet, etc.)
    → Xendit sends webhook to /api/webhooks/xendit/invoice
    → Backend updates payment status to "paid"
    → Backend updates order status to "paid"
```

### Payout Flow

```
Order completed
    → Admin creates payout (POST /api/payouts/{orderId})
    → Admin processes payout (PATCH /api/payouts/{id}/process)
    → Backend creates Xendit disbursement
    → Xendit sends money to provider's bank account
    → Xendit sends webhook to /api/webhooks/xendit/disbursement
    → Backend updates payout status to "completed"
```
