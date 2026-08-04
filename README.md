# 🌍 Impilo Yesizwe NGO Management System

An ASP.NET Core MVC web application developed for **Impilo Yesizwe**, a non-profit organization. The system provides an online platform for visitors to learn about the NGO, make donations, contact the organization, and allows administrators to manage website content through a secure admin panel.

---

## 📌 Features

- 🏠 Home page
- ℹ️ About Us page
- 🛠️ Services page
- 🖼️ Gallery
- ❤️ Online Donations
- 📩 Contact Form
- 🔐 Secure Admin Login
- 👤 Admin User Management
- 💾 SQL Server Database
- 🐳 Docker & Docker Compose Support

---

## 🛠️ Technologies Used

- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core
- SQL Server 2022
- HTML5
- CSS3
- Bootstrap
- JavaScript
- Docker
- Docker Compose
- Git & GitHub

---

## 📂 Project Structure

```
ImpiloYesizweProject
│
├── Controllers
├── Models
├── Views
├── Data
├── wwwroot
├── Migrations
├── Dockerfile
├── docker-compose.yml
├── Program.cs
└── appsettings.json
```

---

## 🚀 Running the Project

### Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/ImpiloYesizweProject.git
```

Navigate into the project

```bash
cd ImpiloYesizweProject
```

Build and start the containers

```bash
docker compose up --build
```

Open your browser

```
http://localhost:8080
```

---

## 🗄️ Database

The project uses **SQL Server** with **Entity Framework Core**.

Database tables are automatically created using EF Core Migrations when the application starts.

---

## 🔑 Default Admin Login

> Username: **admin**

> Password: **Admin@123**

*(Change these credentials before deploying to production.)*

---

## 📷 Screenshots

Add screenshots of:

- Home Page
- About Page
- Services
- Gallery
- Donations
- Contact Page
- Admin Dashboard

---

## 🐳 Docker

This project is fully containerized using Docker.

Services include:

- ASP.NET Core MVC Web Application
- SQL Server 2022 Database

Run with:

```bash
docker compose up --build
```

Stop with:

```bash
docker compose down
```

---

## 👨‍💻 Developer

**Owami T. Msane**

Diploma in Software Development

Passionate about ASP.NET Core, SQL Server, Docker, and Full-Stack Web Development.

GitHub:
https://github.com/YOUR_USERNAME

---

## 📄 License

This project was developed for educational and portfolio purposes.
