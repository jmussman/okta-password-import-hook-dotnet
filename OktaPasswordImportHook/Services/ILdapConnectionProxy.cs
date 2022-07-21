// ILdapConnectionProxy
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// The ILdapConnectionProxy exists to wrap LdapConnection for testing since
// that class cannot be mocked.
//

using System.DirectoryServices.Protocols;
using System.Net;

namespace OktaPasswordImportHook.Services;

public interface ILdapConnectionProxy {

    AuthType AuthType { get; set; }
    DirectoryIdentifier Directory { get; }
    NetworkCredential Credential { set; }
    ILdapSessionOptionsProxy SessionOptions { get; }
    void Bind();
    void Dispose();
}