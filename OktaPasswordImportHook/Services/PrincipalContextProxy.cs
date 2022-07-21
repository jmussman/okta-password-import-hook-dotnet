// PrincipleContextProxy
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// A wrapper for PrincipleContext that exposes ValidateCredentials for testing.
//

namespace OktaPasswordImportHook.Services;

using System.DirectoryServices.AccountManagement;

// Disable platform checking for Windows-only features; a PlatformNotSupportedException will be thrown if the configuration is bad.
#pragma warning disable CA1416 // Validate platform compatibility

public class PrincipalContextProxy : IPrincipalContextProxy {

    private PrincipalContext principalContext; 

    public PrincipalContextProxy(ContextType contextType) {

        principalContext = new PrincipalContext(contextType);
    }

    public bool ValidateCredentials(String username, String password, ContextOptions options) {

        bool result = principalContext.ValidateCredentials(username, password, options);

        return result;
    }
}

#pragma warning restore CA1416 // Validate platform compatibility