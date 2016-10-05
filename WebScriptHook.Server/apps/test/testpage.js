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
  $("#sendResultTitle").text("Sending...");
  $("#sendResultBody").text("");
  var targetComponent = $("#targetComponent").val();
  var cmd = $("#cmd").val();
  var arg = $("#arg").val();
  if (!cmd) {
    $("#sendResultTitle").text("Not sent");
    $("#sendResultBody").text("Empty cmd");
    return;
  };
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
  // Finally compose a request and send to server
  sendInput(targetComponent, cmd, args, function(data) {
    $("#sendResultTitle").text("Sent");
    $("#sendResultBody").text(data);
  }, function(jqXHR, textStatus, errorThrown) {
    $("#sendResultTitle").text("Error");
    $("#sendResultBody").text(errorThrown.stack);
  });
}
