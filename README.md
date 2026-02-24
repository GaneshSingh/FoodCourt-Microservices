# 🍔 Food Court: Cloud-Native Microservices Architecture

An enterprise-grade, zero-trust microservices application built with **.NET 8** and deployed on **Azure Container Apps**. This project demonstrates modern distributed system design, strict network isolation, automated CI/CD monorepo pipelines, and secure API Gateway routing.

## 🏗️ System Architecture

This system implements a fully decoupled architecture utilizing the API Gateway pattern to protect internal services. 

* **Public Ingress:** Only the `ApiGateway` and `AuthService` are exposed to the public internet.
* **Private Network:** The `OrderService` and `ItemService` are isolated within an internal Azure Virtual Network (VNet) and are completely inaccessible from the outside.
* **Serverless Compute:** Deployed on Azure Container Apps, allowing individual microservices to scale to zero to optimize cloud consumption costs.
* **State Management:** Backed by a managed Azure SQL Database.

## 🔐 Zero-Trust Security & JWT Authentication

Security is enforced at the perimeter using a dedicated Identity Provider:
1.  **AuthService:** Validates credentials and mints cryptographically signed JSON Web Tokens (JWT) with Role-Based Access Control (RBAC) claims.
2.  **Ocelot API Gateway:** Acts as the system "Bouncer." It intercepts all incoming public requests, cryptographically verifies the JWT signature, and instantly drops unauthorized traffic (HTTP 401) before it can reach the internal network.

## ⚙️ Monorepo CI/CD & DevOps (GitHub Actions)

The repository utilizes an advanced monorepo CI/CD pipeline using GitHub Actions with strict **Path Filtering**.

* **Decoupled Deployments:** A code commit to one service (e.g., `OrderService`) triggers only that specific service's build pipeline. The rest of the system remains untouched, drastically reducing compute waste and build times.
* **Automated Pipeline:** * Provisions an Ubuntu build agent.
    * Authenticates with Azure using a secure Service Principal stored in GitHub encrypted secrets.
    * Builds the Docker container and tags it dynamically using the GitHub Run Number.
    * Pushes the artifact to Azure Container Registry (ACR).
    * Performs a zero-downtime hot-swap deployment to Azure Container Apps.

## 🧰 Technology Stack

* **Backend Framework:** .NET 8 (C#)
* **API Gateway:** Ocelot
* **Security:** JWT (JSON Web Tokens)
* **Containerization:** Docker
* **Cloud Provider:** Microsoft Azure (Container Apps, Container Registry, SQL Database)
* **CI/CD:** GitHub Actions

## 📦 Microservices Breakdown

| Service | Port | Access | Responsibility |
| :--- | :--- | :--- | :--- |
| **AuthService** | 8080 | Public | Identity Provider, validates users, and issues JWTs. |
| **ApiGateway** | 8080 | Public | Ocelot Gateway routing, JWT validation, and load balancing. |
| **OrderService** | 8080 | Private (Internal DNS) | Manages customer orders and orchestrates calls to the ItemService. |
| **ItemService** | 8080 | Private (Internal DNS) | Inventory management; interfaces directly with Azure SQL Database. |