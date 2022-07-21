// PrincipalContextProxyBuilder
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// This provides a class with a zero-argument constructor to build aPrincipalContext instance of
// the Domain type.
//

namespace OktaPasswordImportHook.Services {
    public interface IPrincipalContextProxyBuilder {

        IPrincipalContextProxy Build();
    }
}