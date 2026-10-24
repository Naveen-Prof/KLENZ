using Microsoft.AspNetCore.Http;
using System;

namespace KLENZ.Services
{
    public class CompanyContext : ICompanyContext
    {
        private const string CompanyIdKey = "SelectedCompanyId";
        private const string CompanyNameKey = "SelectedCompanyName";
        private const string CompanyShortNameKey = "SelectedCompanyShortName";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public int? CurrentCompanyId
        {
            get
            {
                if (HttpContext == null) return null;

                // 1. Check Session
                try
                {
                    var sessionVal = HttpContext.Session.GetInt32(CompanyIdKey);
                    if (sessionVal.HasValue && sessionVal.Value > 0)
                    {
                        return sessionVal.Value;
                    }
                }
                catch
                {
                    // Session might not be initialized yet
                }

                // 2. Fallback to Cookie
                if (HttpContext.Request.Cookies.TryGetValue(CompanyIdKey, out var cookieVal) &&
                    int.TryParse(cookieVal, out int id) && id > 0)
                {
                    try
                    {
                        HttpContext.Session.SetInt32(CompanyIdKey, id);
                    }
                    catch { }
                    return id;
                }

                return null;
            }
        }

        public string? CurrentCompanyName
        {
            get
            {
                if (HttpContext == null) return null;

                try
                {
                    var sessionVal = HttpContext.Session.GetString(CompanyNameKey);
                    if (!string.IsNullOrEmpty(sessionVal)) return sessionVal;
                }
                catch { }

                if (HttpContext.Request.Cookies.TryGetValue(CompanyNameKey, out var cookieVal) && !string.IsNullOrEmpty(cookieVal))
                {
                    try
                    {
                        HttpContext.Session.SetString(CompanyNameKey, cookieVal);
                    }
                    catch { }
                    return cookieVal;
                }

                return null;
            }
        }

        public string? CurrentCompanyShortName
        {
            get
            {
                if (HttpContext == null) return null;

                try
                {
                    var sessionVal = HttpContext.Session.GetString(CompanyShortNameKey);
                    if (!string.IsNullOrEmpty(sessionVal)) return sessionVal;
                }
                catch { }

                if (HttpContext.Request.Cookies.TryGetValue(CompanyShortNameKey, out var cookieVal) && !string.IsNullOrEmpty(cookieVal))
                {
                    try
                    {
                        HttpContext.Session.SetString(CompanyShortNameKey, cookieVal);
                    }
                    catch { }
                    return cookieVal;
                }

                return null;
            }
        }

        public bool HasSelectedCompany => CurrentCompanyId.HasValue && CurrentCompanyId.Value > 0;

        public void SetCompany(int id, string fullName, string shortName)
        {
            if (HttpContext == null) return;

            try
            {
                HttpContext.Session.SetInt32(CompanyIdKey, id);
                HttpContext.Session.SetString(CompanyNameKey, fullName ?? string.Empty);
                HttpContext.Session.SetString(CompanyShortNameKey, shortName ?? string.Empty);
            }
            catch { }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };

            HttpContext.Response.Cookies.Append(CompanyIdKey, id.ToString(), cookieOptions);
            HttpContext.Response.Cookies.Append(CompanyNameKey, fullName ?? string.Empty, cookieOptions);
            HttpContext.Response.Cookies.Append(CompanyShortNameKey, shortName ?? string.Empty, cookieOptions);
        }

        public void ClearCompany()
        {
            if (HttpContext == null) return;

            try
            {
                HttpContext.Session.Remove(CompanyIdKey);
                HttpContext.Session.Remove(CompanyNameKey);
                HttpContext.Session.Remove(CompanyShortNameKey);
            }
            catch { }

            HttpContext.Response.Cookies.Delete(CompanyIdKey);
            HttpContext.Response.Cookies.Delete(CompanyNameKey);
            HttpContext.Response.Cookies.Delete(CompanyShortNameKey);
        }
    }
}
