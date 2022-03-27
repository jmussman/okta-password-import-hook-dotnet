// PasswordHookController
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using OktaPasswordImportHook.Dtos;
using OktaPasswordImportHook.Services;

namespace OktaPasswordImportHook.Controllers;

[ApiController]
[Route("[controller]")]
public class PasswordImportHookController : ControllerBase {

    private readonly IConfiguration configuration;
    private readonly ILogger<PasswordImportHookController> logger;
    private IPasswordValidator validator;

    public PasswordImportHookController(IConfiguration configuration, ILogger<PasswordImportHookController> logger, IPasswordValidator validator) {

        this.configuration = configuration;
        this.logger = logger;
        this.validator = validator;
    }

    [HttpGet]
    public string Authenticate() {

        // This is just to show that the API is up and running.

        return "POST an authentication request to /PasswordImportHook!";
    }

    [HttpPost]
    public ActionResult<DynamicResponse> Authenticate(dynamic requestData) {

        // Reject if the request does not have the correct authentication.

        string authenticationField = configuration["Hook:AuthenticationField"];
        string authenticationSecret = configuration["Hook:AuthenticationSecret"];
        string authentication = Request.Headers[authenticationField];

        if (authentication != authenticationSecret) {

            // Log the unauthorized request.

            logger.LogWarning(String.Format("Unauthorized request received from {0}", HttpContext.Connection.RemoteIpAddress?.ToString()));

            return new UnauthorizedResult();
        }

        // Because the request object is flexible mapping it to a strongly-typed C# object
        // is impossible, it's brought as a dynamic object by Newtonsoft.
        //
        // Most of the request data is irrelevant unless there is a reason to look at the
        // "context" and reject attempts based on that, but that would be better left to
        // the Okta sign-on policy.
        //
        // The "action" in the data is irrelevant, the only action is to validate the password:
        //

        string username = requestData.data.context.credential.username;
        string password = requestData.data.context.credential.password;

        bool valid = validator.Validate(username, password);

        // Log the request.

        logger.LogInformation(String.Format("Request received from {0} to authenticate {1}: {2}",
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            username,
            valid ? "VERIFIED" : "UNVERIFIED"));

        // Build the response object for the password import inline hook. Response structure
        // may vary so this is not done with static classes, instead it is done with a dynammic
        // JObject from newtonsoft. The command and credential values are hardwired here
        // because they will never change.

        dynamic response = new JObject();

        response.commands = new JArray();
        response.commands.Add(new JObject());
        response.commands[0].type = "com.okta.action.update";
        response.commands[0].value = new JObject();
        response.commands[0].value.credential = valid ? "VERIFIED" : "UNVERIFIED";

        // The return type is DynamicResponse, a static object wrapping the dynamic response. This
        // is because the controller ActionResult does not play nicely with dynamic objects (see
        // the DynamicResponse class for details).

        return new DynamicResponse(response);
    }
}