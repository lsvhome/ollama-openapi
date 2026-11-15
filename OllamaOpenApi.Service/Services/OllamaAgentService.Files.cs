using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        private string _uploadsFolder = "uploads";

        public async Task<UserPromptAttachment> SaveUploadedFileAsync(string fileName, Stream fileStream, string contentType)
        {
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }

            var fileId = Guid.NewGuid();
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{fileId}{extension}";
            var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.CopyToAsync(fs);
            }

            var fileInfo = new FileInfo(filePath);

            return new UserPromptAttachment
            {
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                ContentType = contentType
            };
        }
    }
}
