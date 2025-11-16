# CRM Architecture Documentation

## Project Structure

```
CRM/
├── CRM.AppHost/                    # Aspire orchestrator
│   ├── Program.cs                  # Service orchestration
│   └── CRM.AppHost.csproj
│
├── CRM.Api/                        # Main API project
│   ├── Controllers/
│   │   ├── ContactsController.cs   # Contact CRUD operations
│   │   ├── MessagesController.cs   # Message & conversation management
│   │   └── InstagramWebhookController.cs  # Instagram webhook handler
│   │
│   ├── Services/
│   │   ├── IInstagramService.cs    # Instagram service interface
│   │   └── InstagramService.cs     # Instagram API integration
│   │
│   ├── Models/
│   │   ├── Contact.cs              # Customer contact entity
│   │   ├── Conversation.cs         # Conversation thread entity
│   │   ├── Message.cs              # Individual message entity
│   │   └── InstagramAccount.cs     # Connected Instagram account
│   │
│   ├── Data/
│   │   └── CrmDbContext.cs         # EF Core database context
│   │
│   ├── DTOs/Instagram/
│   │   ├── InstagramWebhookDto.cs  # Webhook payload DTOs
│   │   └── SendMessageDto.cs       # Send message DTOs
│   │
│   ├── Migrations/                 # EF Core migrations
│   ├── Program.cs                  # API startup & configuration
│   └── appsettings.json            # Configuration file
│
├── CRM.ServiceDefaults/            # Shared Aspire configurations
│   ├── Extensions.cs               # OpenTelemetry & health checks
│   └── CRM.ServiceDefaults.csproj
│
├── CRM.sln                         # Solution file
├── README.md                       # Setup instructions
├── ARCHITECTURE.md                 # This file
└── global.json                     # SDK version
```

## Technology Stack

### Backend Framework
- **.NET 9**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core 9**: ORM for database operations
- **.NET Aspire**: Cloud-native orchestration

### Database
- **PostgreSQL**: Primary database
- **pgAdmin**: Database management (via Aspire)

### Observability
- **OpenTelemetry**: Distributed tracing & metrics
- **Aspire Dashboard**: Real-time monitoring

### External APIs
- **Instagram Graph API v18.0**: Instagram Business messaging
- **Facebook Graph API**: Webhook integration

## Data Flow

### Receiving Instagram Messages

```
Instagram User → Instagram API → Webhook (POST /api/webhooks/instagram)
    ↓
InstagramWebhookController.ReceiveWebhook()
    ↓
ProcessMessage() - Extract message data
    ↓
GetOrCreateContact() - Find or create contact
    ↓
GetOrCreateConversation() - Find or create conversation thread
    ↓
Save Message to Database
    ↓
Update Conversation & Contact timestamps
```

### Sending Instagram Messages

```
Client → POST /api/messages/send
    ↓
MessagesController.SendMessage()
    ↓
InstagramService.SendMessage() → Instagram Graph API
    ↓
Save Outbound Message to Database
    ↓
Update Conversation timestamp
```

## Database Schema

### Contact Table
- Stores customer information across all platforms
- Indexed on Instagram ID, Facebook ID, Email
- Links to multiple conversations

### Conversation Table
- Represents a messaging thread on a specific platform
- Links one contact to many messages
- Tracks unread count and status (Active/Archived/Closed)

### Message Table
- Individual messages with full content
- Supports text, images, videos, audio, files
- Direction: Inbound (received) or Outbound (sent)
- Indexed on platform message ID and timestamp

### InstagramAccount Table
- Stores connected Instagram Business Accounts
- Manages access tokens and expiry
- Unique constraint on Instagram Business Account ID

## Instagram Webhook Flow

### 1. Webhook Verification (GET Request)
- Facebook/Instagram verifies webhook endpoint
- Checks verify token against configuration
- Returns challenge string if valid

### 2. Message Reception (POST Request)
- Receives JSON payload with message data
- Validates webhook object type (instagram/page)
- Processes each entry and messaging event
- Skips echo messages (sent by bot)
- Creates/updates contact, conversation, message records

### 3. User Info Enrichment
- Fetches Instagram user details via Graph API
- Updates contact with username and profile info
- Falls back to generic name if API call fails

## API Endpoints Reference

### Webhooks
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/webhooks/instagram` | Webhook verification |
| POST | `/api/webhooks/instagram` | Receive messages |

### Contacts
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/contacts` | List all contacts |
| GET | `/api/contacts/{id}` | Get contact details |
| POST | `/api/contacts` | Create contact |
| PUT | `/api/contacts/{id}` | Update contact |
| DELETE | `/api/contacts/{id}` | Delete contact |

