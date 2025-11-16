# CRM with Social Media Integration

A modern CRM system built with .NET 9 Aspire that integrates with Instagram, Facebook, and WhatsApp for managing customer conversations across social media platforms.

## 🚀 Features

- **Instagram Integration**: Receive and send direct messages via Instagram Business API
- **Multi-Platform Support**: Designed to support Instagram, Facebook, and WhatsApp (Instagram implemented)
- **Unified Inbox**: Manage all conversations from different platforms in one place
- **Contact Management**: Automatic contact creation from social media interactions
- **Real-time Webhooks**: Receive messages instantly via webhooks
- **.NET 9 Aspire**: Cloud-ready architecture with built-in observability
- **PostgreSQL Database**: Reliable data storage with Entity Framework Core
- **RESTful API**: Clean API design with Swagger documentation

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL via Aspire)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- Facebook Developer Account (for Instagram Business API access)
- ngrok or similar tool (for local webhook testing)

## 🏗️ Architecture

The solution consists of three main projects:

- **CRM.AppHost**: Aspire orchestrator that manages all services
- **CRM.Api**: Main API with controllers, services, and database logic
- **CRM.ServiceDefaults**: Shared configurations for OpenTelemetry and health checks

## 📦 Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CRM
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Instagram API

#### Create a Facebook App

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app and select "Business" as the app type
3. Add the "Instagram" product to your app
4. Connect your Instagram Business Account

#### Get Your Credentials

You'll need:
- **App ID**: Found in App Settings > Basic
- **App Secret**: Found in App Settings > Basic
- **Page Access Token**: Generate from the Graph API Explorer
- **Verify Token**: Create your own random string (e.g., "my_verify_token_12345")

#### Update Configuration

Edit `CRM.Api/appsettings.json`:

```json
{
  "Instagram": {
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET",
    "VerifyToken": "YOUR_VERIFY_TOKEN",
    "PageAccessToken": "YOUR_PAGE_ACCESS_TOKEN"
  }
}
```

For production, use environment variables or Azure Key Vault instead.

## 🚀 Running the Application

### Option 1: Using Aspire (Recommended)

```bash
dotnet run --project CRM.AppHost
```

This will:
- Start the API service
- Launch PostgreSQL with pgAdmin
- Open the Aspire Dashboard with observability features
- Apply database migrations automatically

### Option 2: Running API Directly

```bash
cd CRM.Api
dotnet run
```

The API will be available at `https://localhost:7xxx` and Swagger UI at `https://localhost:7xxx/swagger`

## 🔗 Setting Up Webhooks

### Local Development with ngrok

1. **Install ngrok**:
   ```bash
   ngrok http https://localhost:7xxx
   ```

2. **Copy the HTTPS URL** (e.g., `https://abc123.ngrok.io`)

3. **Configure Webhook in Facebook**:
   - Go to your Facebook App Dashboard
   - Navigate to Products > Webhooks
   - Click "Edit Subscription" for Instagram
   - Callback URL: `https://abc123.ngrok.io/api/webhooks/instagram`
   - Verify Token: Use the same token from `appsettings.json`
   - Subscribe to `messages` field

4. **Test the Webhook**:
   - Send a message to your Instagram Business Account
   - Check the logs in your application

## 📡 API Endpoints

### Webhooks

- `GET /api/webhooks/instagram` - Webhook verification
- `POST /api/webhooks/instagram` - Receive Instagram messages

### Contacts

- `GET /api/contacts` - Get all contacts
- `GET /api/contacts/{id}` - Get contact by ID
- `POST /api/contacts` - Create new contact
- `PUT /api/contacts/{id}` - Update contact
- `DELETE /api/contacts/{id}` - Delete contact

### Messages

- `GET /api/messages/conversations` - Get all conversations
- `GET /api/messages/conversations/{id}` - Get conversation with messages
- `GET /api/messages/conversations/{conversationId}/messages` - Get messages
- `POST /api/messages/send` - Send a message
- `POST /api/messages/conversations/{conversationId}/read` - Mark as read

## 🗄️ Database Schema

### Tables

- **Contacts**: Customer information from all platforms
- **Conversations**: Platform-specific conversation threads
- **Messages**: Individual messages with metadata
- **InstagramAccounts**: Connected Instagram Business Accounts

### Relationships

- Contact → Many Conversations
- Conversation → Many Messages

## 🔧 Configuration

### Environment Variables (Production)

```bash
Instagram__AppId=YOUR_APP_ID
Instagram__AppSecret=YOUR_APP_SECRET
Instagram__VerifyToken=YOUR_VERIFY_TOKEN
Instagram__PageAccessToken=YOUR_PAGE_ACCESS_TOKEN
ConnectionStrings__crmdb=YOUR_DATABASE_CONNECTION_STRING
```

### Aspire Configuration

Aspire automatically configures:
- PostgreSQL database with connection string injection
- OpenTelemetry for distributed tracing
- Health checks
- Service discovery

## 📊 Monitoring

### Aspire Dashboard

Access the Aspire Dashboard (automatically opens on startup) to view:
- Live logs from all services
- Distributed traces
- Metrics
- Resource health

### Health Checks

- `/health` - Overall health status
- `/alive` - Liveness probe

## 🧪 Testing

### Test Webhook Verification

```bash
curl "https://localhost:7xxx/api/webhooks/instagram?hub.mode=subscribe&hub.challenge=TEST&hub.verify_token=YOUR_VERIFY_TOKEN"
```

Should return `TEST` if configured correctly.

### Send Test Message

Use Instagram app to send a message to your business account. Check:
1. Application logs for webhook receipt
2. Database for new contact/conversation/message records
3. Swagger UI to query the API

## 🔜 Roadmap

- [ ] Facebook Messenger integration
- [ ] WhatsApp Business API integration
- [ ] Web-based frontend UI
- [ ] Real-time notifications (SignalR)
- [ ] Message templates
- [ ] Auto-reply rules
- [ ] Analytics dashboard
- [ ] Multi-user support with authentication
- [ ] File/media upload handling
- [ ] Contact tagging and segmentation

## 📝 Instagram API Limitations

- Requires Instagram Business or Creator Account
- 24-hour messaging window for unsolicited messages
- Rate limits apply (check Facebook documentation)
- Webhook callbacks must respond within 20 seconds

## 🛠️ Development

### Adding New Migrations

```bash
cd CRM.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Removing Last Migration

```bash
dotnet ef migrations remove
```

## 📚 Technologies Used

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL
- Aspire (.NET Aspire)
- OpenTelemetry
- Swagger/OpenAPI

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 🆘 Troubleshooting

### Webhook Not Receiving Messages

1. Verify ngrok/tunnel is running and HTTPS URL is correct
2. Check webhook subscription in Facebook App Dashboard
3. Ensure verify token matches in both places
4. Check application logs for errors
5. Verify Instagram account is a Business Account

### Database Connection Issues

1. Ensure Docker Desktop is running
2. Check Aspire Dashboard for PostgreSQL status
3. Verify connection string in logs

### Authentication Errors with Instagram API

1. Verify Page Access Token is valid and not expired
2. Check App permissions include Instagram messaging
3. Ensure Instagram Business Account is connected to Facebook Page

## 📞 Support

For issues related to:
- Instagram API: Check [Facebook Developer Documentation](https://developers.facebook.com/docs/instagram-api)
- .NET Aspire: Check [Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- This project: Open an issue on GitHub

