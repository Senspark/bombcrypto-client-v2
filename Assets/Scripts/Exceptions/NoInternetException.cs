using System;

namespace Exceptions {
    public class NoInternetException : Exception {
        public NoInternetException() : base("No Internet") { }
    }
}