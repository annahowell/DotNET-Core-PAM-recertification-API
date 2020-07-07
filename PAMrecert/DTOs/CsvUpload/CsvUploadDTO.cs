using Microsoft.AspNetCore.Http;

namespace PAMrecert.DTOs.CsvUpload
{
    public class CsvUploadDTO
    {
        public bool? DeleteAndReplace { get; set; }
        public IFormFile File { get; set; }
    }
}