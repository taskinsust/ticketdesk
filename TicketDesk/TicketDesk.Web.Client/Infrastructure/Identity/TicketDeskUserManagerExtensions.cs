using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System.Linq;
using TicketDesk.Web.Identity.Model;

namespace TicketDesk.Web.Identity
{
    public static class TicketDeskUserManagerExtensions
    {
        public static UserDisplayInfo GetUserInfo(this TicketDeskUserManager userManager, string userId)
        {
            var u = userManager.Users.ToList().Where(x => x.Id.Equals(userId)).SingleOrDefault();
            if (u == null) return new UserDisplayInfo();
            return new UserDisplayInfo()
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                ContactNumber = u.PhoneNumber
            };
            //return new UserDisplayInfo();
            //cacheCollection = manager.Users.Select(u => new UserDisplayInfo
            //{
            //    Id = u.Id,
            //    Email = u.Email,
            //    DisplayName = u.DisplayName,
            //    ContactNumber = u.PhoneNumber
            //}).ToList();

            //userManager.Users().Select(u => new UserDisplayInfo
            //{
            //    Id = u.Id,
            //    Email = u.Email,
            //    DisplayName = u.DisplayName,
            //    ContactNumber = u.PhoneNumber
            //});
            //return GetUserInfo(userId)
            //return userManager.InfoCache.GetUserInfo(userId) ?? new UserDisplayInfo();
        }
        public static void ConfigureDataProtection(this TicketDeskUserManager userManager, IAppBuilder app)
        {
            //TODO: research DpapiDataProtectionProvider and figure out what the f*** this is supposed to do
            var dataProtectionProvider = app.GetDataProtectionProvider();
            if (dataProtectionProvider != null)
            {
                userManager.UserTokenProvider = new DataProtectorTokenProvider<TicketDeskUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
        }
    }
}