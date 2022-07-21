// IAdPrincipalContextProxy
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// Proxy the PrincipalContext to allow overriding of the the validation method.
//

using System.DirectoryServices.AccountManagement;

namespace OktaPasswordImportHook.Services {

    public interface IPrincipalContextProxy {

        bool ValidateCredentials(String username, String password, ContextOptions options);
    }
}
