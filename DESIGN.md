# Technical Design Document: OllamaOpenApi

## 1. Introduction
The `OllamaOpenApi` system is an ASP.NET Core Web API project designed to act as an orchestration layer over an Ollama instance. It provides an OpenAPI-compliant interface to manage models, manage conversation sessions, and handle operational queries via a processing system. The system is referred to as the `ollama-openapi-agent`.

## 2. User Workflow
### How to use the AI Agent

**1. Creating a Session**
Start by creating a **Session**. This is like starting a new chat thread. You can define a **System Prompt** to give the AI a specific persona or set of rules, and add your first **User Prompt**.

**2. Drafting Your Ideas**
When you create or add a prompt, it starts in **'Draft'** mode. In this mode:
- The AI will not process the prompt.
- You can freely edit the text or delete the prompt if you change your mind.

**3. Planning a Sequence**
You can add multiple prompts to a single session. The AI will process these one by one in the order they were created (based on the `CreatedTimestamp`). This allows you to build a complex sequence of instructions for the AI to follow.

**4. Preparing Prompts for the AI**
To let the AI know a prompt is ready to be processed, change its status from **'Draft'** to **'Ready'**. To keep the conversation logical, there are some rules:
- **Follow the Order**: You cannot mark a prompt as 'Ready' if any prompt coming before it in the sequence is still in 'Draft' status. You must activate prompts in the order they are to be executed.
- **Reverting to Draft**: If you decide a 'Ready' prompt needs more work and change it back to 'Draft', all subsequent prompts in that session are also automatically reset to 'Draft'. This ensures the AI doesn't skip steps in your planned sequence.

**5. Starting the Process**
Once you have one or more prompts marked as 'Ready', you can trigger the processing of the session by calling the **Resume** command (`/api/Processing/resume`).

**6. Managing the AI's Work**
While the AI is working, you have two ways to stop the process:
- **Pause**: The AI finishes the current prompt it is working on, but stops before starting the next one in the queue. Use the **Pause** command (`/api/Processing/pause`).
- **Interrupt**: The AI stops immediately, canceling the current prompt and ignoring all remaining prompts in the queue. Use the **Interrupt** command (`/api/Processing/interrupt`).

## 3. Architecture Overview
The system consists of three primary components:
- **Web API Layer**: Provides REST endpoints for external clients to interact with the agent.
- **Hosted Service (Agent Orchestrator)**: A background service that maintains the state of sessions and models, manages the processing queue, and communicates with the Ollama instance using the `OllamaSharp` library.
- **OllamaSharp Integration**: The client library used by the Hosted Service to communicate with the underlying Ollama API.
- **Web Client**: A Blazor WebAssembly application that provides a user interface for interacting with the Web API.

### Component Diagram
`[Web Client] <-> [Web API] <-> [Hosted Service] <-> [OllamaSharp] <-> [Ollama Instance]`

## 4. Data Models

### 4.1 Query
Represents a single exchange between the user and the model.
- `Id`: Unique identifier.
- `UserPromptText`: The input text (query).
- `Response`: A collection of generated output parts from Ollama.
- `CreatedTimestamp`: When the query was created.
- `SentTimestamp`: When the query was sent to Ollama.
- `CompletedTimestamp`: When the response was received.
- `Status`: `QueryStatus { Draft, Ready, InProgress, Completed, Error }`

### 4.2 Session
Represents a conversation session.
- `Id`: Unique identifier.
- `Title`: A descriptive name for the session (defaults to `Id`).
- `Priority`: Integer value used to prioritize sessions in the processing queue.
- `Model`: The model used for this session.
- `ThinkLevel`: The thinking level configured for the session.
- `SystemPromptId`: Reference to a predefined system prompt.
- `SystemPrompt`: The guiding instruction for the model (can override the predefined one).
- `Archived`: Boolean indicating if the session is archived.
- `UserPrompts`: A collection of `Query` objects.
- `HasQueriesToProcess`: Boolean indicating if the session has queries in `Ready` or `InProgress` status.

