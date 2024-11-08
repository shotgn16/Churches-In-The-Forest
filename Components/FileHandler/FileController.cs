using ForestChurches.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace ForestChurches.Components.FileManager
{
    public class FileController : Controller, FileInterface
    {
        private IActionResult actionResult;
        private readonly ILogger<FileController> _logger;
        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }
        public async Task<IActionResult> CreateFile(Details content, string contentType, string fileName = "")
        {
            if (content == null || string.IsNullOrWhiteSpace(fileName)) 
            {
                throw new ArgumentException("Content or filename cannot be null or empty.");
            }

            string fileContent = await GenerateFileContent(content);
            fileName = EnsureValidFilename(fileName) + ".txt";

            try
            {
                await System.IO.File.WriteAllTextAsync(fileName, fileContent);

                actionResult = new FileStreamResult(new FileStream(fileName, FileMode.Open, FileAccess.Read), contentType)
                {
                    FileDownloadName = fileName
                };
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException($"Falid to create file '{fileName}'.", ex);
            }

            return actionResult;
        }

        private async Task<string> GenerateFileContent(Details content)
        {
            StringBuilder fileContent = new StringBuilder();
            string openingHours = await returnOpeningHours(content);

            try
            {
                if (content.result != null)
                {
                    fileContent.AppendLine($"Church Name: {content?.result.name ?? "Unavailable"}")
                        .AppendLine($"Address: {content?.result?.formatted_address ?? "Unavailable"}")
                        .AppendLine($"Phone: {content?.result.formatted_phone_number ?? "Unavailable"}")
                        .AppendLine($"Website: {content?.result.website ?? "Unavailable"}")
                        .AppendLine()
                        .AppendLine($"Wheelchair Church access: {(content?.result.wheelchair_accessible_entrance == true ? "Available" : "Unavailable")}")
                        .AppendLine("\n\n" + openingHours);
                }

                else if (content.result == null)
                {
                    fileContent.AppendLine("An error occurred while processing your request...\nPlease try again later.\n\nIf this issue persists, please contact our support team: support@forestchurches.co.uk");
                }
            }

            catch (Exception ex)
            {
               _logger.LogError($"Error occured while generating file content: {ex.Message}");
                throw ex;
            }

            return fileContent.ToString();
        }

        private string EnsureValidFilename(string filename)
        {
            try
            {
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    filename = filename.Replace(c, '_');
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error occured while ensuring valid filename: {ex.Message}");
            }

            return filename;
        }

        private async Task<string> returnOpeningHours(Details _place)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                if (_place?.result != null && _place?.result?.opening_hours != null)
                {
                    output.AppendLine("Opening Hours:\n");

                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[0] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[1] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[2] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[3] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[4] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[5] ?? "Not available");
                    output.AppendLine(_place?.result?.current_opening_hours?.weekday_text[6] ?? "Not available");
                }

                // BUG : Object reference not set to an instance of an object
                else if (_place?.result?.opening_hours == null)
                {
                    output.Append("No Opening hours available");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error occured while returning opening hours: {ex.Message}");
            }

            return output.ToString();
        }
    }
}
