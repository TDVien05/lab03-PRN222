# Project Idea: LAN Group Chat Web Application

## Overview

This project is a web-based group chat application for people connected to the same local network. One laptop runs the application as the server, and other users on the same Wi-Fi or LAN can open the app in a browser and chat together.

Users do not need to register or log in. They only enter a display name, create or join a group chat, and start chatting.

## Main Goal

The goal of this project is to build a simple local-network communication platform where users can create group chats, join existing group chats, send messages, and share images or files.

## Key Features

- Enter a display name before chatting
- Create new group chats
- Join existing group chats
- Send and receive real-time text messages
- Upload and send images in chat
- Upload and send files in chat
- Display chat history
- Show message sender name
- Show message timestamp
- View group members
- Leave a group chat

## User Actions

Users can:

- Enter a display name
- Create group chats
- Join group chats
- Send messages
- Send images and files
- View chat history
- View group members
- Leave a group

## Core Pages

### Welcome Page

Allows users to enter a display name before using the chat app.

### Group List Page

Displays available group chats and groups the user has joined.

### Chat Room Page

The main page where users can send and receive messages, images, and files.

### Group Detail Page

Shows group information and group members.

## Main Entities

### Participant

Stores a user's display name and the time they first joined the application.

### Group

Stores group chat information such as group name, description, creator, and creation date.

### GroupMember

Stores the relationship between participants and groups.

### Message

Stores chat messages, including text content, sender, group, timestamp, and message type.

### Attachment

Stores uploaded image or file information, including file name, file path, file type, and upload date.

## Suggested Technology Stack

- Frontend: ASP.NET Razor Pages
- Backend: ASP.NET Core
- Database: PostgreSQL
- Real-time communication: SignalR
- File storage: Local server storage

## LAN Deployment

The laptop running the application becomes the server.

To find the laptop IP address, run:

```powershell
ipconfig
```

Look for the IPv4 address, usually similar to `192.168.x.x`.

To run the app for other users on the same network:

```powershell
dotnet run --project lab03 --urls http://0.0.0.0:5026
```

Other users can open the app using:

```text
http://<laptop-ip>:5026
```

Windows Firewall must allow access to port `5026`.

## Future Improvements

- User registration and login
- Private one-to-one messaging
- Message reactions
- Message editing and deletion
- Typing indicators
- Read receipts
- Push notifications
- Search messages and files
- User profile pictures
- Group invitation links

## Expected Outcome

At the end of the project, users on the same local network should be able to open the app from the laptop server, enter a display name, create or join a group chat, exchange messages, and share images or files with other members in the same group.
