// LdapSessionOptionsProxy
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// LdapSessionOptionsProxy is the implementation that wraps the SessionOptions property
// behind the LdapConnectionProxy because LdapSessionOptions is a class which cannot be
// mocked.
//

namespace OktaPasswordImportHook.Services;

using System.DirectoryServices.Protocols;

public class LdapSessionOptionsProxy : ILdapSessionOptionsProxy {

	private LdapSessionOptions sessionOptions;

	public LdapSessionOptionsProxy(LdapSessionOptions sessionOptions)	{

		this.sessionOptions = sessionOptions;
	}

	public int ProtocolVersion {

		get { return sessionOptions.ProtocolVersion; }
		set { sessionOptions.ProtocolVersion = value; }
	}

	public VerifyServerCertificateCallback VerifyServerCertificate {

		get { return sessionOptions.VerifyServerCertificate; }
		set { sessionOptions.VerifyServerCertificate = value; }
	}

	public void StartTransportLayerSecurity(DirectoryControlCollection? controls) {

		sessionOptions.StartTransportLayerSecurity(controls);
    }
}