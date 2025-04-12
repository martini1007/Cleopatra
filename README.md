# 💇 Cleopatra – Beauty Salon Support System

**Cleopatra** is a beauty salon support system: bookings, scheduling, materials (cosmetics and other resources), employees, customer profiles and history, handling of emergencies (booking transfers, schedule shifts), PDF reporting, responsive UI design, also adapted to mobile devices with small screens.

---

## ✨ Features

- 📅 **Bookings & Scheduling** – Manage appointments, calendar views, and staff availability.
- 👩‍💼 **Employee Management** – Track schedules, assign tasks, notify about changes.
- 🧴 **Resource Tracking** – Keep tabs on cosmetics and salon materials in real-time.
- 👩‍💻 **Customer Profiles & History** – View visit history and individual preferences.
- 🚨 **Emergency Handling** – Reassign bookings, notify users, manage last-minute changes.
- 📩 **Email Notifications** – Appointment reminders and schedule updates.
- 🧾 **PDF Reports** – Generate summaries
- 📱 **Responsive UI** – Fully adaptable layout for all screen sizes.

---

## 🏗️ Project Structure

```bash
cleopatra/
├── backend/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Cleopatra.sln
│   └── ...
├── frontend/
│   └── cleopatra-app/
│       ├── public/
│       ├── src/
│       │   ├── components/
│       │   ├── login/
│       │   ├── models/
│       │   └── config.json  ← 🔑 Google Maps API key here
│       └── package.json
```

## 🛠️ Tech Stack

### 🔹 Backend
- **Language:** C#
- **Framework:** ASP.NET Core
- **Database:** SQLite
- **ORM:** Entity Framework Core
- **Notifications:** MailKit


### 🔸 Frontend
- **Library:** React
- **Language:** JavaScript
- **Package Manager:** npm

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/cleopatra.git
cd cleopatra
```

### 2. Backend Setup (ASP.NET Core)
```bash
# Open the solution file using Visual Studio or JetBrains Rider
Cleopatra.sln
# Run the backend from within the IDE or using:
dotnet run
```

### 3. Frontend Setup (React)
```bash
# Go to the folder and write:
cd frontend/cleopatra-app
npm install

# Before running the app, create a configuration file for your Google Maps API key:
cd src
touch config.json

# Paste the following in config.json:
{
  "GOOGLE_API_KEY": "YOUR_API_KEY_HERE"
}

# Then start the frontend:
cd ..
npm start
```







