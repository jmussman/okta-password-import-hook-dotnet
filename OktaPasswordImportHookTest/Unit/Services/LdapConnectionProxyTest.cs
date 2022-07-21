// LdapBuilderServiceTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This may seem trivial, but the tests here check that the builder handles the parameters correctly. These
// are simple, positive-only tests because the library code is not being tested and testing bad parameters is irrelevant.
//

using Xunit;

using System.DirectoryServices.Protocols;
using OktaPasswordImportHook.Services;

namespace OktaPasswordImportHookTest.Unit.Services;

public class LdapConnectionProxyTest {

    private LdapDirectoryIdentifier ldapDirectoryIdentifier;
    private LdapConnectionProxy ldapConnectionProxy;

    public LdapConnectionProxyTest() {
        
        string server = "dev-77167726.ldap.okta.com";
        int port = 389;
        bool fullyQualifiedDnsHostname = true;
        bool connectionless = false;

        ldapDirectoryIdentifier = new LdapDirectoryIdentifier(server, port, fullyQualifiedDnsHostname, connectionless);
        ldapConnectionProxy = new LdapConnectionProxy(ldapDirectoryIdentifier);
    }

    [Fact]
    public void AuthTypeBasicAttachedToConnection() {

        ldapConnectionProxy.AuthType = AuthType.Basic;

        Assert.Equal(ldapConnectionProxy.AuthType, ldapConnectionProxy.connection.AuthType);
    }

    [Fact]
    public void AuthTypeNegotiateAttachedToConnection() {

        ldapConnectionProxy.AuthType = AuthType.Negotiate;

        Assert.Equal(ldapConnectionProxy.AuthType, ldapConnectionProxy.connection.AuthType);
    }

    [Fact]
    public void DirectoryIdentifierPassedToConnection() {

        Assert.Equal(ldapDirectoryIdentifier, ldapConnectionProxy.connection.Directory);
    }

    // Untestable - Credential is not a readable property to check.
    //[Fact]
    //public void NetworkCredentialPassedToConnection() {
    //}
}
