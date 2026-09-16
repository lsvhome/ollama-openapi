using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public class ExecutionList : ConcurrentDictionary<Guid, Session>
    {
        public ExecutionListStatus Status { get; set; } = ExecutionListStatus.Paused;

        public ExecutionList()
        {
        }

        public IEnumerable<Session> GetSessionsToProcess()
        {
            return this.Values
                .Where(s => s.HasQueriesToProcess())
                .OrderByDescending(s => s.Priority)
                .ThenBy(s => s.CreatedAt);
        }

        public Guid? ActiveSessionId { get; set; } = null;
    }
}
