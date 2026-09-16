using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using OllamaOpenApi.Dto;
using OllamaSharp.Models.Chat;

namespace OllamaOpenApi.Service.Tests
{
    public class UserPromptSerializationTests
    {
        [Fact]
        public void UserPrompt_SerializationDeserialization_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalPrompt = new UserPrompt
            {
                Id = Guid.NewGuid(),
                UserPromptText = "What is the capital of France?",
                Response = "The capital of France is Paris.",
                CreatedTimestamp = DateTime.UtcNow,
                SentTimestamp = DateTime.UtcNow.AddMinutes(1),
                CompletedTimestamp = DateTime.UtcNow.AddMinutes(2),
                Status = UserPromptStatus.Completed,
                Messages = new List<Message>
                {
                    new Message(ChatRole.User, "What is the capital of France?"),
                    new Message(ChatRole.Assistant, "The capital of France is Paris."){Thinking = "thinking001", ToolName = "tool001", ToolCalls = new[] { new Message.ToolCall{ Id = "toolcall001", Function = new Message.Function{ Name = "func001"} } }}
                }
            };

            // Act
            string json = JsonSerializer.Serialize(originalPrompt);
            var deserializedPrompt = JsonSerializer.Deserialize<UserPrompt>(json);

            // Assert
            Assert.NotNull(deserializedPrompt);
            Assert.Equal(originalPrompt.Id, deserializedPrompt.Id);
            Assert.Equal(originalPrompt.UserPromptText, deserializedPrompt.UserPromptText);
            Assert.Equal(originalPrompt.Response, deserializedPrompt.Response);
            Assert.Equal(originalPrompt.Status, deserializedPrompt.Status);

            // Compare timestamps with a small tolerance or just check if they are not default
            Assert.Equal(originalPrompt.CreatedTimestamp, deserializedPrompt.CreatedTimestamp);
            Assert.Equal(originalPrompt.SentTimestamp, deserializedPrompt.SentTimestamp);
            Assert.Equal(originalPrompt.CompletedTimestamp, deserializedPrompt.CompletedTimestamp);

            Assert.Equal(originalPrompt.Messages.Count, deserializedPrompt.Messages.Count);
            for (int i = 0; i < originalPrompt.Messages.Count; i++)
            {
                Assert.Equal(originalPrompt.Messages[i].Role, deserializedPrompt.Messages[i].Role);
                Assert.Equal(originalPrompt.Messages[i].Content, deserializedPrompt.Messages[i].Content);
                Assert.Equal(originalPrompt.Messages[i].Thinking, deserializedPrompt.Messages[i].Thinking);
                Assert.Equal(originalPrompt.Messages[i].Created, deserializedPrompt.Messages[i].Created);
                Assert.Equal(originalPrompt.Messages[i].Id, deserializedPrompt.Messages[i].Id);
                Assert.Equal(originalPrompt.Messages[i].ToolName, deserializedPrompt.Messages[i].ToolName);
                Assert.Equal(originalPrompt.Messages[i].ToolCalls != null, deserializedPrompt.Messages[i].ToolCalls != null);
                if (originalPrompt.Messages[i].ToolCalls != null)
                {
                    for (int j = 0; j < originalPrompt.Messages[i].ToolCalls.Count(); j++)
                    {
                        Assert.Equal(originalPrompt.Messages[i].ToolCalls.Skip(j).FirstOrDefault().Id, deserializedPrompt.Messages[i].ToolCalls.Skip(j).FirstOrDefault().Id);
                        Assert.Equal(originalPrompt.Messages[i].ToolCalls.Skip(j).FirstOrDefault().Function.Name, deserializedPrompt.Messages[i].ToolCalls.Skip(j).FirstOrDefault().Function.Name);
                    }
                }
            }
        }
    }
}
