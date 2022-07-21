// LdapBuilderTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This may seem trivial, but the tests here check that the builder handles the parameters correctly. These
// are simple, positive-only tests because the library code is not being tested and testing bad parameters is irrelevant.
//

namespace OktaPasswordImportHookTest.Unit.Services;

using Xunit;

using OktaPasswordImportHook.Services;
using System.Net;
using System.DirectoryServices.Protocols;

public class LdapBuilderTest {

    string username;
    string password;
    string server;
    int port;
    bool fullyQualifiedDnsHostname;
    bool connectionless;
    ILdapBuilder builder;

    public LdapBuilderTest() {

        username = "annebonny@potc.live";
        password = "P!rates17";
        server = "dev-77167726.ldap.okta.com";
        port = 389;
        fullyQualifiedDnsHostname = true;
        connectionless = false;

        builder = new LdapBuilder();
    }

    [Fact]
    public void LdapConnectionMatchesIdentifier() {

        LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(server, port, fullyQualifiedDnsHostname, connectionless);

        ILdapConnectionProxy ldapConnectionProxy = builder.LdapConnection(identifier);

        Assert.Equal(identifier, ldapConnectionProxy.Directory);
    }

    [Fact]
    public void BuildsIdentifierWithTrue() {

        LdapDirectoryIdentifier directoryIdentifier = builder.LdapDirectoryIdentifier(server, port, fullyQualifiedDnsHostname, connectionless);

        Assert.Equal(server, directoryIdentifier.Servers[0]);
        Assert.Equal(port, directoryIdentifier.PortNumber);
        Assert.Equal(fullyQualifiedDnsHostname, directoryIdentifier.FullyQualifiedDnsHostName);
        Assert.Equal(connectionless, directoryIdentifier.Connectionless);
    }

    [Fact]
    public void BuildsIdentifierWithFalse() {

        string server = "dev-77167726.ldap.okta.com";
        int port = 389;
        bool fullyQualifiedDnsHostname = false;
        bool connectionless = false;

        LdapDirectoryIdentifier directoryIdentifier = builder.LdapDirectoryIdentifier(server, port, fullyQualifiedDnsHostname, connectionless);

        Assert.Equal(server, directoryIdentifier.Servers[0]);
        Assert.Equal(port, directoryIdentifier.PortNumber);
        Assert.Equal(fullyQualifiedDnsHostname, directoryIdentifier.FullyQualifiedDnsHostName);
        Assert.Equal(connectionless, directoryIdentifier.Connectionless);
    }

    [Fact]
    public void BuildsNetworkCredential() {

        NetworkCredential networkCredential = builder.NetworkCredential(username, password);

        Assert.Equal(username, networkCredential.UserName);
        Assert.Equal(password, networkCredential.Password);
    }
}
