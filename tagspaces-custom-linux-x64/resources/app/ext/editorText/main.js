/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */
/* globals marked, Mousetrap */
"use strict";

var isCordova;
var isWin;
var isWeb;

$(document).ready(function() {
  function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  }

  var locale = getParameterByName("locale");
  var $htmlContent = $("#editorText");
  var extSettings;
  loadExtSettings();

  isCordova = parent.isCordova;
  isWin = parent.isWin;
  isWeb = parent.isWeb;


  $('#saveEditorText').on('click', function(e) {
    saveEditorText();
  });

  // Init internationalization
  $.i18n.init({
    ns: {namespaces: ['ns.editorText']},
    debug: true,
    lng: locale,
    fallbackLng: 'en_US'
  }, function() {
    $('[data-i18n]').i18n();
  });

  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("editorTextSettings"));
  }

  $($htmlContent).bind('keydown', function(event) {
    if (event.ctrlKey || event.metaKey) {
      switch (String.fromCharCode(event.which).toLowerCase()) {
        case 's':
          event.preventDefault();
          saveEditorText();
          break;
      }
    }
  });

  var filePath;

  function saveEditorText() {
    var msg = {command: "saveDocument", filepath: filePath};
    window.parent.postMessage(JSON.stringify(msg), "*");
  }

});

var editorText;
var cmEditor;

function setContent(content, filePath) {

  var $htmlContent = $("#editorText");
  $htmlContent.append('<div id="code" style="width: 100%; height: 100%; z-index: 9999;">');

  var filetype = [];
  filetype.h = "clike";
  filetype.c = "clike";
  filetype.clj = "clojure";
  filetype.coffee = "coffeescript";
  filetype.cpp = "clike";
  filetype.cs = "clike";
  filetype.css = "css";
  filetype.groovy = "groovy";
  filetype.haxe = "haxe";
  filetype.htm = "xml";
  filetype.html = "xml";
  filetype.java = "clike";
  filetype.js = "javascript";
  filetype.jsm = "javascript";
  filetype.json = "javascript";
  filetype.less = "less";
  filetype.lua = "lua";
  filetype.markdown = "markdown";
  filetype.md = "markdown";
  filetype.mdown = "markdown";
  filetype.mdwn = "markdown";
  filetype.mkd = "markdown";
  filetype.ml = "ocaml";
  filetype.mli = "ocaml";
  filetype.pl = "perl";
  filetype.php = "php";
  filetype.py = "python";
  filetype.rb = "ruby";
  filetype.sh = "shell";
  filetype.sql = "sql";
  filetype.svg = "xml";
  filetype.xml = "xml";
  filetype.txt = "txt";

  var fileExt = filePath.substring(filePath.lastIndexOf(".") + 1, filePath.length).toLowerCase();

  var mode = filetype[fileExt];
  var extensionDirectory;
  var modePath;
  if (mode) {
    modePath = extensionDirectory + "/libs/codemirror/mode/" + mode + "/" + mode;
  }

  var cursorBlinkRate = isViewer ? -1 : 530; // disabling the blinking cursor in readonly mode
  var isViewerMode = !isViewer;

  var place = document.getElementById("code");
  cmEditor = new CodeMirror(place, {
    mode: mode,
    lineNumbers: isViewerMode,
    cursorBlinkRate: cursorBlinkRate,
    //readOnly: isViewer ? "nocursor" : isViewer,
    readOnly: isViewer ? true : false,
    foldGutter: isViewerMode,
    gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"],
    styleActiveLine: isViewerMode,
    lineWrapping: true,
    tabSize: 2,
    //lineSeparator: isWin ? "\n\r" : null, // TODO check under windows if content contains \n\r -> set
    //collapseRange: isViewerMode,
    matchBrackets: isViewerMode,
    styleSelectedText: true,
    autofocus: true
    //theme: "lesser-dark",
    //extraKeys: keys // workarrounded with bindGlobal plugin for mousetrap
  });

  CodeMirror.modeURL = "libs/codemirror/mode/%N/%N.js";
  if (mode) {
    cmEditor.setOption("mode", mode);
    CodeMirror.autoLoadMode(cmEditor, mode);
  } else {
    throw new TypeError("Invalid mode !");
  }

  CodeMirror.on(cmEditor, "inputRead", function() {
    if (!isViewer) {
      var msg = {command: "contentChangedInEditor", filepath: filePath};
      window.parent.postMessage(JSON.stringify(msg), "*");
    }
  });

  CodeMirror.on(cmEditor, "changes", function() {
    if (cmEditor.readOnly === true) {
     $('.CodeMirror-cursor').hide();
    } else {
      $('.CodeMirror-cursor').show();
    }
  });

  cmEditor.setSize("100%", "100%");

  var UTF8_BOM = "\ufeff";
  if (content.indexOf(UTF8_BOM) === 0) {
    content = content.substring(1, content.length);
  }
  cmEditor.setValue(content);
  cmEditor.clearHistory();
  cmEditor.refresh();
}
