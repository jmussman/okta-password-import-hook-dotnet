// ILdapSessionOptionsProxy
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// ILdapSessionOptionsProxy wraps the SessionOptions property underneath the ILdapConnectionProxy,
// which cannot be mocked and is one of the reasons that the connection must be wrapped.
//

using System.DirectoryServices.Protocols;

namespace OktaPasswordImportHook.Services;

public interface ILdapSessionOptionsProxy
{
    int ProtocolVersion { get; set; }
    VerifyServerCertificateCallback VerifyServerCertificate { get; set; }

    void StartTransportLayerSecurity(DirectoryControlCollection? controls);
}