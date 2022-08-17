using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using App.Utils;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace App.Web
{
    /// <summary>�洢�û�������ɫ�б�� Principal</summary>
    public class UserRolePrincipal : GenericPrincipal
    {
        /// <summary>ID</summary>
        public string Id { get; set; }

        /// <summary>��ɫ�б�</summary>
        public string[] Roles { get; set; }

        /// <summary>�����û���ɫ Principal </summary>
        public UserRolePrincipal(IIdentity identity, string id, string[] roles)
            : base(identity, roles)
        {
            this.Id = id;
            this.Roles = roles;
        }
    }

    /// <summary>
    /// ����Ȩ�������������û�����ɫ����Ϣ�ü����ַ���������cookie�У���
    /// ��1��Login ������Ʊ�������û���ɫ����ʱ�����Ϣ���ܱ�����cookie�С�
    /// ��2��LoadPrincipal ��cookie������Ʊ�����õ�ǰ��¼����Ϣ��
    /// ��3��Logout ע��
    /// </summary>
    public class AuthHelper
    {
        /// <summary>�Ƿ��¼</summary>
        public static bool IsLogin()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>��ǰ��¼�û���</summary>
        public static string GetLoginUserName()
        {
            return IsLogin() ? HttpContext.Current.User.Identity.Name : "";
        }

        /// <summary>��ǰ��¼�û��Ƿ����ĳ����ɫ</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return HttpContext.Current.User.IsInRole(role);
            return false;
        }

        /// <summary>��������</summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="userName">�û���</param>
        /// <param name="roleIds">��ɫ�����б�</param>
        public static ClaimsPrincipal CreatePrincipal(string userId, string userName, string roleIds)
        {
            // Aspnetcore ��׼��¼���루Claim-Identity-Principal-Ticket, ����-���-����-��Ʊ��
            var claims = new[] { new Claim("UserID", userId), new Claim("UserName", userName), new Claim("RoleIDs", roleIds) }; // ����
            var identity = new ClaimsIdentity(claims, "AuthenticationScheme");  // ���
            var principal = new ClaimsPrincipal(identity); // ����
            return principal;
        }

        //-----------------------------------------------
        // ��¼
        //-----------------------------------------------
        /// <summary>��¼�����õ�ǰ�û����������û���ƱCookie����</summary>
        /// <param name="id">ID</param>
        /// <param name="user">�û�</param>
        /// <param name="roles">��ɫ�����б�</param>
        /// <param name="expiration">��Ʊ����ʱ��</param>
        /// <param name="domain">��Ʊ��Ч������</param>
        /// <returns>��Ʊ�ַ���</returns>
        public static string Login(string id, string user, string[] roles, DateTime expiration, string domain = "")
        {
            Logout();
            var ticketString = SetCurrentUser(id, user, roles, expiration);
            WriteCookieTicket(ticketString, expiration, domain);
            WriteHeaderTicket(ticketString);
            return ticketString;
        }

        /// <summary>��¼�����õ�ǰ�û�</summary>
        /// <returns>��Ʊ�ַ���</returns>
        public static string SetCurrentUser(string id, string user, string[] roles, DateTime expiration)
        {
            FormsAuthenticationTicket ticket = CreateTicket(id, user, roles, expiration, out string ticketString);
            HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(ticket), id, roles);
            return ticketString;
        }

        /// <summary>�� Ticket д�� Cookie</summary>
        private static void WriteCookieTicket(string ticketString, DateTime expiration, string domain)
        {
            var name = FormsAuthentication.FormsCookieName;
            var cookie = new HttpCookie(name, ticketString);
            cookie.Expires = expiration;
            cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>�� Ticket д�� Header</summary>
        private static void WriteHeaderTicket(string ticketString)
        {
            var name = FormsAuthentication.FormsCookieName;
            HttpContext.Current.Response.Headers.Set(name, ticketString);
        }


        //-----------------------------------------------
        // ��ȡ Cookie ��Ʊ
        //-----------------------------------------------
        /// <summary>�� cookie �� header �ж�ȡ��Ʊ</summary>
        public static UserRolePrincipal LoadPrincipal()
        {
            // ��ȡ��Ʊ�ַ���
            string name = FormsAuthentication.FormsCookieName;
            string value = CookieHelper.FindCookie(name);
            if (value.IsEmpty())
                value = HttpContext.Current.Request.Headers[name];
            if (value.IsEmpty())
                return null;

            // ������Ʊ�ַ���
            try
            {
                FormsAuthenticationTicket authTicket = ParseTicket(value, out string id, out string user, out string[] roles);
                if (authTicket.Expired)
                    return null;
                return new UserRolePrincipal(new FormsIdentity(authTicket), id, roles);
            }
            catch
            {
                return null;
            }
        }



        //-----------------------------------------------
        // ע������
        //-----------------------------------------------
        /// <summary>ע����������Ʊ</summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
            ClearAuthCookie();
            HttpContext.Current.User = null;
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
        }

        private static void ClearAuthCookie()
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
                cookie.Expires = DateTime.Now;
        }

        /// <summary>�ض��򵽵�¼ҳ��</summary>
        public static void RediretToLoginPage()
        {
            FormsAuthentication.RedirectToLoginPage();
        }


        //-----------------------------------------------
        // ��Ʊ�ַ�������
        //-----------------------------------------------
        /// <summary>������Ʊ�ַ���</summary>
        /// <param name="user">�û���</param>
        /// <param name="roles">��ɫ�б�</param>
        /// <param name="expiration">����ʱ��</param>
        /// <param name="ticketString">�����ַ���</param>
        private static FormsAuthenticationTicket CreateTicket(string id, string user, string[] roles, DateTime expiration, out string ticketString)
        {
            // ����ɫ����ת��Ϊ�ַ���
            string userData = string.Format("id={0};roles={1}", id, roles.ToSeparatedString(","));

            // ������Ʊ������֮
            var ticket = new FormsAuthenticationTicket(
                1,                          // �汾
                user,                       // �û���
                DateTime.Now,               // ����ʱ��
                expiration,                 // ����ʱ��
                false,                      // ������
                userData                    // �û�����
                );
            ticketString = FormsAuthentication.Encrypt(ticket);
            return ticket;
        }

        /// <summary>������Ʊ�ַ�������ȡ�û��ͽ�ɫ��Ϣ</summary>
        /// <param name="ticketString">��Ʊ�ַ���</param>
        /// <param name="id">�û�id</param>
        /// <param name="user">�û���</param>
        /// <param name="roles">��ɫ�б�</param>
        /// <returns>����֤Ʊ�ݶ���</returns>
        private static FormsAuthenticationTicket ParseTicket(string ticketString, out string id, out string user, out string[] roles)
        {
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticketString);
            user = authTicket.Name;

            // ����������Ϣ
            id = "";
            roles = null;
            var data = authTicket.UserData;
            Regex r = new Regex("id=(.*);roles=(.*)");
            Match m = r.Match(data);
            if (m.Success)
            {
                id = m.Result("$1");
                roles = m.Result("$2").SplitString().ToArray();
                return authTicket;
            }
            return authTicket;
        }
    }
}
