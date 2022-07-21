// LdapBuilderServiceTest
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This may seem trivial, but the tests here check that the builder handles the parameters correctly. These
// are simple, positive-only tests because the library code is not being tested and testing bad parameters is irrelevant.
//

namespace OktaPasswordImportHookTest.Unit.Services;

internal class PrincipalContextProxyTest {

    // Without the ability to mock the ValidateCredentials method on a PrincipalContext object there are no
    // meaningful tests.
    //[Fact]
    //public void ValidateCredentials() {
    //}
}