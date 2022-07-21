// LdapPasswordValidatorService
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHook.Services;

using Microsoft.Extensions.Logging;
using System.DirectoryServices.AccountManagement;

// Disable platform checking for Windows-only features; a PlatformNotSupportedException will be thrown if the configuration is bad.
#pragma warning disable CA1416 // Validate platform compatibility

public class AdPasswordValidatorService : IPasswordValidator {

    private readonly IPrincipalContextProxyBuilder principalContextProxyBuilder;
    private readonly ILogger<AdPasswordValidatorService> logger;

    public AdPasswordValidatorService(IPrincipalContextProxyBuilder principalContextProxyBuilder, ILogger<AdPasswordValidatorService> logger) {

        this.principalContextProxyBuilder = principalContextProxyBuilder;
        this.logger = logger;
    }

    public bool Validate(string username, string password) {

        return Authenticate(username, password);
    }

    private bool Authenticate(string username, string password) {

        bool result = false;

        try {

            IPrincipalContextProxy principalContextProxy = principalContextProxyBuilder.Build();
            result = principalContextProxy.ValidateCredentials(username, password, ContextOptions.Negotiate);
        }

        catch (PlatformNotSupportedException e) {

            logger.LogError(String.Format("Unable to login: {0}", e.Message));
        }

        catch (Exception e) {

            logger.LogError(String.Format("Unexpected exception occured: {0}" + e.Message));
        }

        return result;
    }
}

#pragma warning restore CA1416 // Validate platform compatibility