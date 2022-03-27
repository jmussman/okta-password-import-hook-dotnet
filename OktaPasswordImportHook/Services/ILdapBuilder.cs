// ILdapBuilder
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// The ILdapBuilder exists to eliminate the new calls from the validation
// service and allow testing.
//

using System.DirectoryServices.Protocols;
using System.Net;

namespace OktaPasswordImportHook.Services;

public interface ILdapBuilder {

    ILdapConnectionProxy LdapConnection(LdapDirectoryIdentifier identifier);
    LdapDirectoryIdentifier LdapDirectoryIdentifier(string server, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless);
    NetworkCredential NetworkCredential(string username, string password);
}