### 4.3 Model
- `Name`: Model identifier.
- `IsDefault`: Boolean indicating if it's the system default.

### 4.4 System Prompt
- `Id`: Unique identifier.
- `Text`: The prompt content.
- `IsReadOnly`: Boolean indicating if the prompt is created from code and cannot be modified or deleted.

## 5. API Specification

### 5.1 Model Management
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/models/list` | `GET` | Returns a list of available models. |
| `/api/models/default` | `GET` | Gets the current default model. |
| `/api/models/default` | `PUT` | Sets a specific model as the default. |

### 5.2 Session Management
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/sessions/list` | `GET` | Lists all current sessions. |
| `/api/sessions/create` | `POST` | Creates a new session with parameters: `model`, `systemPromptId`, `thinkLevel`, `prompt`, `title`, `systemPromptText`. |
| `/api/sessions/{id}/title` | `PUT` | Updates the title of a session. |
| `/api/sessions/{id}/export` | `GET` | Downloads a specific session as a file. |
| `/api/sessions/{id}/import` | `POST` | Uploads a session file to restore a session. |
| `/api/sessions/export-all` | `GET` | Exports all current sessions into a single archive/file. |
| `/api/sessions/import-all` | `POST` | Uploads all session files to restore all sessions. |
| `/api/sessions/save` | `POST` | Manually triggers saving all sessions to disk. |
| `/api/sessions/{id}/archive` | `PUT` | Archives a specific session. |
| `/api/sessions/{id}/unarchive` | `PUT` | Unarchives a specific session. |
| `/api/sessions/{id}/priority` | `PUT` | Updates the priority of a session. |
| `/api/sessions/{id}` | `DELETE` | Deletes a specific session. |
| `/api/sessions/clear` | `DELETE` | Deletes all existing sessions. |

### 5.3 Operational Management (Processing)
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/query` | `POST` | Submits a new query. Includes `SessionId` (optional). If `SessionId` is empty, a new session is automatically created. |
| `/api/Processing/submit` | `POST` | Submits a new query to the processing queue. |
| `/api/Processing/process` | `POST` | Adds specific sessions to the processing queue for processing. |
| `/api/Processing/pause` | `POST` | Pauses the processing. |
| `/api/Processing/resume` | `POST` | Resumes the processing. |
| `/api/Processing/activeSession` | `GET` | Returns the session currently being processed. |
| `/api/Processing/interrupt` | `POST` | Interrupts the processing of the active session. |
| `/api/Processing/status` | `GET` | Returns the status of the processing (Paused/InProgress). |

### 5.4 System Prompt Management
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/SystemPrompt/list` | `GET` | Returns a list of all system prompts. |
| `/api/SystemPrompt` | `PUT` | Creates a new system prompt using `SystemPromptRequest` (only `Text` is required; `Id` is autoincremented and `Role` is always 'system'). |
| `/api/SystemPrompt/{id}` | `PUT` | Updates an existing system prompt using `SystemPromptRequest` (only `Text` is updated). |
| `/api/SystemPrompt/{id}` | `DELETE` | Deletes an existing system prompt. |

### 5.5 Ollama Status Management
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/ollama/ps` | `GET` | Returns a list of currently running models from the Ollama instance. |
| `/api/ollama/load/{model}` | `POST` | Explicitly loads a model into memory. |
| `/api/ollama/unload/{model}` | `POST` | Explicitly unloads a model from memory. |
| `/api/ollama/reload/{model}` | `POST` | Unloads model, compacts memory, and reloads the model. Validates VRAM size. |

### 5.6 User Prompt Management
| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/api/user-prompts/{sessionId}/text` | `POST` | Adds a new user prompt to a session. |
| `/api/user-prompts/{sessionId}/{promptId}/text` | `PUT` | Updates the text of a user prompt (must be in 'Draft' status). |
| `/api/user-prompts/{sessionId}/{promptId}/status/{status}` | `PATCH` | Updates the status of a user prompt (e.g., 'Draft' to 'Ready'). |
| `/api/user-prompts/{sessionId}/{promptId}` | `DELETE` | Deletes a user prompt (must be in 'Draft' status). |

