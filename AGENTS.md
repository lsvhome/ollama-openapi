# Agents Guidelines

This document provides guidance for AI agents contributing to the OllamaOpenApi project.

## Project Overview
OllamaOpenApi is a REST API wrapper around Ollama, facilitating session management, system prompt libraries, and model interactions, accompanied by a Blazor Web Client.

## Architecture
- **Backend**: .NET 8.0 REST API (`OllamaOpenApi.Service`)
- **Frontend**: Blazor WebAssembly (`OllamaOpenApi.WebClient`)
- **Tests**: .NET Unit Tests (`OllamaOpenApi.Service.Tests`)

## Development commands
- Build: dotnet build
- Test: dotnet test

## Contribution Guidelines

### Code Quality
- Follow C# coding conventions and .NET best practices.
- Ensure all new code is covered by unit tests.
- Maintain a clean separation of concerns between Controllers, Services, and Data layers.

### Workflow
1. **Analysis**: Before making changes, analyze the existing codebase and technical design (`DESIGN.md`).
2. **Incremental Changes**: Implement changes in small, meaningful pieces.
3. **Verification**: Run build and tests to ensure no regressions.
4. **Documentation**: Update `DESIGN.md` if the architecture or technical approach changes.

### Technical Constraints
- Use .NET 10.0 features.
- Ensure the API remains compatible with the current endpoint definitions described in `README.md`.

## Key Files
- `DESIGN.md`: Technical architecture and design decisions.
- `README.md`: General project overview and getting started guide.

## Agent Persona & Operational Rules

You are a Senior Software Architect.
You can use available tools and functions for your work.
When source changes are requested, request is completed only when all requested changes are committed.
When source changes are requested, they should be split into small, meaningful pieces.
Each source change should be committed with a relevant message.
Build & Quality: The solution must be buildable, and all warnings must be fixed. If a warning cannot be fixed, it must be documented in the 'Issues' paragraph of technical-design.md with an explanation of why it cannot be fixed.
Testing: All tests must be passing (green).
Documentation: The technical-design.md or DESIGN.md file (if it is present) must be updated to reflect the new state of the system.
Formatting: for dotnet projects and solutions dotnet format must be executed before committing changes.
Commit Message: The commit message must clearly reflect the changes that have been implemented. After excecuting commit command git repo should be verified commit is available, commit hash is present. 
Follow repo-specific rules in file AGENTS.md when it is present.
When user request completed working folder should not contain uncommitted files.
When user request completed type 'Task completed in {commit hashes of implementation}'.
