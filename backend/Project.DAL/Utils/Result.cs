namespace Project.DAL.Utils
{
    public class ResultBase
    {
        protected ResultBase(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None ||
                !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
    }

    public class Result : ResultBase
    {
        private Result(bool isSuccess, Error error) : base(isSuccess, error) { }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }

    public class Result<T> : ResultBase
    {
        private Result(bool isSuccess, Error error, T? payload = default) : base(isSuccess, error)
        {
            Payload = payload;
        }

        public T? Payload { get; }
        public static Result<T> Success(T? payload) => new(true, Error.None, payload);
        public static Result<T> Failure(Error error) => new(false, error);
    }
}