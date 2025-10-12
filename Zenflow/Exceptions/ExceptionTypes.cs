namespace FintechStatsPlatform.Exceptions
{
    public static class ExceptionTypes
    {
        public class CustomException : Exception
        {
            public CustomException(string message)
                : base(message)
            {
                Console.WriteLine(message);
            }

            public CustomException() { }
        }

        public class NotFoundException : CustomException
        {
            public NotFoundException(string objectName, string propertyName, string value)
                : base($"{objectName} with {propertyName} {value} hasn't been found") { }

            public NotFoundException() { }
        }

        public class UserNotFoundException : NotFoundException
        {
            public UserNotFoundException(string propertyName, string userSearchingValue)
                : base("User", propertyName, userSearchingValue) { }

            public UserNotFoundException() { }
        }

        public class AccountNotFoundException : NotFoundException
        {
            public AccountNotFoundException(string propertyName, string accountSearchingValue)
                : base("Account", propertyName, accountSearchingValue) { }

            public AccountNotFoundException() { }
        }

        public class BankNotFoundException : NotFoundException
        {
            public BankNotFoundException(string bankSearchingValue, string propertyName = "name")
                : base("Bank", propertyName, bankSearchingValue) { }

            public BankNotFoundException() { }
        }

        public class ExternalApiException : CustomException
        {
            public ExternalApiException(string apiName, string message)
                : base($"API exception ({apiName}): {message}") { }

            public ExternalApiException() { }
        }

        public class JsonParsingException : Exception
        {
            public JsonParsingException(string message, CustomException? inner = null)
                : base($"Exception while JSON processing: {message}", inner) { }

            public JsonParsingException() { }
        }

        public class UnexpectedException : Exception
        {
            public UnexpectedException(string context, CustomException innerException)
                : base($"Unknown context exception: {context}", innerException) { }

            public UnexpectedException() { }
        }

        public class Auth0Exception : CustomException
        {
            public Auth0Exception(string message)
                : base($"Auth0 error: {message}") { }

            public Auth0Exception() { }
        }

        public class Auth0DeserializationException : JsonParsingException
        {
            public Auth0DeserializationException(string message)
                : base($"Failed to parse Auth0 response: {message}") { }

            public Auth0DeserializationException() { }
        }

        public class ParameterNotFound : CustomException
        {
            public ParameterNotFound(string objectName, string propertyName, string value)
                : base($"{objectName} with {propertyName} {value} is required parameter") { }

            public ParameterNotFound() { }
        }
    }
}
