namespace LushThreads.Application.DTOs.User
{
    public class LockUnlockResultDto
    {
        public bool Success { get; set; }
        public bool IsLocked { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}