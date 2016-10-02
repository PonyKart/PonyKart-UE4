/* globals marked */
"use strict";

var isCordova = parent.isCordova;
var $htmlEditor;

function initEditor() {
  var toolbar = [
    ['todo', ['checkbox']],
    ['style', ['style']],
    ['color', ['color']],
    ['font', ['bold', 'italic', 'underline']],
    ['font2', ['superscript', 'subscript', 'strikethrough', 'clear']],
    ['fontname', ['fontname']],
    // ['fontsize', ['fontsize']],
    ['para', ['ul', 'ol', 'paragraph']],
    ['height', ['height']],
    ['table', ['table']],
    ['insert', ['link', 'picture', 'hr']], // 'video',
    ['view', ['codeview']], // 'fullscreen',
    ['help', ['help']]
  ];

  if (isCordova) {
    toolbar = [
      ['todo', ['checkbox']],
      ['color', ['color']],
      ['style', ['style']],
      ['para', ['paragraph', 'ul', 'ol']],
      ['font', ['bold', 'italic', 'underline']],
      //['font2', ['superscript', 'subscript', 'strikethrough', 'clear']],
      ['fontsize', ['fontsize']],
      //['height', ['height']],
      ['insert', ['picture', 'link', 'hr']],
      ['table', ['table']],
      //['view', ['codeview']]
    ];
  }

  $htmlEditor.summernote({
    focus: true,
    height: "200px",
    disableDragAndDrop: true,
    toolbar: toolbar,
    callbacks: {
      onChange: function(contents, $editable) {
        //console.log('onChange:', contents, $editable);
        var msg = {command: "contentChangedInEditor" , filepath: ""};
        window.parent.postMessage(JSON.stringify(msg) , "*");
      }
    }
  });

  // set note-editable panel-body to window height initially and on frame resize
  $(".note-editable.panel-body").height(window.innerHeight - 80);
  $(window).on('resize', function() {
    $(".note-editable.panel-body").height(window.innerHeight - 80);
    //console.log(window.innerHeight);
  });

  /*Mousetrap.bind(['command+s', 'ctrl+s'], function(e) {
    console.log("Fire saving event");
    window.parent.postMessage(JSON.stringify({command: "saveDocument"}) , "*");
    return false;
  });*/
}

$(document).ready(function() {
  function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  }

  var locale = getParameterByName("locale");

  // Init internationalization
  $.i18n.init({
    ns: {namespaces: ['ns.editorHTML']} ,
    debug: true ,
    lng: locale ,
    fallbackLng: 'en_US'
  } , function() {
    $('[data-i18n]').i18n();
  });
});

function setContent(content, currentFilePath) {
  // adjusting relative paths
  //$("base").attr("href", currentFilePath);

  $htmlEditor = $('#htmlEditor');
  $htmlEditor.append(content);

  $htmlEditor.find(".tsCheckBox").each(function() {
    $(this).removeAttr("disabled");
  });

  // Check if summernote is loaded
  if (typeof $htmlEditor.summernote === 'function') {
    initEditor();
  } else {
    // window.setTimeout(initEditor(), 1000);
  }
}
