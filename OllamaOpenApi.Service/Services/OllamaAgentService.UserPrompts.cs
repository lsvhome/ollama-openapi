using System;
using System.Linq;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        public Task<bool> AddUserPromptAsync(Guid sessionId, string text)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.UserPrompts.Add(new UserPrompt { UserPromptText = text });
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, string text)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                var prompt = session.UserPrompts.FirstOrDefault(p => p.Id == promptId);
                if (prompt != null && prompt.Status == UserPromptStatus.Draft)
                {
                    prompt.UserPromptText = text;
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetUserPromptStatusAsync(Guid sessionId, Guid promptId, UserPromptStatus status)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                var prompt = session.UserPrompts.FirstOrDefault(p => p.Id == promptId);
                if (prompt == null) return Task.FromResult(false);

                if (status == UserPromptStatus.Ready)
                {
                    // Must be in 'Draft' status to move to 'Ready'
                    if (prompt.Status != UserPromptStatus.Draft) return Task.FromResult(false);

                    // Sequential Activation: No prompt before this one should be in 'Draft' status
                    var precedingDraftPrompts = session.UserPrompts
                        .Where(p => p.CreatedTimestamp < prompt.CreatedTimestamp && p.Status == UserPromptStatus.Draft)
                        .Any();

                    if (precedingDraftPrompts) return Task.FromResult(false);

                    prompt.Status = UserPromptStatus.Ready;
                    return Task.FromResult(true);
                }

                if (status == UserPromptStatus.Draft)
                {
                    // Must be in 'Ready' status to move back to 'Draft' (or maybe just allow it from any status that isn't InProgress/Completed/Error)
                    if (prompt.Status != UserPromptStatus.Ready) return Task.FromResult(false);

                    prompt.Status = UserPromptStatus.Draft;

                    // Cascading Reset: all next user prompts in session become status 'Draft'
                    var subsequentPrompts = session.UserPrompts
                        .Where(p => p.CreatedTimestamp > prompt.CreatedTimestamp);

                    foreach (var p in subsequentPrompts)
                    {
                        p.Status = UserPromptStatus.Draft;
                    }

                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteUserPromptAsync(Guid sessionId, Guid promptId)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                var prompt = session.UserPrompts.FirstOrDefault(p => p.Id == promptId);
                if (prompt != null && prompt.Status == UserPromptStatus.Draft)
                {
                    session.UserPrompts.Remove(prompt);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
    }
}