## 6. Hosted Service Logic

### 6.0 Initialization
Upon startup, the `OllamaAgentService` will:
1. Load persisted sessions from disk (if the file exists) using the path specified in `SessionFilePath` configuration (defaults to `sessions.json`).
2. Retrieve the Ollama host URL from the configuration key `ollama_host`.
3. Initialize the `OllamaSharpClient` with this URL.
4. Fetch the list of available models from the Ollama instance via `ListLocalModelsAsync`.
5. Populate the internal `_models` list with the retrieved models.
6. Determine the default model: read from the command-line parameter `--model` (if provided), otherwise read from `OLLAMA_MODEL` configuration; if not found or not available, use the model with the smallest size.
7. Initialize predefined system prompts and apply any overrides from the `SystemPrompts` configuration section.
8. **Default Model Loading**: Send an empty request to Ollama to load the identified default model into memory.

### 6.1 State Management
The Hosted Service will maintain:
- A `ConcurrentDictionary<Guid, Session>` to store sessions in memory.
- A `List<Model>` for available models.
- An `ExecutionList` wrapper that manages the sorting and filtering of sessions for processing.
- A `CancellationTokenSource` to handle the interruption of the active session.
- **Persistence**: Sessions are persisted to a JSON file on disk during service stop or upon manual request.

### 6.2 Execution List Processor
The background worker will:
1. Monitor the `ExecutionList.Status`.
2. If `Status == InProgress`:
    - Retrieve sessions that are in `Ready` or `InProgress` status (checked via `Session.HasQueriesToProcess`), sorted by creation date.
    - If a session is available:
        - Process the session's `UserPrompts` that are in `Ready` or `InProgress` status, **sorted sequentially by `CreatedTimestamp`**.
        - For each prompt:
            - Set prompt status to `InProgress`.
            - Send the prompt to Ollama via `OllamaSharp` using streaming (`IAsyncEnumerable`).
            - Collect response parts into the `Query.Response` collection.
            - Update prompt status to `Completed` or `Error` upon completion.
3. If `Status == Paused`, the processor does nothing.
4. The main loop has a delay of 120,000ms between iterations.
5. **Think Level**: The session's `ThinkLevel` is mapped to `OllamaSharp.ThinkValue`:
    - 0 (None) -> `ThinkValue.Low` (Fallback)
    - 1 (Low) -> `ThinkValue.Low`
    - 2 (Medium) -> `ThinkValue.Medium`
    - 3 (High) -> `ThinkValue.High`
    - 4 (Auto) -> `ThinkValue.Low` (Fallback)

### 6.3 Prompt Status Management
The service implements the following business rules for user prompts:
- **Activation Constraint**: A prompt can only be transitioned from `Draft` to `Ready` if all prompts in the same session with an earlier `CreatedTimestamp` are already in `Ready`, `InProgress`, or `Completed` status.
- **Cascading Reset**: When a prompt is transitioned from `Ready` back to `Draft`, all prompts in that session with a later `CreatedTimestamp` are automatically set to `Draft` status.
- **Modification Guard**: Editing (`PUT`) or deleting (`DELETE`) a prompt is only permitted if the prompt's current status is `Draft`.

### 6.4 Tool Calling Support
The system provides support for tool calling by integrating with `OllamaSharp`'s tool invocation mechanism.
- **Tool Discovery**: The `OllamaAgentService` automatically discovers available tools by scanning the executing assembly for types that inherit from `OllamaSharp.Models.Chat.Tool`. These tools are typically generated by the `OllamaSharp.SourceGenerators` for methods marked with the `[OllamaTool]` attribute.
- **Tool Invocation**: When sending a prompt to the AI model, the discovered tools are passed as part of the request. If the model decides to invoke a tool, the `OllamaSharp.Chat` class handles the invocation via its `ToolInvoker` (by default using `DefaultToolInvoker`) and recursively sends the tool result back to the model to generate the final response.
- **Implementation**: Tools are defined in `OllamaOpenApi.Service.Tools.ToolConsole` and marked with `[OllamaTool]`.

