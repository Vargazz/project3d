# Print3D Studio — Plataforma E-commerce

Plataforma completa de e-commerce para impressão 3D personalizada.

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Frontend | Angular 18 + Tailwind CSS |
| Backend | ASP.NET Core Web API (.NET 10) |
| Banco de dados | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Autenticação | JWT + ASP.NET Identity |
| Pagamentos | Mercado Pago SDK (Checkout Pro) |
| Armazenamento | Local (dev) → Cloudinary (prod) |
| Containers | Docker + Docker Compose |

## Pré-requisitos

- .NET 10 SDK
- Node.js 20+ e npm
- Angular CLI 18+
- Docker Desktop

## Iniciando o projeto

### Opção 1 — Tudo via Docker (recomendado para produção/teste completo)

```bash
# Na raiz do projeto
docker compose up --build
```

Aguarde o build e acesse:
| Serviço | URL |
|---------|-----|
| 🌐 Frontend | http://localhost |
| 🔌 API | http://localhost:5000 |
| 🗄️ pgAdmin | http://localhost:5050 |

Na primeira execução o backend aplica as migrations e cria o seed automaticamente.

---

### Opção 2 — Desenvolvimento local (mais rápido para codar)

**Suba apenas o banco:**
```bash
docker compose -f docker-compose.dev.yml up -d
```

**Backend:**
```bash
cd backend/src/Project3D.API
dotnet run
# API disponível em http://localhost:5000
```

**Frontend:**
```bash
cd frontend
ng serve
# App disponível em http://localhost:4200
```

## Variáveis de ambiente

Copie `.env.example` para `.env` e preencha as variáveis:

```bash
cp .env.example .env
```

Variáveis importantes:
- `ConnectionStrings__DefaultConnection` — string de conexão PostgreSQL
- `Jwt__SecretKey` — chave JWT (mínimo 32 caracteres)
- `MercadoPago__AccessToken` — token do Mercado Pago (quando disponível)

## Estrutura do projeto

```
project3d/
├── backend/
│   └── src/
│       ├── Project3D.Domain/        # Entidades, enums, interfaces
│       ├── Project3D.Application/   # DTOs, validators, interfaces de service
│       ├── Project3D.Infrastructure/ # EF Core, services, Identity, Mercado Pago
│       └── Project3D.API/           # Controllers, Program.cs
├── frontend/
│   └── src/app/
│       ├── core/          # Guards, interceptors, services
│       ├── shared/        # SharedModule, componentes, pipes, models
│       └── features/      # Módulos lazy-loaded por feature
├── docker-compose.yml
├── .env.example
└── README.md
```

## Fluxo de compra normal

1. Cliente navega e adiciona produtos ao carrinho
2. Checkout preenche dados pessoais e endereço
3. API cria o pedido (status: Received)
4. API cria preferência no Mercado Pago
5. Cliente é redirecionado para o Checkout Pro do MP
6. MP envia webhook para `/api/payments/webhook`
7. API valida e atualiza status do pedido
8. Cliente acompanha em Minha Conta → Pedidos

## Fluxo de produto personalizado

1. Cliente preenche formulário em `/personalizado`
2. Faz upload de fotos de referência
3. Solicitação criada com status `Received`
4. Admin analisa em `/admin/personalizados`
5. Admin cria orçamento → status `QuoteSent`
6. Cliente recebe notificação e responde ao orçamento
7. Se aceito → link de pagamento Mercado Pago gerado
8. Após confirmação via webhook → `InProduction`

## Painel Admin

Acessar `/admin` com conta de administrador.

Funcionalidades:
- Dashboard com métricas e gráficos
- CRUD de produtos e categorias
- Controle de estoque com alertas
- Gestão de pedidos com atualização de status
- Análise de pedidos personalizados + envio de orçamentos
- Gestão de clientes e cupons
- Configurações da loja

## Mercado Pago

Para ativar os pagamentos:

1. Crie uma conta no [Mercado Pago Developers](https://www.mercadopago.com.br/developers)
2. Obtenha suas credenciais no painel
3. Configure no `.env`:
   ```
   MercadoPago__AccessToken=APP_USR-...
   MercadoPago__PublicKey=APP_USR-...
   MercadoPago__WebhookSecret=...
   MercadoPago__NotificationUrl=https://SEU_DOMINIO.com/api/payments/webhook
   ```
4. Para testes locais use ngrok para expor o webhook:
   ```bash
   ngrok http 5000
   # copie a URL HTTPS gerada para MercadoPago__NotificationUrl
   ```

## Armazenamento de imagens (produção)

Por padrão as imagens são salvas localmente em `/uploads` (pasta ignorada pelo git).

Para usar Cloudinary:
1. Configure `Upload__UseLocal=false` no `.env`
2. Preencha `Cloudinary__CloudName`, `Cloudinary__ApiKey` e `Cloudinary__ApiSecret`
