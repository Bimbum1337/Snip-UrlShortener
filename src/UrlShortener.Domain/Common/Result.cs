namespace UrlShortener.Domain.Common;

public readonly record struct Result
{
    private Result(bool isSuccess, Error[] errors)
        => (IsSuccess, Errors) = (isSuccess, errors);

    public bool IsSuccess { get; }
    public Error[] Errors { get; }

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, []);
    public static Result Failure(params Error[] errors) => new(false, errors);

    public static Result NotFound(string description) => Failure(Error.NotFound(description));
    public static Result Conflict(string description) => Failure(Error.Conflict(description));
    public static Result BadRequest(string description) => Failure(Error.BadRequest(description));
    public static Result Validation(string description) => Failure(Error.Validation(description));

    public static Result Combine(params Result[] results)
        => Array.Exists(results, r => r.IsFailure)
            ? Failure(results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToArray())
            : Success();
}

public readonly record struct Result<T>
{
    private Result(bool isSuccess, T? value, Error[] errors)
        => (IsSuccess, Value, Errors) = (isSuccess, value, errors);

    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error[] Errors { get; }

    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) => new(true, value, []);
    public static Result<T> Failure(params Error[] errors) => new(false, default, errors);

    public static Result<T> NotFound(string description) => Failure(Error.NotFound(description));
    public static Result<T> Conflict(string description) => Failure(Error.Conflict(description));
    public static Result<T> BadRequest(string description) => Failure(Error.BadRequest(description));
    public static Result<T> Validation(string description) => Failure(Error.Validation(description));

    public static implicit operator Result<T>(T value) => Success(value);

    public Result<TOut> Map<TOut>(Func<T, TOut> map)
        => IsSuccess ? Result<TOut>.Success(map(Value!)) : Result<TOut>.Failure(Errors);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> next)
        => IsSuccess ? next(Value!) : Result<TOut>.Failure(Errors);

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
        => IsSuccess && !predicate(Value!) ? Failure(error) : this;

    public Result ToResult() => IsSuccess ? Result.Success() : Result.Failure(Errors);
}
