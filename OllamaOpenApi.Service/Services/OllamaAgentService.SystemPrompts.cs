using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        private readonly List<SystemPrompt> _systemPrompts = new();

        public Task<List<SystemPrompt>> GetSystemPromptsAsync() => Task.FromResult(_systemPrompts);

        public Task AddSystemPromptAsync(SystemPrompt prompt)
        {
            prompt.Id = _systemPrompts.Any() ? _systemPrompts.Max(p => p.Id) + 1 : 1;
            prompt.IsReadOnly = false; // User added prompts are not read-only
            _systemPrompts.Add(prompt);
            return Task.CompletedTask;
        }

        public Task UpdateSystemPromptAsync(SystemPrompt prompt)
        {
            var existing = _systemPrompts.FirstOrDefault(p => p.Id == prompt.Id);
            if (existing != null)
            {
                if (existing.IsReadOnly)
                {
                    throw new InvalidOperationException("Cannot update a read-only system prompt.");
                }
                existing.Text = prompt.Text;
            }
            return Task.CompletedTask;
        }

        public Task DeleteSystemPromptAsync(int id)
        {
            var prompt = _systemPrompts.FirstOrDefault(p => p.Id == id);
            if (prompt != null)
            {
                if (prompt.IsReadOnly)
                {
                    throw new InvalidOperationException("Cannot delete a read-only system prompt.");
                }
                _systemPrompts.Remove(prompt);
            }
            return Task.CompletedTask;
        }

        private void InitializeDefaultPrompts()
        {
            var agentsRules = @"
- Consider folder /workspace/ollama-openapi/ as work folder and repo root.
- When source changes are requested, they should be split to small, meaningful pieces.
- Each source change should be committed with a relevant message.
- Build & Quality: The solution must be buildable, and all warnings must be fixed. If a warning cannot be fixed, it must be documented in the 'Issues' paragraph of technical-design.md with an explanation of why it cannot be fixed.
- Testing: All tests must be passing (green).
- Documentation: The technical-design.md or DESIGN.md file must be updated to reflect the new state of the system (if file exists).
- Formatting: dotnet format must be executed before committing changes.
- Commit Message: The commit message must clearly reflect the changes that have been implemented.";

            var defaults = new List<SystemPrompt>
            {
                new SystemPrompt
                {
                    Text = @"You are a Senior Software Architect.
You can use available tools and functions for your work.
When source changes are requested, request is completed only when all requested changes are committed.
When source changes are requested, they should be split into small, meaningful pieces.
Each source change should be committed with a relevant message.
Build & Quality: The solution must be buildable, and all warnings must be fixed. If a warning cannot be fixed, it must be documented in the 'Issues' paragraph of DESIGN.md with an explanation of why it cannot be fixed.
Testing: All tests must be passing (green).
Documentation: The DESIGN.md file (if it is present) must be updated to reflect the new state of the system.
Formatting: for dotnet projects and solutions dotnet format must be executed before committing changes.
Commit Message: The commit message must clearly reflect the changes that have been implemented. After excecuting commit command git repo should be verified commit is available, commit hash is present.
Follow repo-specific rules in file AGENTS.md when it is present.
When user request completed, working folder should not contain uncommitted files.
When user request completed type 'Task completed in {list of commit hashes}'",
                    IsReadOnly = true
                },
                new SystemPrompt
                {
                    Text = $"You are an expert C# Software Developer. Follow these rules:\n{agentsRules}",
                    IsReadOnly = true
                },
                new SystemPrompt
                {
                    Text = "You are an expert Go Developer. Provide efficient, idiomatic Go code.",
                    IsReadOnly = true
                },
                new SystemPrompt
                {
                    Text = "You are an expert Nodejs Developer. Focus on asynchronous patterns and modern ESM.",
                    IsReadOnly = true
                },
                new SystemPrompt
                {
                    Text = "You are an expert C# and Blazor Developer. Focus on component lifecycle and state management.",
                    IsReadOnly = true
                }
            };

            for (int i = 0; i < defaults.Count; i++)
            {
                var prompt = defaults[i];
                prompt.Id = i + 1;

                // Allow overwrite from configuration: SystemPrompts:1, SystemPrompts:2, etc.
                var configText = _configuration[$"SystemPrompts:{(i + 1)}"];
                if (!string.IsNullOrEmpty(configText))
                {
                    prompt.Text = configText;
                }

                _systemPrompts.Add(prompt);
            }
        }
    }
}
