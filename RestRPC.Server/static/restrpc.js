function sendInput(tid, method, params, id, callback, errorCallback) {
  var stringified = JSON.stringify({ "TID": tid, "Method": method, "Params": params, "ID": id });
  console.log(stringified);
  $.ajax({
    url: '/input',
    type: 'POST',
    dataType: 'text',
    data: stringified,
    cache: false,
    timeout: 10000,
    success: callback,
    error: errorCallback,
  });
}
