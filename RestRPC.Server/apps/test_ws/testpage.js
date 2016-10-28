var ws;

function getTypeDropdown() {
  var data = {
    'string': 'string',
    'number': 'number',
    'bool': 'bool',
  }
  var s = $('<select class="form-control" />');
  for(var val in data) {
    $('<option />', {value: val, text: data[val]}).appendTo(s);
  }
  return s.prop('outerHTML');
}

function addArgsClick() {
  var id = ($('#argsDivs div').length).toString();
  $('#argsDivs').append('<div class="form-group" id="argDiv' + id + '">' + getTypeDropdown() + '<input type="text" class="form-control" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" placeholder="Args[' + id + ']..." id="args' + id + '"></div>');
}

function removeArgsClick() {
  if ($('#argsDivs').length == 0) {
    console.log("No more textbox to remove");
    return false;
  }
  $("#argsDivs div:last").remove();
}

function sendInputClick() {
  var targetComponent = $("#targetComponent").val();
  var cmd = $("#cmd").val();
  var arg = $("#arg").val();
  var args = [];
  var argDivs = $("#argsDivs").children("div");
  for (var i = 0; i < argDivs.length; i++) {
    var aType = $(argDivs[i]).find("select option:selected").text();
    var aData = $(argDivs[i]).find("input").val();
    if (aType == "number") {
      aData = Number(aData);
    } else if (aType == "bool") {
      aData = aData.toLowerCase() == "true" ? true : false;
    }
    args.push(aData);
  }
  var cid = $("#cid").val()
  // Stringify the object then send on WS
  var stringified = JSON.stringify({ "TID": targetComponent, "Method": cmd, "Params": args, "ID": cid });
  console.log("Sending: ", stringified);
  ws.send(stringified);
}

function messageClearClick() {
  $("#wsOutput").html("");
}

function connectClick() {
  // Disable connect button for now
  $("#connectBtn").prop('disabled', true);
  $("#connectBtn").text("Status: Connecting...");
  var uri = $("#uri").val();
  console.log("Connecting to " + uri);
  ws = new WebSocket(uri);
  ws.onopen = ws_onopen;
  ws.onclose = ws_onclose;
  ws.onerror = ws_onerror;
  ws.onmessage = ws_onmessage;
}

function ws_onopen(event) {
  console.log("WS is connected.");
  $("#connectBtn").text("Status: Connected");
  $("#sendBtn").prop("disabled", false);
}

function ws_onclose(event) {
  console.log("WS is closed.");
  $("#connectBtn").text("Status: Closed");
  $("#connectBtn").prop('disabled', false);
  $("#sendBtn").prop("disabled", true);
}

function ws_onerror(event) {
  console.log("WS error!")
  console.log(event);
}

function ws_onmessage(event) {
  var outputs = $("#wsOutput");
  outputs.append(event.data);
  // Scroll to bottom
  outputs.scrollTop(outputs.prop("scrollHeight"));
}