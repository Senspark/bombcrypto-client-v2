using System.Collections.Generic;

namespace Senspark.Server {
    using Dict = Dictionary<string, object>;

    public interface IDataTable {
        string TableName { get; }
        InputData Serialize();
    }

    public class WriteResult {
        public readonly bool IsSuccess;
        public readonly string ErrorMessage;

        public WriteResult(bool isSuccess, string errorMessage) {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }

    public class ReadResult {
        public readonly bool IsSuccess;
        public readonly string ErrorMessage;
        public readonly IOutputData Data;

        public ReadResult(bool isSuccess, string errorMessage, IOutputData data) {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Data = data;
        }
    }
}