using System;

namespace BackEndConsole3A
{
    public class InvalidProfileNumberException : Exception
    {
        public InvalidProfileNumberException() {

        }

        public InvalidProfileNumberException(string message) : base(message) {

        }

        public InvalidProfileNumberException(string message, Exception inner) : base(message, inner) {

        }
    }
}