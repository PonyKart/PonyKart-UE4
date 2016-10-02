/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "editorText"; // ID should be equal to the directory name where the ext. is located
  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var currentFilePath;
  var $containerElement;
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath, containerElementID, isViewer) {
    console.log("Initalization Text Editor...");
    $containerElement = $('#' + containerElementID);

    currentFilePath = filePath;
    $containerElement.empty();
    $containerElement.css("background-color", "white");
    $containerElement.append($('<iframe>', {
      sandbox: "allow-same-origin allow-scripts allow-modals",
      id: "iframeViewer",
      "nwdisable": "",
      "src": extensionDirectory + "/index.html?&locale=" + TSCORE.currentLanguage
    }));

    TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
      setContent(content, isViewer);
      //viewerMode(isViewer);
    }, function(error) {
      TSCORE.hideLoadingAnimation();
      TSCORE.showAlertDialog("Loading " + filePath + " failed.");
      console.error("Loading file " + filePath + " failed " + error);
    });

  }

  function viewerMode(isViewerMode) {

  }

  function setContent(content, isViewer) {
    var UTF8_BOM = "\ufeff";
    if (content.indexOf(UTF8_BOM) === 0) {
      content = content.substring(1, content.length);
    }

    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.setContent === "function") {
      contentWindow.require = require;
      contentWindow.isViewer = isViewer;
      contentWindow.extensionDirectory = extensionDirectory;
      contentWindow.setContent(content, currentFilePath);
    } else {
      window.setTimeout(function() {
        if (typeof contentWindow.setContent === "function") {
          contentWindow.require = require;
          //var isViewerMode = true;
          contentWindow.isViewer = isViewer;
          contentWindow.extensionDirectory = extensionDirectory;
          contentWindow.setContent(content, currentFilePath);
        }
      }, 500);
    }
  }

  function getContent() {
    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    return contentWindow.cmEditor.getValue();
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  //exports.viewerMode = viewerMode;

});

