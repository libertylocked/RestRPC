function sendInput(targetComponent, cmd, args, callback, errorCallback) {
  var stringified = JSON.stringify({ "Target": targetComponent, "Cmd": cmd, "Args" : args });
  console.log(stringified);
  $.ajax({
    url: '/input',
    type: 'POST',
    dataType: 'text',
    data: stringified,
    cache: false,
    timeout: 2000,
    success: callback,
    error: errorCallback,
  });
}
