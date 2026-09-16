# OllamaOpenApi

OllamaOpenApi is a project that provides a REST API wrapper around Ollama, allowing for better management of sessions, system prompts, and model interactions. It includes a Web Client for a user-friendly interface to interact with the Ollama service.

## Project Structure

- **OllamaOpenApi.Service**: The backend service that implements the REST API, manages sessions, and communicates with the Ollama agent.
- **OllamaOpenApi.WebClient**: A Blazor WebAssembly client that provides a UI for managing and interacting with the Ollama service.
- **OllamaOpenApi.Service.Tests**: Unit and integration tests for the backend service.

## Features

- **Session Management**: Create and manage chat sessions with specific system prompts and think levels.
- **System Prompt Management**: Create and maintain a library of system prompts.
- **Model Monitoring**: View currently running models and their status.
- **Custom Think Levels**: Control the reasoning depth of the models.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Ollama installed and running on your machine

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```
2. Navigate to the project root:
   ```bash
   cd ollama-openapi
   ```

## API Endpoints

The service provides several endpoints for managing the Ollama interaction:
- `/api/models`: Model management and status.
- `/api/sessions`: Session lifecycle management.
- `/api/system-prompts`: System prompt library.
- `/api/queue`: Queue management for sessions.

## Technical Design

For more detailed technical information, please refer to `technical-design.md`.
