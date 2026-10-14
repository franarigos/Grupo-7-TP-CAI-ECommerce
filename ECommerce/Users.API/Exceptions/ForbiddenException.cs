using System;

namespace Users.API.Exceptions
{
    public class ForbiddenException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}