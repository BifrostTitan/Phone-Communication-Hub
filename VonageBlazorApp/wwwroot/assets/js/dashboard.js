function Toggle_call_popup() 
{
  $('#callPopup').toggle();
}
function startCall()
{
    let destination = document.getElementById("phone-number").value;
    if (destination !== "") {
        app.callServer(destination);
    } else {
        statusElement.innerText = 'Please enter your phone number.';
    }
}

$(document).ready(function(){
    
    $('#showPopup, .popupCall').click( function(e) {
        e.stopPropagation();
        $('#callPopup').toggle();
        
    });

    
    $('mainContainer').click( function() {
        $('#callPopup').hide();
    });
    
});

$(document).ready(function(){
    $('.adminUser').click( function(e) {
    e.stopPropagation();
    $('.profileMenu').toggle();
    
});

$('.profileMenu').click( function(e) {
    e.stopPropagation(); 
});

$('body').click( function() {
    $('.profileMenu').hide();
});


});

$(document).ready(function() {
    $("#emojionearea1").emojioneArea();
  });
  
    $(document).ready(function() {
    $("#emojionearea2").emojioneArea();
  });

  $(document).ready(function(){
    $(".dialBtn").click(function(){
      $(".dialPad").toggleClass("dialToggle");
    });
  });
  
  $(document).ready(function(){
    $(".dialBtn").click(function(){
      $(".dialBtn").toggleClass("selected");
    });
  });
  
  $(document).ready(function(){
    $(".adminUser").click(function(){
      $(".profileMenu").toggleClass("show");
    });
});

$( function() {
    $( "#draggable" ).draggable({ containment: "window" });
  });