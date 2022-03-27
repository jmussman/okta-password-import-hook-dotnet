// LdapPasswordValidatorService
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHook.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.DirectoryServices.Protocols;

public class LdapPasswordValidatorService : IPasswordValidator {

    private readonly IConfiguration configuration;
    private readonly ILdapBuilder ldapBuilder;
    private readonly ILogger<LdapPasswordValidatorService> logger;

    public LdapPasswordValidatorService(IConfiguration configuration, ILogger<LdapPasswordValidatorService> logger, ILdapBuilder ldapBuilder) {

        this.configuration = configuration;
        this.logger = logger;
        this.ldapBuilder = ldapBuilder;
    }

    public bool Validate(string username, string password) {

        return Authenticate(BuildUserDn(username), password);
    }

    private string BuildUserDn(string username) {

        string basedn = configuration["Ldap:Base"];
        string identifier = configuration["Ldap:Identifier"];
        string result;

        // Generally an LDAP server requires a distinguished name to authenticate
        // a user, but Without a prefix it's just the username for Active Directory.

        if (identifier.Length == 0) {

            result = username;

        } else {

            result = String.Format("{0}={1},{2}", identifier, username, basedn);
        }

        return result;
    }

    private bool Authenticate(string username, string password) {


        bool result = false;

        try {

            string server = configuration["Ldap:Server"];
            int port = Int32.Parse(configuration["Ldap:Port"]);
            bool starttls = Boolean.Parse(configuration["Ldap:StartTls"]);
            bool verifyServerCertificate = Boolean.Parse(configuration["Ldap:VerifyServerCertificate"]);

            // The ILdapBuilder wraps the code that would instantiate LDAP components directly.

            LdapDirectoryIdentifier identifier = ldapBuilder.LdapDirectoryIdentifier(server, port, false, false);
            ILdapConnectionProxy ldapConnectionProxy = ldapBuilder.LdapConnection(identifier);

            // The ILdapConnectionProxy wraps the LdapConnection class so that this code may be unit tested.

            ldapConnectionProxy.SessionOptions.ProtocolVersion = 3;
            ldapConnectionProxy.Credential = ldapBuilder.NetworkCredential(username, password);
            ldapConnectionProxy.AuthType = AuthType.Basic;

            if (starttls) {

                // We aren't worried about the certificate for this example. However, per
                // https://github.com/dotnet/runtime/issues/60972 at the time of creating this module,
                // overriding the certificate verification on Mac and Linux is not supported
                // and causes a general LdapException. As of 3/16/2022 this may be fixed but has not made it
                // into through NuGet get. Set the configuration parameter in appsettings.json with caution.

                if (verifyServerCertificate) {

                    ldapConnectionProxy.SessionOptions.VerifyServerCertificate = (conn, cert) => { return true; };
                }

                // Invoke STARTTLS when binding.

                ldapConnectionProxy.SessionOptions.StartTransportLayerSecurity(null);
            }

            ldapConnectionProxy.Bind();
            ldapConnectionProxy.Dispose();

            result = true;
        }

        catch (LdapException e) {

            logger.LogError(String.Format("Unable to login: {0}", e.Message));
        }

        catch (Exception e) {

            logger.LogError(String.Format("Unexpected exception occured: {0}" + e.Message));
        }

        return result;
    }
}