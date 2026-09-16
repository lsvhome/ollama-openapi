using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Tests
{
    public class SessionSerializationTests
    {
        [Fact]
        public void Session_SerializationDeserialization_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalSession = new Session
            {
                Id = Guid.NewGuid(),
                Title = "Test Session",
                Priority = 10,
                CreatedAt = DateTime.UtcNow,
                Model = "llama3",
                ThinkLevel = 2,
                SystemPromptId = 1,
                SystemPrompt = "You are a helpful assistant.",
                WorkingFolder = "/tmp/test-folder",
                UserPrompts = new List<UserPrompt>
                {
                    new UserPrompt
                    {
                        Id = Guid.NewGuid(),
                        UserPromptText = "Hello!",
                        Response = "Hi there!",
                        Status = UserPromptStatus.Completed
                    },
                    new UserPrompt
                    {
                        Id = Guid.NewGuid(),
                        UserPromptText = "How are you?",
                        Response = "I'm doing great!",
                        Status = UserPromptStatus.Completed
                    }
                }
            };

            // Act
            string json = JsonSerializer.Serialize(originalSession);
            var deserializedSession = JsonSerializer.Deserialize<Session>(json);

            // Assert
            Assert.NotNull(deserializedSession);
            Assert.Equal(originalSession.Id, deserializedSession.Id);
            Assert.Equal(originalSession.Title, deserializedSession.Title);
            Assert.Equal(originalSession.Priority, deserializedSession.Priority);
            Assert.Equal(originalSession.CreatedAt, deserializedSession.CreatedAt);
            Assert.Equal(originalSession.Model, deserializedSession.Model);
            Assert.Equal(originalSession.ThinkLevel, deserializedSession.ThinkLevel);
            Assert.Equal(originalSession.SystemPromptId, deserializedSession.SystemPromptId);
            Assert.Equal(originalSession.SystemPrompt, deserializedSession.SystemPrompt);
            Assert.Equal(originalSession.WorkingFolder, deserializedSession.WorkingFolder);
            Assert.Equal(originalSession.UserPrompts.Count, deserializedSession.UserPrompts.Count);

            for (int i = 0; i < originalSession.UserPrompts.Count; i++)
            {
                Assert.Equal(originalSession.UserPrompts[i].Id, deserializedSession.UserPrompts[i].Id);
                Assert.Equal(originalSession.UserPrompts[i].UserPromptText, deserializedSession.UserPrompts[i].UserPromptText);
                Assert.Equal(originalSession.UserPrompts[i].Response, deserializedSession.UserPrompts[i].Response);
                Assert.Equal(originalSession.UserPrompts[i].Status, deserializedSession.UserPrompts[i].Status);
            }
        }

        [Fact]
        public void SessionExportEnvelope_SerializationDeserialization_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalEnvelope = new SessionExportEnvelope
            {
                Version = 1,
                ExportedAt = DateTime.UtcNow,
                Sessions = new List<Session>
                {
                    new Session
                    {
                        Id = Guid.NewGuid(),
                        Title = "Session 1",
                        UserPrompts = new List<UserPrompt> { new UserPrompt { UserPromptText = "P1" } }
                    },
                    new Session
                    {
                        Id = Guid.NewGuid(),
                        Title = "Session 2",
                        UserPrompts = new List<UserPrompt> { new UserPrompt { UserPromptText = "P2" } }
                    }
                }
            };

            // Act
            string json = JsonSerializer.Serialize(originalEnvelope);
            var deserializedEnvelope = JsonSerializer.Deserialize<SessionExportEnvelope>(json);

            // Assert
            Assert.NotNull(deserializedEnvelope);
            Assert.Equal(originalEnvelope.Version, deserializedEnvelope.Version);
            Assert.Equal(originalEnvelope.ExportedAt, deserializedEnvelope.ExportedAt);
            Assert.Equal(originalEnvelope.Sessions.Count, deserializedEnvelope.Sessions.Count);

            for (int i = 0; i < originalEnvelope.Sessions.Count; i++)
            {
                Assert.Equal(originalEnvelope.Sessions[i].Id, deserializedEnvelope.Sessions[i].Id);
                Assert.Equal(originalEnvelope.Sessions[i].Title, deserializedEnvelope.Sessions[i].Title);
                Assert.Equal(originalEnvelope.Sessions[i].UserPrompts.Count, deserializedEnvelope.Sessions[i].UserPrompts.Count);
                Assert.Equal(originalEnvelope.Sessions[i].UserPrompts[0].UserPromptText, deserializedEnvelope.Sessions[i].UserPrompts[0].UserPromptText);
            }
        }
    }
}
