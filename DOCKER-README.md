# CRM Docker Setup

This guide shows how to run the CRM API with PostgreSQL using Docker Compose.

## 🚀 Quick Start

### Prerequisites
- Docker Desktop installed and running
- Docker Compose installed (included with Docker Desktop)

### 1. Start All Services

```bash
docker-compose up -d
```

This will start:
- **PostgreSQL** database on port `5432`
- **CRM API** on port `8080`
- **pgAdmin** (database UI) on port `5050`

### 2. Check Services Status

```bash
docker-compose ps
```

### 3. View Logs

```bash
# All services
docker-compose logs -f

# API only
docker-compose logs -f api

# Database only
docker-compose logs -f postgres
```

### 4. Access the Services

- **API Swagger UI**: http://localhost:8080/swagger
- **API Health Check**: http://localhost:8080/health
- **pgAdmin**: http://localhost:5050
  - Email: `admin@crm.com`
  - Password: `admin123`

## 📋 Available Commands

### Start services
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### Stop and remove volumes (deletes database data)
```bash
docker-compose down -v
```

### Rebuild API after code changes
```bash
docker-compose build api
docker-compose up -d api
```

### Restart a specific service
```bash
docker-compose restart api
```

### View service logs
```bash
docker-compose logs -f api
```

### Execute commands in running container
```bash
# Access API container shell
docker exec -it crm_api bash

# Access PostgreSQL
docker exec -it crm_postgres psql -U postgres -d crmdb
```

## 🗄️ Database Access

### Via pgAdmin (Web UI)
1. Open http://localhost:5050
2. Login with credentials above
3. Add new server:
   - **Name**: CRM Database
   - **Host**: postgres
   - **Port**: 5432
   - **Username**: postgres
   - **Password**: postgres123
   - **Database**: crmdb

### Via psql (Command Line)
```bash
docker exec -it crm_postgres psql -U postgres -d crmdb
```

### Via External Tool (DBeaver, DataGrip, etc.)
```
Host: localhost
Port: 5432
Database: crmdb
Username: postgres
Password: postgres123
```

## 🔧 Configuration

### Environment Variables

Edit `docker-compose.yml` to change:

**Database Configuration:**
```yaml
postgres:
  environment:
    POSTGRES_DB: crmdb
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres123
```

**API Configuration:**
```yaml
api:
  environment:
    - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=crmdb;Username=postgres;Password=postgres123
    - Instagram__AppId=YOUR_APP_ID
    - Instagram__AppSecret=YOUR_APP_SECRET
    - Instagram__VerifyToken=YOUR_VERIFY_TOKEN
    - Instagram__PageAccessToken=YOUR_PAGE_ACCESS_TOKEN
```

### Ports

Default ports:
- API: `8080`
- PostgreSQL: `5432`
- pgAdmin: `5050`

To change ports, edit `docker-compose.yml`:
```yaml
ports:
  - "8080:8080"  # Change first number: "YOUR_PORT:8080"
```

## 🧪 Testing

### Health Check
```bash
curl http://localhost:8080/health
```

### Test Instagram Webhook Verification
```bash
curl "http://localhost:8080/api/webhooks/instagram?hub.mode=subscribe&hub.challenge=TEST&hub.verify_token=Tekron"
```

Should return: `TEST`

### Get All Contacts
```bash
curl http://localhost:8080/api/contacts
```

### Get All Conversations
```bash
curl http://localhost:8080/api/messages/conversations
```

## 🔄 Database Migrations

Migrations run automatically when the API starts. To manually run migrations:

```bash
# From the project root
cd CRM.Api
dotnet ef database update
```

## 🐛 Troubleshooting

### API won't start
1. Check if PostgreSQL is healthy:
   ```bash
   docker-compose logs postgres
   ```
2. Check API logs:
   ```bash
   docker-compose logs api
   ```

### Database connection errors
1. Verify PostgreSQL is running:
   ```bash
   docker-compose ps postgres
   ```
2. Check connection string in `docker-compose.yml`

### Port already in use
```bash
# Find what's using the port
netstat -ano | findstr :8080  # Windows
lsof -i :8080  # Mac/Linux

# Change the port in docker-compose.yml
```

### Reset everything
```bash
# Stop and remove all containers, networks, and volumes
docker-compose down -v

# Start fresh
docker-compose up -d
```

## 📊 Development Workflow

### 1. Make code changes
Edit files in `CRM.Api/`

### 2. Rebuild and restart
```bash
docker-compose build api
docker-compose up -d api
```

### 3. Watch logs
```bash
docker-compose logs -f api
```

## 🔐 Production Considerations

For production deployment:

1. **Change default passwords** in `docker-compose.yml`
2. **Use environment variables** or secrets management
3. **Enable HTTPS** with reverse proxy (nginx/traefik)
4. **Set up persistent volumes** for database backups
5. **Configure health checks** for monitoring
6. **Use production-ready connection strings**
7. **Remove pgAdmin** or secure it properly

### Production docker-compose.yml Example

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: always

  api:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
      - Instagram__AppId=${INSTAGRAM_APP_ID}
      - Instagram__AppSecret=${INSTAGRAM_APP_SECRET}
      - Instagram__VerifyToken=${INSTAGRAM_VERIFY_TOKEN}
      - Instagram__PageAccessToken=${INSTAGRAM_PAGE_ACCESS_TOKEN}
    restart: always
```

Create `.env` file:
```env
DB_NAME=crmdb
DB_USER=postgres
DB_PASSWORD=your_secure_password
CONNECTION_STRING=Host=postgres;Port=5432;Database=crmdb;Username=postgres;Password=your_secure_password
INSTAGRAM_APP_ID=your_app_id
INSTAGRAM_APP_SECRET=your_app_secret
INSTAGRAM_VERIFY_TOKEN=your_verify_token
INSTAGRAM_PAGE_ACCESS_TOKEN=your_page_access_token
```

## 📚 Additional Resources

- **Docker Compose**: https://docs.docker.com/compose/
- **PostgreSQL Docker**: https://hub.docker.com/_/postgres
- **ASP.NET Core Docker**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/
- **pgAdmin Docker**: https://www.pgadmin.org/docs/pgadmin4/latest/container_deployment.html

