using System.Net;

namespace CoursesManagementSystem.DTOs
{
    public class APIResponse
    {
        public bool IsSuccess { get; set; } = true;

        public HttpStatusCode StatusCode { get; set; }

        public object Result { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