### Messages & Conversations
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/messages/conversations` | List all conversations |
| GET | `/api/messages/conversations/{id}` | Get conversation with messages |
| GET | `/api/messages/conversations/{conversationId}/messages` | Get messages |
| POST | `/api/messages/send` | Send a message |
| POST | `/api/messages/conversations/{conversationId}/read` | Mark as read |

## Configuration Management

### Required Configuration
```json
{
  "Instagram": {
    "AppId": "Facebook App ID",
    "AppSecret": "Facebook App Secret",
    "VerifyToken": "Custom webhook verify token",
    "PageAccessToken": "Instagram/Facebook Page Access Token"
  }
}
```

### Security Best Practices
1. **Never commit secrets** to source control
2. Use **environment variables** in production
3. Consider **Azure Key Vault** for secret management
4. Rotate **access tokens** before expiry
5. Use **HTTPS only** for webhooks

## Aspire Integration

### Service Orchestration
- **AppHost** manages all services
- **Automatic service discovery**
- **Health monitoring** for all components

### Database Management
- PostgreSQL container automatically provisioned
- Connection string injected via Aspire
- Migrations applied on startup

### Observability Features
- Distributed tracing across services
- Live logs with filtering
- Performance metrics
- Resource health dashboard

## Extension Points

### Adding New Platforms

1. **Create Platform-Specific DTOs**
   - Webhook payload DTOs
   - Send message DTOs

2. **Implement Platform Service**
   - Create `IFacebookService` or `IWhatsAppService`
   - Implement message send/receive logic

3. **Add Webhook Controller**
   - Create controller for platform webhooks
   - Handle verification and message reception

4. **Update Models**
   - Add platform-specific fields to Contact
   - Use existing Conversation and Message models

5. **Register Services**
   - Add to dependency injection in `Program.cs`

### Future Enhancements

- **SignalR**: Real-time notifications to frontend
- **Background Jobs**: Message queue processing
- **Redis**: Caching and session management
- **Azure Functions**: Serverless webhook handlers
- **AI Integration**: Chatbot auto-replies
- **Analytics**: Message insights and reporting

## Performance Considerations

### Database Indexing
- Composite indexes on frequently queried fields
- Platform-specific IDs indexed for fast lookups
- Timestamp indexes for chronological queries

### Webhook Processing
- Must respond within 20 seconds (Facebook requirement)
- Async processing for heavy operations
- Return 200 OK immediately to avoid retries

### API Rate Limiting
- Instagram/Facebook have rate limits
- Implement retry logic with exponential backoff
- Cache user info to reduce API calls

## Security Considerations

### Webhook Validation
- Verify token on GET requests
- Validate request signatures (recommended)
- Use HTTPS endpoints only

### Data Protection
- Encrypt sensitive data at rest
- Use secure connection strings
- Implement proper CORS policies

### Authentication
- API endpoints currently open (add authentication)
- Consider JWT tokens for frontend
- Role-based access control for multi-user

## Testing Strategy

### Unit Tests
- Service layer logic
- Message processing
- Contact management

### Integration Tests
- Webhook endpoint verification
- Database operations
- External API calls (mocked)

### End-to-End Tests
- Full message flow
- Multi-platform scenarios
- Error handling

## Deployment

### Azure Deployment
1. Deploy PostgreSQL (Azure Database for PostgreSQL)
2. Deploy API (Azure App Service or Container Apps)
3. Configure Aspire for Azure
4. Set up webhook URL in Facebook

### Docker Deployment
1. Build Docker images
2. Use Docker Compose for multi-container
3. Configure reverse proxy (nginx)
4. Set up SSL certificates

## Monitoring & Logging

### Key Metrics
- Message throughput
- Webhook response time
- API error rates
- Database query performance

### Log Levels
- **Information**: Normal operations
- **Warning**: Degraded performance
- **Error**: Failures requiring attention
- **Debug**: Development troubleshooting

## Troubleshooting Guide

### Webhook Not Working
1. Check ngrok/tunnel is running
2. Verify token matches configuration
3. Review Facebook webhook logs
4. Check application logs

### Messages Not Saving
1. Verify database connection
2. Check EF migrations applied
3. Review error logs
4. Validate JSON payload structure

### Instagram API Errors
1. Verify access token validity
2. Check API permissions
3. Review rate limiting
4. Ensure Business Account connected

