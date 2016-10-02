/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* globals JSONEditor, marked */
"use strict";

var isCordova;
var isWin;
var isWeb;
var jsonEditor;
var isViewer = true;
var filePath;

$(document).ready(function() {
  function getParameterByName(name) {
    name = name.replace(/[\[]/ , "\\\[").replace(/[\]]/ , "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)");
    var results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g , " "));
  }

  var locale = getParameterByName("locale");

  var extSettings;
  loadExtSettings();

  isCordova = parent.isCordova;
  isWin = parent.isWin;
  isWeb = parent.isWeb;

  $('#markdownHelpModal').on('show.bs.modal' , function() {
    $.ajax({
      url: 'libs/jsoneditor/docs/shortcut_keys.md' ,
      type: 'GET'
    }).done(function(jsonData) {
      //console.log("DATA: " + mdData);
      if (marked) {
        var modalBody = $("#markdownHelpModal .modal-body");
        modalBody.html(marked(jsonData , {sanitize: true}));
        handleLinks(modalBody);
      } else {
        console.log("markdown to html transformer not found");
      }
    }).fail(function(data) {
      console.warn("Loading file failed " + data);
    });
  });

  function handleLinks($element) {
    $element.find("a[href]").each(function() {
      var currentSrc = $(this).attr("href");
      $(this).bind('click' , function(e) {
        e.preventDefault();
        var msg = {command: "openLinkExternally" , link: currentSrc};
        window.parent.postMessage(JSON.stringify(msg) , "*");
      });
    });
  }

  $("#jsonHelpButton").on("click", function(e) {
    $("#markdownHelpModal").modal({show: true});
  });

  // Init internationalization
  $.i18n.init({
    ns: {namespaces: ['ns.editorJSON']} ,
    debug: true ,
    lng: locale ,
    fallbackLng: 'en_US'
  } , function() {
    $('[data-i18n]').i18n();
  });

  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("viewerJSONSettings"));
  }
});

function contentChanged() {
  //console.log('Content changed');
  var msg = {command: "contentChangedInEditor" , filepath: filePath};
  window.parent.postMessage(JSON.stringify(msg) , "*");
}

function getContent() {
  if (jsonEditor) {
    return JSON.stringify(jsonEditor.get());
  }
}

function setContent(jsonContent , path) {
  filePath = path;
  try {
    jsonContent = JSON.parse(jsonContent);
  } catch (e) {
    console.log("Error parsing JSON document. " + e);
    return false;
  }

  var options = {
    search: true ,
    history: true ,
    mode: isViewer ? 'view' : "tree" ,
    //modes: ['code' , 'form' , 'text' , 'tree' , 'view'] , // allowed modes
    onError: function(err) {
      alert(err.toString());
    } ,
    onChange: contentChanged ,
  };

  var container = document.getElementById('jsonEditor');

  if (!!Object.keys(jsonContent) &&
          (typeof jsonContent !== 'function' ||
          jsonContent === null)) {
    //console.debug(Object.keys(jsonContent));
    jsonEditor = new JSONEditor(container , options , jsonContent);
  } else {
    throw new TypeError("Object.keys called on non-object");
  }
}

function viewerMode(isViewerMode) {
  isViewer = isViewerMode;
  if (isViewerMode) {
    jsonEditor.setMode('view');
  } else {
    jsonEditor.setMode('tree');
  }
}