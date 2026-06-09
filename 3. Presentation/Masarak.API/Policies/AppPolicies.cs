namespace Masarak.API.Policies
{
    public static class AppPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string TeacherOnly = "TeacherOnly";
        public const string StudentOnly = "StudentOnly";
        public const string ParentOnly = "ParentOnly";
        public const string AdminOrTeacher = "AdminOrTeacher";
        public const string StudentOrParent = "StudentOrParent";
        public const string AnyAuthenticated = "AnyAuthenticated";
    }
}
