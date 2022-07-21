// LdapBuilderServiceTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This may seem trivial, but the tests here check that the builder handles the parameters correctly. These
// are simple, positive-only tests because the library code is not being tested and testing bad parameters is irrelevant.
//

using System.DirectoryServices.Protocols;
using Xunit;

using OktaPasswordImportHook.Services;
using System.Security.Cryptography.X509Certificates;

namespace OktaPasswordImportHookTest.Unit.Services;

public class LdapSessionOptionsProxyTest {


    private LdapDirectoryIdentifier ldapDirectoryIdentifier;
    private LdapConnection ldapConnection;
    private LdapSessionOptionsProxy ldapSessionOptionsProxy;

    public LdapSessionOptionsProxyTest() {

        string server = "dev-77167726.ldap.okta.com";
        int port = 389;
        bool fullyQualifiedDnsHostname = true;
        bool connectionless = false;

        ldapDirectoryIdentifier = new LdapDirectoryIdentifier(server, port, fullyQualifiedDnsHostname, connectionless);
        ldapConnection = new LdapConnection(ldapDirectoryIdentifier);
        ldapSessionOptionsProxy = new LdapSessionOptionsProxy(ldapConnection.SessionOptions);
    }

    [Fact]
    public void LdapProtocol2AttachedToSessionOptions() {

        ldapSessionOptionsProxy.ProtocolVersion = 2;

        Assert.Equal(2, ldapConnection.SessionOptions.ProtocolVersion);
    }

    [Fact]
    public void LdapProtocol3AttachedToSessionOptions() {

        ldapSessionOptionsProxy.ProtocolVersion = 3;

        Assert.Equal(3, ldapConnection.SessionOptions.ProtocolVersion);
    }

    [Fact]
    public void VerifyServerCertificateAttachedToSessionOptions() {

        // This test is isolated to Windows, on the Mac it will respond with "The LDAP server is unavailabe"
        // because certificate verification does not work in that environment and cannot be tested.

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {

            VerifyServerCertificateCallback callback = (LdapConnection connection, X509Certificate certificate) => { return true; };

            ldapSessionOptionsProxy.VerifyServerCertificate = callback;

            Assert.Equal(callback, ldapConnection.SessionOptions.VerifyServerCertificate);
        }
    }

    // Without a way to mock this method there is not any way to check it is called with the correct parameter.
    //[Fact]
    //public void StartTransportLayerSecurity() {
    //}
}
