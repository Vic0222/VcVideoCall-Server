using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Exceptions
{
    public class UserNotFoundExeption : Exception
    {
        public UserNotFoundExeption(string message) : base(message)
        {
        }

        public UserNotFoundExeption(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
