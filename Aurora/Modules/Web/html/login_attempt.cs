﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Services.Interfaces;

namespace Aurora.Modules.Web
{
    public class LoginAttemptPage : IWebInterfacePage
    {
        public string FilePath { get { return "html/login_attempt.html"; } }
        
        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, Hashtable query, OSHttpResponse httpResponse,
            Dictionary<string, object> requestParameters, ITranslator translator)
        {
            var vars = new Dictionary<string, object>();

            if (query.ContainsKey("username") && query.ContainsKey("password"))
            {
                string username = query["username"].ToString().Replace('+', ' ');
                string password = query["password"].ToString();

                ILoginService loginService = webInterface.Registry.RequestModuleInterface<ILoginService>();
                if (loginService.VerifyClient(UUID.Zero, username, "UserAccount", password, UUID.Zero))
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.Redirect;
                    UUID sessionID = UUID.Random();
                    Authenticator.AddAuthentication(sessionID, UUID.Zero);
                    httpResponse.AddCookie(new System.Web.HttpCookie("SessionID", sessionID.ToString()) { Expires = DateTime.MinValue, Path = "" });

                    httpResponse.AddHeader("Location", "/welcomescreen/index.html");
                }
            }

            return vars;
        }
    }
}
