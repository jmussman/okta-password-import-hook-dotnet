// LdapPasswordValidatorServiceTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// Testing any code that leverages the LDAP functionality of System.DirectoryServices
// is difficult, because the code is riddled with non-overridable methods, extension
// methods, and non-existing iterfaces. This test class demonstrates how to work around
// most of the problems.
//

namespace OktaPasswordImportHookTest.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.DirectoryServices.Protocols;
using Xunit;

using OktaPasswordImportHook.Services;

public class LdapPasswordValidatorServiceTest {


    private string username;
    private string password;
    private Mock<IConfiguration> configurationMock;
    private Mock<ILogger<LdapPasswordValidatorService>> loggerMock;
    private Mock<LdapDirectoryIdentifier> ldapDirectoryIdentifierMock;
    private Mock<ILdapSessionOptionsProxy> ldapSessionOptionsProxyMock;
    private Mock<ILdapConnectionProxy> ldapConnectionProxyMock;
    private Mock<ILdapBuilder> ldapBuilderMock;
    private LdapPasswordValidatorService ldapPasswordValidatorService;

    public LdapPasswordValidatorServiceTest() {

        username = "annebonny@potc.live";
        password = "P!rates17";

        configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Ldap:Server"]).Returns("S14053001.smallrock.local");
        configurationMock.Setup(x => x["Ldap:Port"]).Returns("389");
        configurationMock.Setup(x => x["Ldap:StartTls"]).Returns("true").Verifiable();
        configurationMock.Setup(x => x["Ldap:Base"]).Returns("");
        configurationMock.Setup(x => x["Ldap:Identifier"]).Returns("");
        configurationMock.Setup(x => x["Ldap:VerifyServerCertificate"]).Returns("false");

        loggerMock = new Mock<ILogger<LdapPasswordValidatorService>>();

        // Mocking LDAP components is barely possible because of the lack of interfaces,
        // non-overridable methods, and extension methods. Specific problems are that LdapSessionOptions
        // cannot be mocked because of no public constructor, and Bind and Dispose are not overridable in
        // LdapConnection. To make this work three wrappers are used in the code under test (CUT):
        // ILdapBuilder, ILdapConnectionProxy and ILdapSessionOptionsProxy.

        ldapDirectoryIdentifierMock = new Mock<LdapDirectoryIdentifier>("mydirectory.ldap.oktapreview.com", 389, false, false);

        ldapSessionOptionsProxyMock = new Mock<ILdapSessionOptionsProxy>();
        ldapSessionOptionsProxyMock.SetupSet(x => x.VerifyServerCertificate = It.IsAny<VerifyServerCertificateCallback>()).Verifiable();
        ldapSessionOptionsProxyMock.Setup(x => x.StartTransportLayerSecurity(null)).Verifiable();

        ldapConnectionProxyMock = new Mock<ILdapConnectionProxy>();
        ldapConnectionProxyMock.SetupGet(x => x.AuthType).Returns(AuthType.Basic);
        ldapConnectionProxyMock.SetupSet(x => x.AuthType = AuthType.Basic).Verifiable();
        ldapConnectionProxyMock.SetupSet(x => x.Credential = It.IsAny<NetworkCredential>()).Verifiable();
        ldapConnectionProxyMock.SetupGet(x => x.SessionOptions).Returns(ldapSessionOptionsProxyMock.Object);
        ldapConnectionProxyMock.Setup(x => x.Bind()).Verifiable();
        ldapConnectionProxyMock.Setup(x => x.Dispose()).Verifiable();

        // ILdapBuilder allows the CUT to avoid new operators instantiating LDAP components. Note that
        // the mock for NetworkCredential returns null, but that is OK because it isn't used during test.

        ldapBuilderMock = new Mock<ILdapBuilder>();
        ldapBuilderMock.Setup(x => x.LdapConnection(It.IsAny<LdapDirectoryIdentifier>())).Returns(ldapConnectionProxyMock.Object);
        ldapBuilderMock.Setup(x => x.LdapDirectoryIdentifier(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(ldapDirectoryIdentifierMock.Object);
        ldapBuilderMock.Setup(x => x.NetworkCredential(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        ldapPasswordValidatorService = new LdapPasswordValidatorService(configurationMock.Object, loggerMock.Object, ldapBuilderMock.Object);
    }

    [Fact]
    public void ValidationSuccessful() {

        Assert.True(ldapPasswordValidatorService.Validate(username, password));
    }

    [Fact]
    public void LdapPasswordValidationFailure() {

        ldapConnectionProxyMock.Setup(x => x.Bind()).Throws(new LdapException("test exception"));

        Assert.False(ldapPasswordValidatorService.Validate(username, password));
    }

    [Fact]
    public void StartTlsRequested() {

        // Just checking to make sure that the StartTls configuation property was
        // read is not as good as we would like, but it's the best we can do because
        // LdapSessionOptions is not overridable. 

        bool result = ldapPasswordValidatorService.Validate(username, password);

        configurationMock.Verify(m => m[It.Is<string>(s => s == "Ldap:StartTls")]);
    }

    [Fact]
    public void InterceptsCertificateVerification() {

        configurationMock.Setup(x => x["Ldap:VerifyServerCertificate"]).Returns("true");

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapSessionOptionsProxyMock.VerifySet(m => m.VerifyServerCertificate = It.IsAny<VerifyServerCertificateCallback>(), Times.Once);
    }

    [Fact]
    public void SkipsCertificateVerification() {

        configurationMock.Setup(x => x["Ldap:VerifyServerCertificate"]).Returns("false");

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapSessionOptionsProxyMock.VerifySet(m => m.VerifyServerCertificate = It.IsAny<VerifyServerCertificateCallback>(), Times.Never);
    }

    [Fact]
    public void AttemptsTls() {

        configurationMock.Setup(x => x["Ldap:StartTls"]).Returns("true");

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapSessionOptionsProxyMock.Verify(m => m.StartTransportLayerSecurity(null), Times.Once);
    }

    [Fact]
    public void SkipsTls() {

        configurationMock.Setup(x => x["Ldap:StartTls"]).Returns("false");

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapSessionOptionsProxyMock.Verify(m => m.StartTransportLayerSecurity(null), Times.Never);
    }

    [Fact]
    public void BindDnsRequested() {

        string baseDn = "dc=mydomain,dc=com";
        string identifier = "uid";
        string fqUsername = string.Format("{0}={1},{2}", identifier, username, baseDn);

        configurationMock.Setup(x => x["Ldap:Base"]).Returns(baseDn);
        configurationMock.Setup(x => x["Ldap:Identifier"]).Returns(identifier);

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapBuilderMock.Verify(m => m.NetworkCredential(fqUsername, password));
    }

    [Fact]
    public void ShortUsernameRequested() {

        string baseDn = "";
        string identifier = "";

        configurationMock.Setup(x => x["Ldap:Base"]).Returns(baseDn);
        configurationMock.Setup(x => x["Ldap:Identifier"]).Returns(identifier);

        bool result = ldapPasswordValidatorService.Validate(username, password);

        ldapBuilderMock.Verify(m => m.NetworkCredential(username, password));
    }
}