### 6.5 Shutdown
Upon stopping, the `OllamaAgentService` will:
1. Save all current in-memory sessions to the disk persistence file.

## 7. Logging
The system uses **Serilog** for structured logging.
- **Service (OllamaOpenApi.Service)**:
    - **Sinks**: Logs are written to the **Console** and to a **Rolling File** (`logs/service-log-.txt`).
    - **Level**: The minimum logging level is set to `Debug`.
    - **Exception Handling**: A global `ExceptionHandlingMiddleware` catches all unhandled exceptions, logs them via Serilog, and returns a standardized JSON error response (with detailed info in Development mode).
    - **Agent Verbosity**: The `OllamaOpenApi.Service` provides verbose logging in its main loop and session processing, including:
        - Session start/completion/failure.
        - Model being used for a session.
        - Individual query processing and prompts.
        - Queue status updates.
- **Web Client (OllamaOpenApi.WebClient)**:
    - **Sinks**: Logs are written to the **Browser Console**.
    - **Level**: The minimum logging level is set to `Information`.
    - **Exception Handling**: A `GlobalErrorBoundary` component wraps the application to catch unhandled UI exceptions, log them via Serilog, and provide a recovery option for the user.

## 8. Concurrency & Thread Safety
- **Storage**: Use `ConcurrentDictionary` for sessions and an `ExecutionList` wrapper to ensure thread-safe access to sessions.
- **Locks**: Use `SemaphoreSlim` or `lock` blocks when updating global states like the "Default Model" to prevent race conditions.
- **Async/Await**: All I/O operations (API calls to Ollama) must be asynchronous to avoid blocking the hosted service's main loop.

## 9. Technology Stack
- **Framework**: .NET 10.0 (ASP.NET Core Web API, Blazor WASM)
- **Library**: `OllamaSharp`, `Riok.Mapperly`, `Serilog`, `CommandLineParser`
- **Documentation**: Swagger/OpenAPI
- **Persistence**: JSON file-based persistence.
- **CLI**: Use `dotnet` CLI for project creation and management.

## 10. Folder Structure
```text
OllamaOpenApi/
├── OllamaOpenApi.slnx
├── OllamaOpenApi.Service/
│   ├── OllamaOpenApi.Service.csproj
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── Controllers/
│   │   ├── ModelsController.cs
│   │   ├── SessionsController.cs
│   │   ├── SystemPromptController.cs
│   │   ├── ProcessingController.cs
│   │   ├── OllamaController.cs
│   │   ├── UserPromptController.cs
│   │   └── VersionController.cs
│   ├── Data/
│   │   ├── Session.cs
│   │   └── SessionDbContext.cs
│   │   ├── Services/
│   │   │   ├── IOllamaAgentService.cs
│   │   │   ├── OllamaAgentService.cs
│   │   │   ├── OllamaAgentService.MainLoop.cs
│   │   │   ├── OllamaAgentService.Models.cs
│   │   │   ├── OllamaAgentService.ModelsManagement.cs
│   │   │   ├── OllamaAgentService.Sessions.cs
│   │   │   ├── OllamaAgentService.Status.cs
│   │   │   ├── OllamaAgentService.SystemPrompts.cs
│   │   │   ├── OllamaAgentService.UserPrompts.cs
│   │   │   ├── ExecutionList.cs
│   │   │   └── SessionService.cs
│   │   ├── Mappers/
│   │   │   └── ModelMapper.cs
│   │   ├── Models/
│   │   │   ├── Session.cs
│   │   │   ├── Query.cs
│   │   │   ├── SystemPrompt.cs
│   │   │   ├── ModelInfo.cs
│   │   │   └── Enums.cs
│   │   └── Dtos/
│   │       ├── SessionDto.cs
│   │       ├── QueryDto.cs
│   │       ├── ModelDto.cs
│   │       └── Requests.cs
│   └── Tools/
│       └── ToolConsole.Tools.FileSystem.cs
├── OllamaOpenApi.Service.Tests/
│   ├── OllamaOpenApi.Service.Tests.csproj
│   └── ...
├── OllamaOpenApi.ServiceClient/
│   ├── OllamaOpenApi.ServiceClient.csproj
│   ├── Services/
│   │   ├── IOllamaApiService.cs
│   │   └── OllamaApiService.cs
│   └── Models/
│       └── Dtos.cs
├── OllamaOpenApi.ServiceClient.Tests/
│   └── ...
└── OllamaOpenApi.WebClient/
    ├── OllamaOpenApi.WebClient.csproj
    ├── Program.cs
    ├── App.razor
    ├── Layout/
    │   ├── MainLayout.razor
    │   └── NavMenu.razor
    └── Pages/
        ├── Chat.razor
        ├── Sessions.razor
        ├── Processing.razor
        ├── SystemPrompts.razor
        └── OllamaStatus.razor
```

## 11. Issues
- **Analyzer Version Mismatch**: Warning `CS9057` occurs because the `OllamaSharp.SourceGenerators.dll` references a newer version of the compiler (5.6.0.0) than the one currently running (5.0.0.0). This is an environment/tooling issue and cannot be fixed through source code changes.
- **Test Nullability Warnings**: Several warnings (`CS8620`, `CS8602`, `CS8604`) exist in the test project related to nullability. These are currently accepted as the tests are functioning correctly, but should be addressed in a future cleanup phase.

## 12. Web Client Behavior
- **Ollama Status Notification**: When the `Ollama Status` page refreshes the list of running models (either on initialization or via the "Refresh" button), a notification sound is played in the browser upon completion of the request, regardless of whether the request was successful or resulted in an error.

## 13. Workflow Implementation Analysis
To fully support the described user workflow, the following technical functionality must be implemented in the service:

### 13.1 Prompt Status Management
- **Sequential Validation Logic**: The `PATCH /api/UserPrompt/{id}/status` endpoint must implement a check that prevents setting a prompt to `Ready` if any prompt in the same session with a smaller `CreatedTimestamp` is currently in `Draft` status.
- **Cascading Update Trigger**: The same endpoint must implement logic so that if a prompt's status is changed from `Ready` to `Draft`, all prompts in that session with a larger `CreatedTimestamp` are automatically transitioned to `Draft`.
- **State Guards**: The `PUT` and `DELETE` endpoints for `UserPrompt` must explicitly validate that the prompt's current status is `Draft` before allowing modifications.

### 13.2 Execution Control
- **Interrupt Mechanism**: The `/api/Processing/interrupt` endpoint must trigger the `CancellationTokenSource` associated with the active session. This token must be propagated down to the `OllamaSharp` API call to ensure the HTTP request to the Ollama instance is cancelled immediately.
- **Pause Mechanism**: The `/api/Processing/pause` endpoint must update the `ExecutionList.Status` to `Paused`. The background worker must check this status *before* starting the processing of any new prompt in the sequence, ensuring the current prompt completes but the next one does not start.

### 13.3 Processing Order
- **Temporal Sorting**: The background worker's loop must ensure that prompts within a session are retrieved and processed in strict ascending order of their `CreatedTimestamp`.

## 14. CI/CD Pipeline
The project includes a GitHub Actions workflow for continuous delivery of the `OllamaOpenApi.Service` component.
- **Workflow**: `.github/workflows/docker-push.yml`
- **Trigger**: Triggered on push to `main` or `master` branches, or when a version tag (e.g., `v1.0.0`) is pushed.
- **Action**:
    1. Builds the Docker image using the `OllamaOpenApi.Service/Dockerfile`.
    2. Uses `docker/metadata-action` to handle tagging (e.g., `latest` for branches, and specific versions for tags).
    3. Pushes the resulting image to Docker Hub using credentials stored in GitHub Secrets (`DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`).
- **Image Name**: `${{ secrets.DOCKERHUB_USERNAME }}/ollama-openapi-service`
