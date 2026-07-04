# itisrajya-api

Backend API powering the interactive terminal portfolio available at **https://itisrajya.net**.

The API provides a lightweight real-time chat system that allows visitors to communicate directly with the portfolio owner through a terminal-style interface.

---

## Features

- ASP.NET Core Minimal API
- SignalR real-time communication
- Live two-way chat
- Email notification on new chat session
- Automatic session expiration after 60 seconds of inactivity
- JSON-based session storage
- Automatic timer reset on every message
- Automatic cleanup of expired sessions
- CORS support for Angular frontend
- Swagger/OpenAPI documentation

---

## Tech Stack

- ASP.NET Core 9
- SignalR
- Minimal APIs
- System.Text.Json
- SMTP (Gmail)
- Swagger

---

## Project Structure

```
itisrajya-api
│
├── Endpoints/
│   └── ChatEndpoints.cs
│
├── Hubs/
│   └── ChatHub.cs
│
├── Services/
│   ├── ChatSessionStore.cs
│   └── ChatSessionManager.cs
│
├── chat/
│   └── *.json
│
├── Program.cs
└── appsettings.json
```

---

## Chat Flow

```
Visitor
    │
    │ Start Session
    ▼
API
    │
    ├── Create Session
    ├── Save JSON
    ├── Start 60s Timer
    └── Send Email Notification
            │
            ▼
         Admin

Visitor ↔ SignalR ↔ API ↔ SignalR ↔ Admin

Every Message
    │
    ├── Save to JSON
    ├── Broadcast via SignalR
    └── Reset 60s Timer

No activity for 60 seconds
    │
    ▼
Session Expired
    │
    ├── Delete JSON
    ├── Notify both clients
    └── Dispose Timer
```

---

## API Endpoints

### Start Chat

```
POST /chat/start
```

Creates a new chat session.

---

### Send Message

```
POST /chat/message
```

Adds a new message and broadcasts it via SignalR.

---

### Get Messages

```
GET /chat/messages/{sessionId}
```

Returns complete chat history.

---

### End Chat

```
POST /chat/end
```

Ends the session and removes all stored data.

---

### Send Chat Notification

```
POST /chat/send-chat-notification
```

Sends an email notification containing the chat session link.

---

## SignalR Hub

```
/chatHub
```

Supported events:

### Client → Server

- JoinSession
- LeaveSession

### Server → Client

- ReceiveMessage
- SessionExpired

---

## Configuration

Configure Gmail SMTP credentials inside `appsettings.json` or environment variables.

```json
{
  "EmailSettings": {
    "GmailUser": "your-email@gmail.com",
    "GmailAppPassword": "your-app-password"
  }
}
```

---

## Running Locally

Clone the repository

```bash
git clone https://github.com/yourusername/itisrajya-api.git
```

Restore packages

```bash
dotnet restore
```

Run

```bash
dotnet run
```

Swagger

```
https://localhost:xxxx/swagger
```

---

## Session Lifecycle

```
Session Created
        │
        ▼
Timer Started (60s)
        │
        ▼
Message Sent
        │
        ▼
Timer Reset
        │
        ▼
No activity for 60 seconds
        │
        ▼
Session Expired
        │
        ├── JSON deleted
        ├── Timer disposed
        └── SignalR notification sent
```

---

## Future Improvements

- Authentication for admin chat
- Persistent database storage (SQL Server/PostgreSQL)
- Typing indicators
- Read receipts
- File attachments
- Message editing/deletion
- Multiple concurrent admin sessions
- Docker support
- Redis backplane for SignalR scaling
- Unit and integration tests

---

## Author

**Rajya Vardhan**

Portfolio: https://itisrajya.net

GitHub: https://github.com/itisrajya

LinkedIn: https://www.linkedin.com/in/itisrajya

X : https://x.com/itisrajya