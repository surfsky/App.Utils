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
    /// <summary>存储用户名及角色列表的 Principal</summary>
    public class UserRolePrincipal : GenericPrincipal
    {
        /// <summary>ID</summary>
        public string Id { get; set; }

        /// <summary>角色列表</summary>
        public string[] Roles { get; set; }

        /// <summary>创建用户角色 Principal </summary>
        public UserRolePrincipal(IIdentity identity, string id, string[] roles)
            : base(identity, roles)
        {
            this.Id = id;
            this.Roles = roles;
        }
    }

    /// <summary>
    /// 表单鉴权辅助函数（将用户、角色等信息用加密字符串保存在cookie中）。
    /// （1）Login 创建验票，并将用户角色过期时间等信息加密保存在cookie中。
    /// （2）LoadPrincipal 从cookie解析验票并设置当前登录人信息。
    /// （3）Logout 注销
    /// </summary>
    public class AuthHelper
    {
        /// <summary>是否登录</summary>
        public static bool IsLogin()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>当前登录用户名</summary>
        public static string GetLoginUserName()
        {
            return IsLogin() ? HttpContext.Current.User.Identity.Name : "";
        }

        /// <summary>当前登录用户是否具有某个角色</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return HttpContext.Current.User.IsInRole(role);
            return false;
        }

        /// <summary>创建主角</summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="roleIds">角色名称列表</param>
        public static ClaimsPrincipal CreatePrincipal(string userId, string userName, string roleIds)
        {
            // Aspnetcore 标准登录代码（Claim-Identity-Principal-Ticket, 属性-身份-主角-验票）
            var claims = new[] { new Claim("UserID", userId), new Claim("UserName", userName), new Claim("RoleIDs", roleIds) }; // 属性
            var identity = new ClaimsIdentity(claims, "AuthenticationScheme");  // 身份
            var principal = new ClaimsPrincipal(identity); // 主角
            return principal;
        }

        //-----------------------------------------------
        // 登录
        //-----------------------------------------------
        /// <summary>登录（设置当前用户，并创建用户验票Cookie）。</summary>
        /// <param name="id">ID</param>
        /// <param name="user">用户</param>
        /// <param name="roles">角色名称列表</param>
        /// <param name="expiration">验票到期时间</param>
        /// <param name="domain">验票生效的域名</param>
        /// <returns>验票字符串</returns>
        public static string Login(string id, string user, string[] roles, DateTime expiration, string domain = "")
        {
            Logout();
            var ticketString = SetCurrentUser(id, user, roles, expiration);
            WriteCookieTicket(ticketString, expiration, domain);
            WriteHeaderTicket(ticketString);
            return ticketString;
        }

        /// <summary>登录并设置当前用户</summary>
        /// <returns>验票字符串</returns>
        public static string SetCurrentUser(string id, string user, string[] roles, DateTime expiration)
        {
            FormsAuthenticationTicket ticket = CreateTicket(id, user, roles, expiration, out string ticketString);
            HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(ticket), id, roles);
            return ticketString;
        }

        /// <summary>将 Ticket 写入 Cookie</summary>
        private static void WriteCookieTicket(string ticketString, DateTime expiration, string domain)
        {
            var name = FormsAuthentication.FormsCookieName;
            var cookie = new HttpCookie(name, ticketString);
            cookie.Expires = expiration;
            cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>将 Ticket 写入 Header</summary>
        private static void WriteHeaderTicket(string ticketString)
        {
            var name = FormsAuthentication.FormsCookieName;
            HttpContext.Current.Response.Headers.Set(name, ticketString);
        }


        //-----------------------------------------------
        // 读取 Cookie 验票
        //-----------------------------------------------
        /// <summary>从 cookie 或 header 中读取验票</summary>
        public static UserRolePrincipal LoadPrincipal()
        {
            // 获取验票字符串
            string name = FormsAuthentication.FormsCookieName;
            string value = CookieHelper.FindCookie(name);
            if (value.IsEmpty())
                value = HttpContext.Current.Request.Headers[name];
            if (value.IsEmpty())
                return null;

            // 解析验票字符串
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
        // 注销处理
        //-----------------------------------------------
        /// <summary>注销。销毁验票</summary>
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

        /// <summary>重定向到登录页面</summary>
        public static void RediretToLoginPage()
        {
            FormsAuthentication.RedirectToLoginPage();
        }


        //-----------------------------------------------
        // 验票字符串处理
        //-----------------------------------------------
        /// <summary>创建验票字符串</summary>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <param name="expiration">过期时间</param>
        /// <param name="ticketString">加密字符串</param>
        private static FormsAuthenticationTicket CreateTicket(string id, string user, string[] roles, DateTime expiration, out string ticketString)
        {
            // 将角色数组转化为字符串
            string userData = string.Format("id={0};roles={1}", id, roles.ToSeparatedString(","));

            // 创建验票并加密之
            var ticket = new FormsAuthenticationTicket(
                1,                          // 版本
                user,                       // 用户名
                DateTime.Now,               // 创建时间
                expiration,                 // 到期时间
                false,                      // 非永久
                userData                    // 用户数据
                );
            ticketString = FormsAuthentication.Encrypt(ticket);
            return ticket;
        }

        /// <summary>解析验票字符串，获取用户和角色信息</summary>
        /// <param name="ticketString">验票字符串</param>
        /// <param name="id">用户id</param>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <returns>表单验证票据对象</returns>
        private static FormsAuthenticationTicket ParseTicket(string ticketString, out string id, out string user, out string[] roles)
        {
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticketString);
            user = authTicket.Name;

            // 解析附属信息
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
