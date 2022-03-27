// LdapConnectionProxyService
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// The LdapConnectionProxyService exists to wrap LdapConnection for testing with
// the ILdapConnectionProxy interface. A new connection is created each
// time the identifier is set.
//
// This class is used by the LdapPasswordValidatorService to interface to the
// LDAP module in such a way that the code is testable because ILdapConnectionProxy
// can be mocked.
//

using System.DirectoryServices.Protocols;
using System.Net;

namespace OktaPasswordImportHook.Services;

public class LdapConnectionProxy : ILdapConnectionProxy {

    private LdapConnection connection;
    private ILdapSessionOptionsProxy sessionOptionsProxy;

    public LdapConnectionProxy(LdapDirectoryIdentifier identifier) {

        connection = new LdapConnection(identifier);
        sessionOptionsProxy = new LdapSessionOptionsProxy(connection.SessionOptions);
    }

    public AuthType AuthType {

        get { return connection.AuthType; }
        set { connection.AuthType = value; }
    }

    public NetworkCredential Credential {

        set { connection.Credential = value; }
    }

    public ILdapSessionOptionsProxy SessionOptions {

        get { return sessionOptionsProxy; }
    }

    public void Bind() {

        connection.Bind();
    }

    public void Dispose() {

        connection.Dispose();
    }
}