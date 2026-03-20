namespace Gesdata.VF.Core
{
    /// <summary>
    /// Helpers para manejo de excepciones con Result pattern.
    /// ⚠️ NO incluye logging interno - cada servicio debe hacer su propio logging.
    /// </summary>
    public static class ErrorHandling
    {
        public static Result Try(Action action, string context = null)
        {
            try
            {
                action();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(context ?? ex.Message, ex);
            }
        }

        public static async Task<Result> TryAsync(Func<Task> action, string context = null)
        {
            try
            {
                await action().ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(context ?? ex.Message, ex);
            }
        }

        public static async Task<Result> TryAsync(Func<CancellationToken, Task> action, CancellationToken ct, string context = null)
        {
            try
            {
                await action(ct).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(context ?? ex.Message, ex);
            }
        }

        public static Result<T> Try<T>(Func<T> func, string context = null)
        {
            try
            {
                var value = func();
                return Result<T>.Ok(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Fail(context ?? ex.Message, ex);
            }
        }

        public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, string context = null)
        {
            try
            {
                var value = await func().ConfigureAwait(false);
                return Result<T>.Ok(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Fail(context ?? ex.Message, ex);
            }
        }

        public static async Task<Result<T>> TryAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken ct, string context = null)
        {
            try
            {
                var value = await func(ct).ConfigureAwait(false);
                return Result<T>.Ok(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Fail(context ?? ex.Message, ex);
            }
        }
    }
}
