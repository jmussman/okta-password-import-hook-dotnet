// PrincipalContextProxyDomainBuilderTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHookTest.Unit.Services;

using System.DirectoryServices.AccountManagement;
using Xunit;

using OktaPasswordImportHook.Services;

// Disable platform checking for Windows-only features; a PlatformNotSupportedException will be thrown if the configuration is bad.
#pragma warning disable CA1416 // Validate platform compatibility

public class PrincipalContextProxyDomainBuilderTest {

    // Creating an actual PrincipalContext attempts a connection to the domain server, which if the current computer is
    // not a domain member will fail.
    //[Fact]
    //public void ReturnsDomainPrincipalContext() {
    //    IPrincipalContextProxyBuilder principalContextProxyBuilder = new PrincipalContextDomainProxyBuilder();
    //    IPrincipalContextProxy principalContextProxy = principalContextProxyBuilder.Build();
    //    Assert.Equal(ContextType.Domain, ((PrincipalContext)principalContextProxy).ContextType);
    //}
}

#pragma warning restore CA1416 // Validate platform compatibility