# SnapShop - E-Commerce Web API

## Overview  
SnapShop is a scalable, secure, and high-performance RESTful API built with ASP.NET Core for managing an e-commerce platform.  
It includes advanced features like caching, secure authentication, payment integration, and flexible design patterns to ensure clean, maintainable code.

## Features  
- Product management (CRUD)  
- Order processing  
- Customer management  
- Authentication & Authorization with **Identity & JWT tokens**  
- Payment integration with **Stripe**  
- Caching using **Redis** to improve performance  
- Clean architecture using **Specification Design Pattern**  
- Secure token management and refresh tokens  

## Technologies Used  
- ASP.NET Core  
- Entity Framework Core  
- SQL Server (or your preferred database)  
- Redis caching  
- JWT Authentication & Identity  
- Specification Design Pattern  
- Stripe Payment Gateway  

## Getting Started  

### Prerequisites  
- .NET SDK installed  
- Redis Server running (locally or remotely)  
- SQL Server or compatible database  
- IDE (Visual Studio, VS Code)  

### Running the Project  
1. Clone the repository:  
   ```bash
   git clone https://github.com/your-username/SnapShop.git

2. Navigate to the project folder:
cd SnapShop
3. Restore dependencies and build:
dotnet restore
dotnet build
4. Configure your appsettings.json for database connection, Redis, JWT keys, and Stripe payment settings.
5. Run the API:
dotnet run
ntend and CORS Configuration
The frontend Angular project runs separately and communicates with this API backend.
Make sure to configure CORS in the API to allow requests from your Angular app domain, for example:
services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        builder => builder.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
app.UseCors("AllowAngularClient");
Payment Processing and Testing
Payments are integrated using Stripe. Before setting up webhooks, you can test the payment cycle manually in Stripe test mode using test cards.
Check API responses and logs to verify order status updates after payment confirmation.

Stripe Webhook Integration and Local Server Setup
To handle real-time payment events, implement a Stripe webhook endpoint in the API.
Use tools like ngrok to expose your local development server to the internet for testing webhooks.

Steps to connect your local server to Stripe:

1. Run your local API (e.g., on port 5000).
2. Start ngrok tunneling your port:
ngrok http 5000
3. Copy the HTTPS URL provided by ngrok.
4. Add this URL as the webhook endpoint in the Stripe Dashboard.
5. Handle webhook events such as payment_intent.succeeded in your API to update orders accordingly.

Contribution
Contributions are welcome! Feel free to fork the repository and submit pull requests.

License
This project is licensed under the MIT License.


