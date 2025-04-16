namespace CoursesManagementSystem.Constants
{
    public static class UploadsSettings
    {
        public const string BooksPath = "/Uploads/Books";
        public const string AudiosPath = "/Uploads/Audios";
        public const string VideosPath = "/Uploads/Videos";

        public const string AllowedExtensions = ".pdf,.doc,.docx,.xls,.xlsx,.txt";
        public const int MaxFileSizeInMB = 1;
        public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
    }
}
