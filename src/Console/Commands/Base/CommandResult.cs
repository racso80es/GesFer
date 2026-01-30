using System.Collections.Generic;

namespace GesFer.ConsoleApp.Commands.Base;

public class CommandResult<TResult>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TResult? Data { get; set; }
    public List<string> Logs { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();

    public static CommandResult<TResult> Ok(TResult data, string message = "Success")
    {
        return new CommandResult<TResult>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static CommandResult<TResult> Fail(string message, List<string>? errors = null)
    {
        return new CommandResult<TResult>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public void AddLog(string log)
    {
        Logs.Add(log);
    }
}

public class CommandResult : CommandResult<object?>
{
    public static CommandResult Ok(string message = "Success")
    {
        return new CommandResult
        {
            Success = true,
            Message = message,
            Data = null
        };
    }

    public static new CommandResult Fail(string message, List<string>? errors = null)
    {
        return new CommandResult
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
