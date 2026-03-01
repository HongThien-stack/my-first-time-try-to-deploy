# Product Service

Product Management Service for FSA - Bách Hóa Xanh grocery store system.

## Architecture

This project follows Clean Architecture principles with the following layers:

- **ProductService.Domain**: Core business entities and repository interfaces
- **ProductService.Application**: Business logic, DTOs, and service interfaces
- **ProductService.Infrastructure**: Data access implementations using Entity Framework Core
- **ProductService.API**: RESTful API endpoints

## Technology Stack

- .NET 9.0
- Entity Framework Core 9.0.1
- SQL Server
- Swagger/OpenAPI

## Database Setup

1. Create the database using the provided schema:
   ```bash
   sqlcmd -S localhost -i database-schema.sql
   ```

2. Update the connection string in `appsettings.json` if needed

## Running the Service

```bash
cd src/ProductService.API
dotnet run
```

The API will be available at: `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger`

## API Endpoints

### Products

- `GET /api/product` - Get all products

## Features (Current Implementation)

- ✅ Get all products with category information
- ⏳ Create product (to be implemented)
- ⏳ Update product (to be implemented)
- ⏳ Delete product (to be implemented)
- ⏳ Search and filter products (to be implemented)


## Database

The service uses two main tables:
- `categories`: Product categories (26 categories for Bách Hóa Xanh)
- `products`: Product catalog with detailed information

Sample data includes 8 products across different categories (vegetables, fruits, dairy, rice).
