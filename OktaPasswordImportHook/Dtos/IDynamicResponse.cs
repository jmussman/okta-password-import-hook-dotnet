// IDynamicResponse
// Copyright © 2022 Joel A Mussman. All rights reserved.
//
// Controllers do not play nicely returning dynamic objects. Return ActionResult<dynamic> will
// create a Value that points back to the ActionResult object, creatng a loop. If you put
// the result into "var" it reads right in the debeugger, but the loop shows up during
// evaluation.
//
// This interface defines a class which wraps the dynamic response in a static object,
// which does pass back correclty as an ActionResult<DynamicResponse>.
//

namespace OktaPasswordImportHook.Dtos;

public interface IDynamicResponse {

    dynamic Response { get; set; }
}