# Smart Home Control Panel (IoT Simulator) 🏠🎛️

An academic project developed for the Software Engineering course (Semester 5) at the Silesian University of Technology. This application serves as a central Smart Home control panel, allowing users to add, monitor, and comprehensively manage simulated IoT devices. 

Together with the associated Together with the associated [IoT Device Simulator](https://github.com/akubek/IO-projekt-symulator), the system forms a fully distributed architecture communicating via a RabbitMQ message broker., the system forms a fully distributed architecture communicating via a RabbitMQ message broker.

## 👥 Team & Roles

The project was developed as a group effort and is divided into two main parts: the Control Panel (this repository) and the IoT Device Simulator.

**Control Panel Development Team:**
* **Artur Kubek** – **Lead Developer** (Architecture, integration, full-stack development of the panel)
* **Paweł Litwiński** – **Co-developer** (Panel)
* **Jarosław Makówka** – **Co-developer** (Requirements analysis and initial system specification; contributed during the early phases before leaving for an Erasmus exchange)

**System Co-authors (Primarily responsible for the IoT Device Simulator):**
* **Jakub Tywonek** – Simulator frontend and UI development
* **Dominika Maćkowiak** – Simulator backend architecture, RabbitMQ, and SignalR integration

---

## 🚀 Tech Stack

### Backend
* **Framework:** ASP.NET Core Web API
* **Database:** SQLite with Entity Framework Core
* **Real-time Communication:** SignalR
* **Message Brokering:** RabbitMQ via MassTransit
* **Authentication:** ASP.NET Core Identity & JWT Bearer
* **Testing:** xUnit and Moq (for unit and integration testing of business logic)

### Frontend
* **Framework:** React (Vite)
* **State Management & Fetching:** TanStack React Query
* **Real-time Client:** `@microsoft/signalr`

---

## 🏗️ Architecture & Key Features

The system relies on a decoupled client-server architecture.

* **Device Management:** The panel provides a GUI to register devices, assign them to rooms, and control virtual parameters (switches, sliders, read-only sensors).
* **Asynchronous Communication (RabbitMQ):** Utilizing RabbitMQ, the panel securely sends asynchronous commands (e.g., `SetDeviceStateCommand`) to the simulator and listens for updates, mimicking real industrial IoT systems.
* **Real-time Telemetry (SignalR):** To eliminate polling, the system implements push notifications via WebSockets. When a simulated device changes state, the backend broadcasts this event, and the React client instantly refreshes the UI.
* **Automations & Scenes:** Administrators can create "Scenes" to execute multiple actions simultaneously, or define "Automations" based on logic triggers and specific time windows.
* **Device Malfunction Simulation:** The system supports a "malfunction" state where devices stop responding to commands, requiring the user to resolve the issue on the simulator side.
* **Virtual Time Configuration:** The system clock can be overridden in the panel to thoroughly test time-based automations without waiting for real-time events.

---

## ⚙️ Running Locally (Disclaimer)

**Note:** This is an academic Proof-of-Concept project. While it is functional, running it locally requires manual setup and might necessitate some configuration adjustments. 

**Note:** To run the full system and see the real-time interactions, you will also need to clone and run the [IoT Device Simulator](https://github.com/akubek/IO-projekt-symulator).

### Prerequisites
1. [.NET 7.0+ SDK](https://dotnet.microsoft.com/download)
2. [Node.js](https://nodejs.org/) (v18+)
3. [Docker](https://www.docker.com/) (Required for the RabbitMQ broker)

### 1. Start RabbitMQ
A running RabbitMQ instance is mandatory for the microservices to communicate.
```bash
docker run -d --hostname rmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 2. Configure & Run the Backend
The server relies on a `panel.ini` file for runtime configuration (e.g., RabbitMQ host, API URLs, and default Admin credentials).
```bash
cd IO_Panel.Server
# The SQLite database is generated automatically
dotnet restore
dotnet run
```

### 3. Run the Frontend
```bash
cd io_panel.client
npm install
npm run dev
```

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
