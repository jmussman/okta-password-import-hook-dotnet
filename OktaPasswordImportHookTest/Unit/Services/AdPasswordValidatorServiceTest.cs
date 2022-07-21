// AdPasswordValidatorServiceTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHookTest.Unit.Services;

using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using Xunit;

using OktaPasswordImportHook.Services;

// Disable platform checking for Windows-only features, we still have the Fact logic skipped below with "if" statements.
#pragma warning disable CA1416 // Validate platform compatibility

public class AdPasswordValidatorServiceTest {

    private string username;
    private string password;
    private Mock<IPrincipalContextProxy> principalContextProxyMock;
    private Mock<IPrincipalContextProxyBuilder> principalContextProxyBuilderMock;
    private Mock<ILogger<AdPasswordValidatorService>> loggerMock;
    private AdPasswordValidatorService adPasswordValidatorService;

    public AdPasswordValidatorServiceTest() {

        username = "annebonny@potc.live";
        password = "P!rates17";

        principalContextProxyMock = new Mock<IPrincipalContextProxy>();
        principalContextProxyBuilderMock = new Mock<IPrincipalContextProxyBuilder>();

        principalContextProxyBuilderMock.Setup(x => x.Build()).Returns(principalContextProxyMock.Object);
        
        loggerMock = new Mock<ILogger<AdPasswordValidatorService>>();
        adPasswordValidatorService = new AdPasswordValidatorService(principalContextProxyBuilderMock.Object, loggerMock.Object);
    }

    [Fact]
    public void ValidationSuccessful() {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {

            principalContextProxyMock.Setup(x => x.ValidateCredentials(username, password, ContextOptions.Negotiate)).Returns(true);

            Assert.True(adPasswordValidatorService.Validate(username, password));
        }
    }

    [Fact]
    public void PasswordValidationFailure() {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {

            principalContextProxyMock.Setup(x => x.ValidateCredentials(username, password, ContextOptions.Negotiate)).Returns(false);

            Assert.False(adPasswordValidatorService.Validate(username, password));
        }
    }

    [Fact]
    public void HandlesPlatformNotSupportedException() {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {

            principalContextProxyMock.Setup(x => x.ValidateCredentials(username, password, ContextOptions.Negotiate)).Throws(new PlatformNotSupportedException());

            Assert.False(adPasswordValidatorService.Validate(username, password));
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility