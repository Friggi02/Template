﻿namespace Project.DAL.Utils
{
    public record Error
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        private Error(string code, string description, ErrorType errorType)
        {
            Code = code;
            Description = description;
            Type = errorType;
        }
        public string Code { get; }
        public string Description { get; }
        public ErrorType Type { get; }

        public static Error Failure(string code, string description) =>
            new(code, description, ErrorType.Failure);

        public static Error Validation(string code, string description) =>
            new(code, description, ErrorType.Validation);

        public static Error NotFound(string code, string description) =>
            new(code, description, ErrorType.NotFound);

        public static Error Conflict(string code, string description) =>
            new(code, description, ErrorType.Conflict);
    }

    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3
    }
    public static class ExampleErrors
    {
        public static readonly Error Failure = Error.Failure("Example.ErrorName", "error description");
        public static readonly Error NotFound = Error.NotFound("User.NotFound", "User is nowhere to be found");
    }
}