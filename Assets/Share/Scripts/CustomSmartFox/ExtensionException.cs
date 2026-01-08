using System;

public class ExtensionException : Exception {
    public readonly int ErrorCode;
    public readonly string ServerErrorMessage;

    public ExtensionException(int errorCode, string serverErrorMessage) : base(serverErrorMessage) {
        ErrorCode = errorCode;
        ServerErrorMessage = serverErrorMessage;
    }
}