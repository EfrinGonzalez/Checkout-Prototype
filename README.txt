Checkout Prototype (.NET 9)
- TargetFramework: net9.0
- EF Core: 9.0.0
- Swagger + MediatR as before
Run: add all projects to a VS solution, set multiple startup (Shop.WebApi, Payments.WebApi, Orders.WebApi), F5.

Repository Overview
Platform and tooling
A .NET 9 prototype using EF Core 9, Swagger, and MediatR, intended to run three web APIs together in Visual Studio

Shared contracts
Shared.Contracts defines the DTOs exchanged between services—order creation, payment initiation, status updates, and read models for orders

Shop service
Manages catalog, basket, and checkout workflow. Commands and queries include adding/removing items and starting checkout, which coordinates order creation and payment initiation
The Web API wires up EF-backed repositories, HTTP clients to Orders and Payments, and seeds sample products at startup

Orders service
Handles creation, retrieval, and status updates of orders through MediatR commands/queries and an EF repository layer

Payments service
Initiates payments via a Nexi client and updates payment intent status (authorized, failed, captured). A webhook endpoint also relays payment outcomes back to the Orders service

Overall, the repository implements a modular checkout system where each service encapsulates its domain while communicating through shared contracts and HTTP-based integrations.
