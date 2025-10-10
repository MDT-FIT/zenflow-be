namespace FintechStatsPlatform.Exceptions
{
    public class ExceptionTypes
    {
        public class CustomException : Exception
        {
            public CustomException(string message) : base(message)
            {
                Console.WriteLine(message);
            }
        }
        public class NotFoundException : CustomException
        {
            public NotFoundException(string objectName, string propertyName, string value) : base($"{objectName} with {propertyName} {value} hasn't been found"){ }
        }
        public class UserNotFoundException : NotFoundException
        {
            public UserNotFoundException(string propertyName, string userSearchingValue) : base("User", propertyName, userSearchingValue) { }
        }

        public class AccountNotFoundException : NotFoundException
        {
            public AccountNotFoundException(string propertyName, string accountSearchingValue) : base("Account", propertyName, accountSearchingValue) { }
        }

        public class BankNotFoundException : NotFoundException 
        {
            public BankNotFoundException(string bankSearchingValue, string propertyName="name") : base("Bank",propertyName,bankSearchingValue) { }
        }

        public class ExternalApiException : CustomException
            {
            public ExternalApiException(string apiName, string message)
                : base($"API exception ({apiName}): {message}") { }
        }

        public class JsonParsingException : Exception
        {
            public JsonParsingException(string message, CustomException inner = null)
                : base($"Exception while JSON processing: {message}",inner) { }
        }

        public class UnexpectedException : Exception
        {
            public UnexpectedException(string context, CustomException innerException)
                : base($"Unknown context exception: {context}", innerException) { }
        }

        public class Auth0Exception : CustomException
        {
            public Auth0Exception(string message)
                : base($"Auth0 error: {message}") { }
        }

        public class Auth0DeserializationException : JsonParsingException
        {
            public Auth0DeserializationException(string message)
                : base($"Failed to parse Auth0 response: {message}") { }
        }
    }
}
