using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            public const string BasePath = AppControllers.BasePath + "/identity";
            public const string Login = BasePath + "/login";
            public const string Register = BasePath + "/register";
            public const string Logout = BasePath + "/logout";
        }
    }
}