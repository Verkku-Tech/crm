# Quick Start Guide

## Prerequisites Check
```bash
# Check .NET version
dotnet --version
# Should be 9.0.x or higher

# Check Docker
docker --version
```

## 1️⃣ Initial Setup (5 minutes)

### Restore packages
```bash
dotnet restore
```

### Configure Instagram API
Edit `CRM.Api/appsettings.json`:
```json
{
  "Instagram": {
    "VerifyToken": "my_secret_token_12345",
    "PageAccessToken": "YOUR_INSTAGRAM_PAGE_ACCESS_TOKEN"
  }
}
```

## 2️⃣ Run the Application (2 minutes)

### Start with Aspire (Recommended)
```bash
dotnet run --project CRM.AppHost
```

This will:
- ✅ Start PostgreSQL database
- ✅ Apply migrations automatically
- ✅ Launch the API
- ✅ Open Aspire Dashboard
- ✅ Open Swagger UI

### Or run API directly
```bash
cd CRM.Api
dotnet run
```

## 3️⃣ Test the API (1 minute)

Open Swagger UI: `https://localhost:7000/swagger`

### Test endpoints:
1. **GET** `/api/contacts` - Should return empty array
2. **GET** `/api/messages/conversations` - Should return empty array

## 4️⃣ Setup Instagram Webhooks (10 minutes)

### Install ngrok (for local testing)
```bash
# Download from: https://ngrok.com/download
ngrok http https://localhost:7000
```

### Copy your ngrok URL
Example: `https://abc123.ngrok-free.app`

### Configure in Facebook Developer Console

1. Go to: https://developers.facebook.com/
2. Select your app
3. Products → Instagram → Configuration
4. Webhooks section:
   - **Callback URL**: `https://abc123.ngrok-free.app/api/webhooks/instagram`
   - **Verify Token**: `my_secret_token_12345` (from appsettings.json)
   - Click "Verify and Save"
5. Subscribe to: `messages` field

## 5️⃣ Test Instagram Integration (2 minutes)

### Send test message:
1. Open Instagram app
2. Send a DM to your Instagram Business Account
3. Check your application logs

### Verify in database:
1. Open pgAdmin (link in Aspire Dashboard)
2. Check tables: `contacts`, `conversations`, `messages`

### Or use API:
```bash
# Get all conversations
curl https://localhost:7000/api/messages/conversations

# Get all contacts
curl https://localhost:7000/api/contacts
```

## 🎉 You're Ready!

### What's working:
- ✅ Instagram message receiving
- ✅ Automatic contact creation
- ✅ Conversation management
- ✅ Message history
- ✅ Send messages via API

### Next steps:
- Build a frontend UI
- Add Facebook Messenger integration
- Add WhatsApp integration
- Implement auto-replies
- Add user authentication

## 🆘 Quick Troubleshooting

### Problem: "Cannot connect to database"
**Solution**: Make sure Docker Desktop is running

### Problem: "Webhook verification failed"
**Solution**: Check that verify token in Facebook matches appsettings.json

### Problem: "Migration not found"
**Solution**: Run `cd CRM.Api && dotnet ef migrations add InitialCreate`

### Problem: "Instagram API returns 403"
**Solution**: 
- Verify your access token is valid
- Check your Instagram account is a Business Account
- Ensure your Facebook App has Instagram permissions

## 📱 Instagram Requirements

### Before using Instagram messaging:
1. ✅ Facebook Developer Account
2. ✅ Facebook App created
3. ✅ Instagram Business Account (not personal)
4. ✅ Instagram account connected to Facebook Page
5. ✅ App reviewed for `instagram_manage_messages` permission (for production)

### Development Mode:
- Can test with your own Instagram Business Account
- No app review needed
- Limited to accounts connected to your app

## 🔗 Useful Links

- **Swagger UI**: https://localhost:7000/swagger
- **Aspire Dashboard**: https://localhost:XXXXX (opens automatically)
- **pgAdmin**: Available via Aspire Dashboard
- **Facebook Developers**: https://developers.facebook.com/
- **Instagram API Docs**: https://developers.facebook.com/docs/instagram-api

## 📊 Database Access

### Via pgAdmin (Aspire):
1. Click "pgAdmin" link in Aspire Dashboard
2. Default credentials shown in Aspire
3. Browse: Servers → postgres → Databases → crmdb

### Via connection string:
```
Host=localhost;Port=5432;Database=crmdb;Username=postgres;Password=<from-aspire>
```

## 🧪 Testing Webhooks Locally

### Test verification endpoint:
```bash
curl "https://localhost:7000/api/webhooks/instagram?hub.mode=subscribe&hub.challenge=TEST&hub.verify_token=my_secret_token_12345"
```
Should return: `TEST`

### Send test message via API:
```bash
curl -X POST https://localhost:7000/api/messages/send \
  -H "Content-Type: application/json" \
  -d '{
    "conversationId": "YOUR_CONVERSATION_ID",
    "message": "Hello from API!"
  }'
```

## 🚀 Production Deployment

### Azure App Service:
```bash
# Publish
dotnet publish CRM.Api/CRM.Api.csproj -c Release

# Deploy to Azure
az webapp up --name your-app-name --resource-group your-rg
```

### Update webhook URL in Facebook:
Change from ngrok URL to your production URL:
`https://your-app.azurewebsites.net/api/webhooks/instagram`

### Set environment variables in Azure:
```bash
az webapp config appsettings set --name your-app-name --resource-group your-rg \
  --settings Instagram__PageAccessToken="YOUR_TOKEN" \
              Instagram__VerifyToken="YOUR_VERIFY_TOKEN"
```

## 📚 Learn More

- **README.md**: Comprehensive documentation
- **ARCHITECTURE.md**: Technical architecture details
- **.NET Aspire**: https://learn.microsoft.com/en-us/dotnet/aspire/
- **Instagram Graph API**: https://developers.facebook.com/docs/graph-api/

---

**Need help?** Check the README.md or open an issue on GitHub.

