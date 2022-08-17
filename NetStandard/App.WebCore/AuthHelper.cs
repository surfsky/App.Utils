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
    /// 表单鉴权辅助函数（将用户、角色等信息用加密字符串保存在cookie中）。
    /// （1）Login 创建验票，并将用户角色过期时间等信息加密保存在cookie中。
    /// （2）LoadPrincipal 从cookie解析验票并设置当前登录人信息。
    /// （3）Logout 注销
    /// </summary>
    public class AuthHelper
    {
        //-----------------------------------------------
        // 登录注销
        //-----------------------------------------------
        /// <summary>登录（设置当前用户，并创建用户验票Cookie）。</summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="roleIds">角色名称列表</param>
        /// <param name="expiration">验票到期时间</param>
        /// <example>AuthHelper.Login("123", "Admin", "1,2,3", DateTime.Now.AddDays(1));</example>
        public static ClaimsPrincipal Login(string userId, string userName, string roleIds, DateTime expiration)
        {
            // Aspnetcore 标准登录代码（Claim-Identity-Principal-Ticket, 属性-身份-主角-验票）
            var claims = new[] { new Claim("UserID", userId), new Claim("UserName", userName), new Claim("RoleIDs", roleIds) }; // 属性
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);  // 身份
            var principal = new ClaimsPrincipal(identity); // 主角
            Asp.Current.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties() { IsPersistent = false }
                );
            return principal;
        }

        /// <summary>注销。销毁验票</summary>
        public static void Logout()
        {
            Asp.Current.SignOutAsync();
            Asp.Current.Session.Clear();
        }

        //-----------------------------------------------
        // 用户信息
        //-----------------------------------------------
        /// <summary>当前用户</summary>
        public static ClaimsPrincipal User => Asp.Current.User;

        /// <summary>是否登录</summary>
        public static bool IsLogin()
        {
            return (Asp.Current.User != null && Asp.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>当前登录用户名</summary>
        public static string GetUserId()
        {
            return IsLogin() ? Asp.Current.User.Identity.Name : "";
        }
        /// <summary>当前登录用户名</summary>
        public static string GetUserName()
        {
            if (IsLogin())
                return Asp.Current.User.Claims.Where(x => x.Type == "UserName").FirstOrDefault().Value;
            return "";
        }

        /// <summary>当前登录用户的角色列表</summary>
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

        /// <summary>当前登录用户是否具有某个角色</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return Asp.Current.User.IsInRole(role);
            return false;
        }
    }
}
