using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Security.Principal;
using App.Utils;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace App.Web
{
    /// <summary>
    /// ����Ȩ�������������û�����ɫ����Ϣ�ü����ַ���������cookie�У���
    /// ��1��Login ������Ʊ�������û���ɫ����ʱ�����Ϣ���ܱ�����cookie�С�
    /// ��2��LoadPrincipal ��cookie������Ʊ�����õ�ǰ��¼����Ϣ��
    /// ��3��Logout ע��
    /// </summary>
    public class AuthHelper
    {
        //-----------------------------------------------
        // ��¼ע��
        //-----------------------------------------------
        /// <summary>��¼�����õ�ǰ�û����������û���ƱCookie����</summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="userName">�û���</param>
        /// <param name="roleIds">��ɫ�����б�</param>
        /// <param name="expiration">��Ʊ����ʱ��</param>
        /// <example>AuthHelper.Login("123", "Admin", "1,2,3", DateTime.Now.AddDays(1));</example>
        public static ClaimsPrincipal Login(string userId, string userName, string roleIds, DateTime expiration)
        {
            // Aspnetcore ��׼��¼���루Claim-Identity-Principal-Ticket, ����-���-����-��Ʊ��
            var claims = new[] { new Claim("UserID", userId), new Claim("UserName", userName), new Claim("RoleIDs", roleIds) }; // ����
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);  // ���
            var principal = new ClaimsPrincipal(identity); // ����
            Asp.Current.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties() { IsPersistent = false }
                );
            return principal;
        }

        /// <summary>ע����������Ʊ</summary>
        public static void Logout()
        {
            Asp.Current.SignOutAsync();
            Asp.Current.Session.Clear();
        }

        //-----------------------------------------------
        // �û���Ϣ
        //-----------------------------------------------
        /// <summary>��ǰ�û�</summary>
        public static ClaimsPrincipal User => Asp.Current.User;

        /// <summary>�Ƿ��¼</summary>
        public static bool IsLogin()
        {
            return (Asp.Current.User != null && Asp.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>��ǰ��¼�û���</summary>
        public static string GetUserId()
        {
            return IsLogin() ? Asp.Current.User.Identity.Name : "";
        }
        /// <summary>��ǰ��¼�û���</summary>
        public static string GetUserName()
        {
            if (IsLogin())
                return Asp.Current.User.Claims.Where(x => x.Type == "UserName").FirstOrDefault().Value;
            return "";
        }

        /// <summary>��ǰ��¼�û��Ľ�ɫ�б�</summary>
        public static List<T> GetRoles<T>() where  T: struct
        {
            var roleIds = new List<T>();
            if (IsLogin())
            {
                string text = Asp.Current.User.Claims.Where(x => x.Type == "RoleIDs").FirstOrDefault().Value;
                roleIds.AddRange(text.Split<T>());
            }
            return roleIds;
        }

        /// <summary>��ǰ��¼�û��Ƿ����ĳ����ɫ</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return Asp.Current.User.IsInRole(role);
            return false;
        }
    }
}
