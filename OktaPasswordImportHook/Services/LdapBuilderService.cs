// LdapBuilderService
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// The LdapBuilderService exists to eliminates the new calls from the validation
// service and allows testing via the ILdapBuilder interface.
//

using System.DirectoryServices.Protocols;
using System.Net;

namespace OktaPasswordImportHook.Services;

public class LdapBuilderService : ILdapBuilder {

    public ILdapConnectionProxy LdapConnection(LdapDirectoryIdentifier identifier) {

        return new LdapConnectionProxy(identifier);
    }

    public LdapDirectoryIdentifier LdapDirectoryIdentifier(string server, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless) {

        return new LdapDirectoryIdentifier(server, portNumber, fullyQualifiedDnsHostName, connectionless);
    }

    public NetworkCredential NetworkCredential(string username, string password) {

        return new NetworkCredential(username, password);
    }
}