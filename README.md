# Mini Amazon Clone

Mini Amazon Clone is a simplified e-commerce API built with ASP.NET Core. It supports user authentication, product management, order creation, and retrieval, with a mix of Dapper and Entity Framework Core for data access. The project includes unit tests for the `OrderRepository` using Moq and xUnit.

## Features
- **Authentication**: JWT-based login and registration.
- **Authorization**: Role-based access (e.g., Admin) and custom policies (e.g., `CanViewOrdersPolicy`).
- **Product Management**: CRUD operations for products (Admin only).
- **Order Management**: Create, retrieve, and refund orders, with eager loading and Dapper-based queries.
- **Unit Tests**: Tests for `OrderRepository` using Moq for mocking database interactions.

## Prerequisites
Before setting up the project, ensure you have the following installed:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or the version matching your project)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express/LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional, for IDE support) or any code editor (e.g., VS Code)
- [Postman](https://www.postman.com/downloads/) (optional, for testing API endpoints)
- [Git](https://git-scm.com/downloads) (for cloning the repository)

## Project Structure
- **Mini_Amazon_Clone**: Main API project with controllers, models, repositories, and configuration.
- **Mini_Amazon_Clone.Tests**: Unit test project for `OrderRepository`.

## Setup Instructions

### 1. Clone the Repository
Clone the project to your local machine:
```bash
git clone https://github.com/your-username/mini-amazon-clone.git
cd mini-amazon-clone
```

### 2. Configure the Database
The project uses SQL Server with Entity Framework Core and Dapper. Follow these steps to set up the database:

#### a. Update Connection String
Edit the `appsettings.json` file in the `Mini_Amazon_Clone` directory:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server_name;Database=MiniAmazonClone;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSecretKeyHereAtLeast16Chars",
    "Issuer": "MiniAmazonClone",
    "Audience": "MiniAmazonClone"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
- Replace `your_server_name` with your SQL Server instance (e.g., `(localdb)\\MSSQLLocalDB` for LocalDB or `.` for default SQL Server).
- Ensure the `Jwt:Key` is at least 16 characters long.

#### b. Create the Database
1. Open a terminal in the `Mini_Amazon_Clone` directory.
2. Apply Entity Framework Core migrations to create the database and tables:
   ```bash
   dotnet ef migrations add InitialCreate --project Mini_Amazon_Clone
   dotnet ef database update --project Mini_Amazon_Clone
   ```
   - This creates the `MiniAmazonClone` database with tables like `Users`, `Products`, `Orders`, and `OrderItems`.

#### c. Seed Initial Data (Optional)
You can manually insert test data into the database using SQL Server Management Studio (SSMS) or a script. Example:
```sql
INSERT INTO Users (Email, Password, Name, Role)
VALUES ('admin@gmail.com', '$2a$11$hashedpasswordhere', 'Admin User', 'Admin');

INSERT INTO Products (Name, Description, Price, Stock, CreatedBy)
VALUES ('Product A', 'Description A', 50.00, 10, 1);
```

### 3. Install Dependencies
Restore the NuGet packages for both the main project and the test project:
```bash
dotnet restore Mini_Amazon_Clone
dotnet restore Mini_Amazon_Clone.Tests
```

Required packages (already included in `.csproj` files):
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Dapper`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore` (for Swagger)
- `Moq` (in test project)
- `xunit` (in test project)

### 4. Build the Project
Build both projects to ensure there are no compilation errors:
```bash
dotnet build Mini_Amazon_Clone
dotnet build Mini_Amazon_Clone.Tests
```

### 5. Run the API
Run the main API project:
```bash
cd Mini_Amazon_Clone
dotnet run
```
- The API will start on `https://localhost:7228` (or the port specified in `launchSettings.json`).
- Open Swagger UI at `https://localhost:7228/swagger` to explore and test endpoints.

### 6. Test the API
Use Postman or Swagger to test the endpoints:
- **Register a User**: `POST /api/auth/register`
  ```json
  {
    "email": "test@example.com",
    "password": "password123",
    "name": "Test User",
    "role": "User"
  }
  ```
- **Login**: `POST /api/auth/login`
  ```json
  {
    "email": "test@example.com",
    "password": "password123"
  }
  ```
  - Copy the returned JWT token for authenticated requests.
- **Get Orders**: `GET /api/orders/customer/{userId}` (replace `{userId}` with the user’s ID from the token).

Add the `Authorization` header with the JWT token for protected endpoints (e.g., `Bearer <token>`).

### 7. Run Unit Tests
Run the unit tests for `OrderRepository`:
```bash
cd Mini_Amazon_Clone.Tests
dotnet test
```
- Expected output: Two passing tests (`GetOrdersByUserId_ReturnsOrdersForUser` and `AddOrder_AddsOrderSuccessfully`).

## Configuration Notes
- **JWT Authentication**: Ensure `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` in `appsettings.json` match your token generation logic.
- **Database**: The project uses both EF Core (for most operations) and Dapper (in `OrderRepository` and `ProductRepository`). Ensure the connection string is valid.
- **Logging**: Console logging is enabled by default.

## Troubleshooting
- **Database Connection Errors**: Verify the connection string and SQL Server accessibility.
- **JWT Errors**: Check the `Jwt:Key` length (minimum 16 characters) and token claims.
- **Test Failures**: Ensure the `Order` model matches the test data (e.g., `OrderItems` as `ICollection<OrderItem>` works with `First()`).

## Contributing
Feel free to submit issues or pull requests to improve the project!

## License
This project is for educational purposes and has no explicit license.

