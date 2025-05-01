namespace TicTacToeGame.Helpers.Constants;

public class AppConstants
{
    public const string DefaultRoutePrefix = "api";
    public const string CurrentConnectionStringSectionName = "DefaultConnection";

    public class Tables
    {
        public const string Schema = "dbo";
        public class IdentityTables
        {
            public const string Schema = "Identity";
            public const string AppUser = "Users";
        }
        public class TicTacToeTables
        {
            public const string Schema = "TicTacToe";
            public const string TicTacToeMatch = "TicTacToeMatches";
            public const string TicTacToeMatchHistory = "TicTacToeMatchHistories";
        }
    }

    public class HubPath
    {
        public const string BasePath = "/hubs";
        public const string TicTacToeHub = BasePath + "/gamehub";
    }

    public class AppControllers
    {
        public const string BasePath = "";

        public class IdentityController
        {
            public const string BasePath = AppControllers.BasePath + "identity";
            public const string Login = "login";
            public const string Register = "register";
            public const string Logout = "logout";
            public const string GetInfo = "userinfo";
        }
    }

    public class AppResponses
    {
        public class IdentityControllerResponses
        {
            public class LoginResponses
            {
                public const string InvalidUsernameOrPassword = "Invalid username or password.";
                public static readonly Dictionary<string, string> InvalidUsernameOrPasswordErrors = new()
                {
                    { "summary", InvalidUsernameOrPassword }
                };
                public const string LoginSuccessful = "Login successful.";
            }
            public class LogoutReponses
            {
                public const string LogoutSuccessful = "Logout successful.";
            }
            public class RegisterResponses
            {
                public const string RegistrationFailed = "Registration failed.";
                public static readonly Dictionary<string, string> RegistrationFailedErrors = new()
                {
                    { "summary", RegistrationFailed }
                };
                public const string RegistrationSuccessful = "Registration successful.";
            }

            public class GetUserInfoResponses
            {
                public const string GetInfoSuccessful = "Get info successful.";
                public const string UserIdNotFound = "User ID not found.";
                public static readonly Dictionary<string, string> UserIdNotFoundErrors = new()
                {
                    { "summary", UserIdNotFound }
                };
                public const string UserNotFound = "Get user info failed. User not found.";
                public static readonly Dictionary<string, string> UserNotFoundErrors = new()
                {
                    { "summary", UserNotFound }
                };
            }
        }
    }
}