![](.common/joels-private-stock.png?raw=true)

# Okta Password Import Hook (.NET)

This solution implements an Okta inline hook for Password Import.
The purpose of the inline hook is to handle the verifification of a password missing in the Okta organization against another directory or database.
If the password is deemed to be correct, then Okta will set the cleartext password it saw as the user's password, this is the *import* part of the scenario.

This solution will run out of the box and the unit and integration tests will pass. The target for password verification is a bind to an LDAP server. To actually use the project the appsettings.json file would have to be configured with a real target server, and the user must be created in the Okta organization with a specification to use the hook: [Create User with Password Import Inline Hook](https://developer.okta.com/docs/reference/api/users/#create-user-with-password-import-inline-hook).

## License

The code is licensed under the MIT license. You may use and modify all or part of it as you choose, as long as attribution to the source is provided per the license. See the details in the [license file](./LICENSE.md) or at the [Open Source Initiative](https://opensource.org/licenses/MIT)

## Software Configuration

The API project targets .NET Core 6 and above.
The unit and integration tests are written with xUnit with Moq.
Open the project in a suitable IDE which supports .NET Core 6 and running tests.

## Project Goals

This project addresses three goals:

* Provide an example of a Password Import inline hook.
* Demonstrate the use of an LDAP server for password verification.
* Show unit and integration testing strategies for unmockable code, e.g. LdapConnection.

### Architecture

The entry point to the API is PasswordImportHookController.
It supports GET and POST operations.
The GET operation is simply a reminder to use POST, and acts as a verification that the API is running.
The POST operation follows the guidelines of the [Password Import Inline Hook Reference](https://developer.okta.com/docs/reference/password-hook/).

The controller uses the LdapPasswordValidatorService class to verify a password.
It implements the IPasswordValidatorService.

The controller is responsible for referencing the JSON request, and building a JSON response.
There is a lot of information in the request that is not necessary, so instead of building a hierarchy of classes to represent it
Newtonsoft Json.NET is used to parse the request into a dynamic object and the username and password are reached directly.
Look at Program.cs to see where Newtonsoft replaces the System.Text.Json default handling.

The same problem exists for the response; the JSON response structure may vary.
The controller uses the Newtonsoft JObject to dynamically build the required structure for response, in place of a complicated hierarchy of classes:

```
dynamic response = new JObject();

response.commands = new JArray();
response.commands.Add(new JObject());
response.commands[0].type = "com.okta.action.update";
response.commands[0].value = new JObject();
response.commands[0].value.credential = valid ? "VERIFIED" : "UNVERIFIED";
```

### ActionResult\<DynamicResponse>

ActionResult\<dynamic> does not play well with dynamic objects.
While it actually evaluates OK in the debugger (in Visual Studio), what happens is that the Value property of the ActionResult
is actually a circular reference to itself and the code to evaluate it in the tests does not resolve.

The solution is to return a static object, and instance of DynamicResponse:

```
public class DynamicResponse {

    public DynamicResponse(dynamic response) {

        Response = response;
    }

    public dynamic Response { get; set; }
}
```

This class has a single property which is the *dynamic* object.
When handled this way, the ActionResult instance has a Value property correctly set to the DynamicResponse instance, where the
*dynamic* response is correctly evaluated.

### LdapConnection and Testing Issues

System.DirectoryServices.Protocols.LdapConnection does not implement an interface.
C# classes are not inherently mockable unless all of the properties and methods are marked as virtual.
The LdapConnection scenario assumes that the client will simply make *new* instances of it and use them to query the LDAP server,
and that is exactly what should *not* happen during unit testing.

To solve this problem, the client code is written to use an interface and wrapper around the LdapConnection, ILdapConnectionProxy
and the implementation LdapConnectionProxyServer.
The class is instantiated and injected into the password validation service:

```
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
```

LdapConnectionProxyServer also needs to wrap the SessionOptions property of type LdapSessionOptions,
since this class has no public constructor and cannot be mocked:

```
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
```

In order to remove the *new* operations from the LdapPasswordValidatorService it is built to depend on an instance of ILdapBuilder
which can be mocked:

```
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
```

The real builder will build real LdapDirectoryIdentifer, LdapConnection wrapped by ILdapConnectionProxy, and NetworkCredential objects.
A mock builder can provide mocks of the LdapDirectoryIdentifier (it is mockable), ILdapConnectionProxy, and the NetworkCredential
(also mockable).

### Integration Testing

An integration test is an end-to-end test.
It may be argued that mocks should never be involved, only the real objects.
However, there are some times that a real dependency may not be used, such as a third-party provider for credit-card processing.

In this case, there are two areas where a mock is advisable: the API configuration and the logger.
It is possible to create a real logger instance, but why bother?
Who would read it when integration tests are run on an integration server?

And, it is certainly possible to build a configuration that reads appsettings.json and serves that to the LdapPasswordValidator.
But again, why bother?
All that proves is that the Microsoft code to read appsettings.json works, and that is not a problem for our tests while this mock
will suffice and is much cleaner:

```
configurationMock = new Mock<IConfiguration>();
configurationMock.Setup(x => x["Ldap:Server"]).Returns("dev-77167726.ldap.okta.com");
configurationMock.Setup(x => x["Ldap:Port"]).Returns("389");
configurationMock.Setup(x => x["Ldap:StartTls"]).Returns("true").Verifiable();
configurationMock.Setup(x => x["Ldap:Base"]).Returns("ou=users,dc=dev-77167726,dc=okta,dc=com");
configurationMock.Setup(x => x["Ldap:Identifier"]).Returns("uid");
configurationMock.Setup(x => x["Ldap:VerifyServerCertificate"]).Returns("false");
configurationMock.Setup(x => x["Hook:AuthenticationField"]).Returns(authenticationField);
configurationMock.Setup(x => x["Hook:AuthenticationSecret"]).Returns(authenticationSecret);
```

Everything else in the integration test is real, including the LDAP server targeted.
The target server is LDAP provided by an Okta organization at dev-77167726.ldap.okta.com.
As long as that Okta development server organization is running LDAP, the integration test will work using a well-known username
and password.

<hr>
Copyright © 2022 Joel Mussman. All rights reserved.