using System;

namespace Share.Scripts.CustomException {
    public class InvalidJwtException : Exception {
        public InvalidJwtException(string message) : base(message) {
        }
        
    }
}