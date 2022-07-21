// PasswordImportHookController
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHookTest.Integration.Controllers;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using OktaPasswordImportHook.Controllers;
using OktaPasswordImportHook.Services;
using Microsoft.AspNetCore.Mvc;
using OktaPasswordImportHook.Dtos;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;

public class PasswordImportHookControllerTest {

    // If you think this JSON request looks familiar, see:
    // https://developer.okta.com/docs/reference/password-hook/#sample-json-payload-of-request.

    private string json = @"
        {
          'eventId': '3o9jBzq1SmOGmmsDsqyyeQ',
          'eventTime': '2020-01-17T21:23:56.000Z',
          'eventType': 'com.okta.user.credential.password.import',
          'eventTypeVersion': '1.0',
          'contentType': 'application/json',
          'cloudEventVersion': '0.1',
          'source': 'https://mydomain.okta.com/api/v1/inlineHooks/cal2xd5phv9fsPLcF0g7',
          'data': {
            'context': {
              'request': {
                'id': 'XiIl6wn7005Rr@fjYqeC7AAABxw',
                'method': 'POST',
                'url': {
                  'value': '/api/v1/authn'
                },
                'ipAddress': '98.124.153.138'
              },
              'credential': {
                'username': 'annebonny@potc.live',
                'password': 'P!rates17'
              }
            },
            'action': {
              'credential': 'UNVERIFIED'
            }
          }
        }
        ";

    private string authenticationField;
    private string authenticationSecret;
    private Mock<IConfiguration> configurationMock;
    private PasswordImportHookController controller;
    private HttpContext httpContext;
    private ILdapBuilder ldapBuilder;
	private Mock<ILogger<LdapPasswordValidatorService>> ldapPasswordValidatorServiceLoggerMock;
	private Mock<ILogger<PasswordImportHookController>> passwordImportHookControllerLoggerMock;
	private IPasswordValidator passwordValidator;
    private dynamic? requestData;

    public PasswordImportHookControllerTest() {

        // Credentials.

        authenticationField = "mydomain-authentication";
        authenticationSecret = "secret";

        // HTTP context for the call.

        httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[authenticationField] = authenticationSecret;

        // Setup the JsonDocument for the call.

        requestData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

        // We could build a configuration that reads appsettings.json, but it's easier just to build the mock :) This test
        // will actually run as long as dev-77167726.ldap.okta.com is running, annebonny@potc.live and P!rates17 are a valid
        // user in that Okta tenant.

        configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Ldap:Server"]).Returns("dev-77167726.ldap.okta.com");
        configurationMock.Setup(x => x["Ldap:Port"]).Returns("389");
        configurationMock.Setup(x => x["Ldap:StartTls"]).Returns("true").Verifiable();
        configurationMock.Setup(x => x["Ldap:Base"]).Returns("ou=users,dc=dev-77167726,dc=okta,dc=com");
        configurationMock.Setup(x => x["Ldap:Identifier"]).Returns("uid");
        configurationMock.Setup(x => x["Ldap:VerifyServerCertificate"]).Returns(
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false");
        configurationMock.Setup(x => x["Hook:AuthenticationField"]).Returns(authenticationField);
        configurationMock.Setup(x => x["Hook:AuthenticationSecret"]).Returns(authenticationSecret);

        // Set up a real password validator service.

        ldapPasswordValidatorServiceLoggerMock = new Mock<ILogger<LdapPasswordValidatorService>>();
		ldapBuilder = new LdapBuilder();
		passwordValidator = new LdapPasswordValidatorService(configurationMock.Object, ldapPasswordValidatorServiceLoggerMock.Object, ldapBuilder);

        // Set up a real controller.

		passwordImportHookControllerLoggerMock = new Mock<ILogger<PasswordImportHookController>>();
        controller = new PasswordImportHookController(configurationMock.Object, passwordImportHookControllerLoggerMock.Object, passwordValidator) {

            ControllerContext = new ControllerContext() {

                HttpContext = httpContext,
            }
        };
    }


    [Fact]
    public void AuthenticationSuccess() {

        // Do not put this into an ActionResult<dynamic>, the Value will become another ActionResult<dynamic>.

        ActionResult<DynamicResponse> response = controller.Authenticate(requestData);

        Assert.NotNull(response.Value);

        if (response.Value != null) {

            Assert.Equal("com.okta.action.update", (string)response.Value.Response.commands[0].type);
            Assert.Equal("VERIFIED", (string)response.Value.Response.commands[0].value.credential);
        }
    }

    [Fact]
    public void AuthenticationFailure() {

        if (requestData != null) {

            // The null check is only because requestData is nullable via Newtonsoft, which is not going to happen. But
            // if it did, the test will fail anyways because the object is null.

            requestData.data.context.credential.password = "bad-password";
        }

        ActionResult<DynamicResponse> response = controller.Authenticate(requestData);

        Assert.NotNull(response.Value);

        if (response.Value != null) {

            Assert.Equal("com.okta.action.update", (string)response.Value.Response.commands[0].type);
            Assert.Equal("UNVERIFIED", (string)response.Value.Response.commands[0].value.credential);
        }
    }
}