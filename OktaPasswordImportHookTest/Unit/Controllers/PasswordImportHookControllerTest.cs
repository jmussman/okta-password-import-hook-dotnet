// PasswordImportHookControllerTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

namespace OktaPasswordImportHookTest.Unit.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using OktaPasswordImportHook.Controllers;
using OktaPasswordImportHook.Services;
using OktaPasswordImportHook.Dtos;

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
    private HttpContext httpContext;
    private Mock<IConfiguration> configurationMock;
    private Mock<ILogger<PasswordImportHookController>> loggerMock;
    private Mock<IPasswordValidator> passwordValidatorMock;
    private dynamic ?requestData;
    private PasswordImportHookController controller;

    public PasswordImportHookControllerTest() {

        authenticationField = "mydomain-authentication";
        authenticationSecret = "secret";

        httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[authenticationField] = authenticationSecret;

        configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Hook:AuthenticationField"]).Returns(authenticationField);
        configurationMock.Setup(x => x["Hook:AuthenticationSecret"]).Returns(authenticationSecret);

        // Mocking the functionality of the logger for things like verification is not really
        // possible becuase the Log methods are defined as extension methods. We'll just mock
        // a logger and ignore it.

        loggerMock = new Mock<ILogger<PasswordImportHookController>>();
        // loggerMock.Setup(x => x.LogError(It.IsAny<string>())).Verifiable();

        // The only thing exposed in the validator is Validate.

        passwordValidatorMock = new Mock<IPasswordValidator>();
        passwordValidatorMock.Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Setup the JsonDocument for the call.

        requestData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

        controller = new PasswordImportHookController(configurationMock.Object, loggerMock.Object, passwordValidatorMock.Object) {

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

        passwordValidatorMock.Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        ActionResult<DynamicResponse> response = controller.Authenticate(requestData);

        Assert.NotNull(response.Value);

        if (response.Value != null) {

            Assert.Equal("com.okta.action.update", (string)response.Value.Response.commands[0].type);
            Assert.Equal("UNVERIFIED", (string)response.Value.Response.commands[0].value.credential);
        }
    }

    [Fact]
    public void AuthorizationFieldMissingDenied() {

        authenticationField = "wrong-fieldname";
        authenticationSecret = "secret";

        httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[authenticationField] = authenticationSecret;

        controller = new PasswordImportHookController(configurationMock.Object, loggerMock.Object, passwordValidatorMock.Object) {

            ControllerContext = new ControllerContext() {
                HttpContext = httpContext,
            }
        };

        ActionResult<DynamicResponse> response = controller.Authenticate(requestData);

        Assert.IsType<UnauthorizedResult>(response.Result);
    }

    [Fact]
    public void AuthorizationWrongSecretDenied() {

        authenticationField = "mydomain-authentication";
        authenticationSecret = "password";

        httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[authenticationField] = authenticationSecret;

        controller = new PasswordImportHookController(configurationMock.Object, loggerMock.Object, passwordValidatorMock.Object) {

            ControllerContext = new ControllerContext() {
                HttpContext = httpContext,
            }
        };

        ActionResult<DynamicResponse> response = controller.Authenticate(requestData);

        Assert.IsType<UnauthorizedResult>(response.Result);
    }
}