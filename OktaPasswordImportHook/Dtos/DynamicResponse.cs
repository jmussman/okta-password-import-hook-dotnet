// DynamicResponse
// Copyright © Joel A Mussman. All rights reserved.
//
// Controllers do not play nicely returning dynamic objects. Return ActionResult<dynamic> will
// create a Value that points back to the ActionResult object, creatng a loop. If you put
// the result into "var" it reads right in the debeugger, but the loop shows up during
// evaluation.
//
// This wraps the dynamic response in a static object, which does pass back correclty as
// an ActionResult<DynamicResponse>.
//

namespace OktaPasswordImportHook.Dtos;

public class DynamicResponse {

    public DynamicResponse(dynamic response) {

        Response = response;
    }

    public dynamic Response { get; set; }
}