using Microsoft.AspNetCore.Http;
using SistemaNominaMVC.Models;

namespace SistemaNominaMVC.Helpers
{
    public static class SessionHelper
    {
        public static void SetUser(this ISession session, User user)
        {
            session.SetString("UserId", user.Id.ToString());
            session.SetString("UserName", user.usuario);
            session.SetString("UserRole", user.rol);
            session.SetInt32("EmpNo", user.emp_no);
        }

        public static bool IsLoggedIn(this ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString("UserId"));
        }

        public static string? GetUserRole(this ISession session)
        {
            return session.GetString("UserRole");
        }

        public static int? GetEmpNo(this ISession session)
        {
            return session.GetInt32("EmpNo");
        }

        public static void Logout(this ISession session)
        {
            session.Clear();
        }
    }
}