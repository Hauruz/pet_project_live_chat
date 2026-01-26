# Chat Application - Angular Frontend

A real-time chat application built with Angular 18, SignalR, and TypeScript.

## Features

- **Authentication**: User registration and login with JWT tokens
- **Real-time Messaging**: WebSocket support via SignalR for live chat
- **Chat Rooms**: Create and manage multiple chat rooms
- **User Management**: See active users in chat rooms
- **Responsive Design**: Works on desktop and mobile devices

## Project Structure

```
src/
├── app/
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login/
│   │   │   └── register/
│   │   └── chat/
│   │       ├── chat-room/
│   │       ├── chat-room-list/
│   │       └── chat.component.ts
│   ├── services/
│   │   ├── auth.service.ts
│   │   ├── chat.service.ts
│   │   └── signalr.service.ts
│   ├── models/
│   │   └── index.ts
│   ├── shared/
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts
│   │   └── guards/
│   │       └── auth.guard.ts
│   ├── app.routes.ts
│   └── app.component.ts
├── main.ts
├── index.html
└── styles.scss
```

## Getting Started

### Prerequisites

- Node.js (v18 or higher)
- npm or yarn
- Angular CLI (v18)

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm start
```

3. Open your browser and navigate to:
```
http://localhost:4200
```

## Configuration

Update the API URLs in the services to match your backend:

- [src/app/services/auth.service.ts](src/app/services/auth.service.ts): Update `apiUrl`
- [src/app/services/chat.service.ts](src/app/services/chat.service.ts): Update `apiUrl`
- [src/app/services/signalr.service.ts](src/app/services/signalr.service.ts): Update hub URL

Default URLs (HTTPS):
```typescript
private apiUrl = 'https://localhost:7000/api/auth';
private apiUrl = 'https://localhost:7000/api/chat';
this.hubConnection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7000/hubs/chat', ...)
```

## Usage

### 1. Register a New Account

- Click on "Register here" on the login page
- Enter username (min 3 characters) and password (min 6 characters)
- Click "Register"

### 2. Login

- Enter your credentials
- Click "Login"

### 3. Start Chatting

- Select a chat room from the list
- Type a message and click "Send"
- Messages appear in real-time via WebSocket

## Building for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Technologies Used

- **Angular 18**: Frontend framework
- **TypeScript**: Programming language
- **RxJS**: Reactive programming library
- **SignalR**: Real-time communication
- **SCSS**: Styling
- **HTTP Client**: API communication

## License

MIT
