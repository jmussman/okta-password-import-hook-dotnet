// PrincipalContextProxyBuilder
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This provides a class with a zero-argument constructor to build aPrincipalContext instance of
// the Domain type.
//

namespace OktaPasswordImportHook.Services;

using System.DirectoryServices.AccountManagement;

// Disable platform checking for Windows-only features; a PlatformNotSupportedException will be thrown if the configuration is bad.
#pragma warning disable CA1416 // Validate platform compatibility

public class PrincipalContextDomainProxyBuilder : IPrincipalContextProxyBuilder {

    public IPrincipalContextProxy Build() {

        return new PrincipalContextProxy(ContextType.Domain);
    }
}

#pragma warning restore CA1416 // Validate platform compatibility