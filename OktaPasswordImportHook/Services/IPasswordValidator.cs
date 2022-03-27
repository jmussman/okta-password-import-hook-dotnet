// IAuthenticator
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

using System;

namespace OktaPasswordImportHook.Services;

public interface IPasswordValidator
{

    public bool Validate(string username, string password);
}
