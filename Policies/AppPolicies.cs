namespace Masarak.Policies
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";
        public const string Parent = "Parent";
    }

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
