﻿// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using Newtonsoft.Json;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Persisting
{
    public class Identity
    {
        public static Identity Global;

        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        [JsonIgnore]
        public string MailAddress { get; set; }

        public string Account { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public string PhpSessionId { get; set; }

        public DateTime CookieCreation { get; set; }

        public static Identity Parse(string password, TokenResponse token)
        {
            var response = token.ToResponse;
            return new Identity
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id.ToString(),
                MailAddress = response.User.MailAddress,
                Account = response.User.Account,
                Password = password
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), ToString());
            CredentialManager.SaveCredentials(AppContext.AppIdentifier, new NetworkCredential(MailAddress, Password));
        }

        public static async Task Restore()
        {
            Global = (await File.ReadAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), Encoding.UTF8)).FromJson<Identity>();
            var credential = CredentialManager.GetCredentials(AppContext.AppIdentifier);
            Global.MailAddress = credential.UserName;
            Global.Password = credential.Password;
        }

        public static bool ConfExists()
        {
            var path = Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName);
            return File.Exists(path) && new FileInfo(path).Length != 0 && CredentialManager.GetCredentials(AppContext.AppIdentifier) != null;
        }

        public static bool AppApiRefreshRequired(Identity identity)
        {
            return identity.ExpireIn <= DateTime.Now;
        }

        public static bool WebApiRefreshRequired(Identity identity)
        {
            return (DateTime.Now - identity.CookieCreation).Days >= 7;
        }

        public static async Task RefreshIfRequired()
        {
            if (Global == null) await Restore();

            var conf = (await File.ReadAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), Encoding.UTF8)).FromJson<Identity>();

            async Task RefreshAppApi()
            {
                if (AppApiRefreshRequired(conf))
                {
                    if (Global?.RefreshToken.IsNullOrEmpty() is true)
                        await Authentication.AppApiAuthenticate(Global?.MailAddress, Global?.Password);
                    else
                        await Authentication.AppApiAuthenticate(Global?.RefreshToken);
                }
            }

            async Task RefreshWebApi()
            {
                if (WebApiRefreshRequired(conf)) await Authentication.WebApiAuthenticate(Global?.MailAddress, Global?.Password);
            }

            await Task.WhenAll(RefreshAppApi(), RefreshWebApi());
        }

        public static void Clear()
        {
            File.Delete(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName));
            CredentialManager.RemoveCredentials(AppContext.AppIdentifier);
            Global = new Identity();
        }
    }
}