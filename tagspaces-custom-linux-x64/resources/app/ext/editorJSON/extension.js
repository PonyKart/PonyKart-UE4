/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require , exports , module) {
  "use strict";

  var extensionID = "editorJSON"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["json"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var containerElID;
  var currentFilePath;
  var $containerElement;
  var extensionsPath = TSCORE.Config.getExtensionPath();
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath , containerElementID , isViewer) {
    console.log("Initalization JSON Viewer...");
    containerElID = containerElementID;
    $containerElement = $('#' + containerElID);

    currentFilePath = filePath;
    $containerElement.empty();
    $containerElement.css("background-color" , "white");
    $containerElement.append($('<iframe>' , {
      sandbox: "allow-same-origin allow-scripts allow-modals" ,
      id: "iframeViewer" ,
      "nwdisable": "" ,
      //"nwfaketop": "",
      "src": extensionDirectory + "/index.html?&locale=" + TSCORE.currentLanguage ,
    }));

    TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
      setContent(content);
      viewerMode(isViewer);
    } , function(error) {
      TSCORE.hideLoadingAnimation();
      TSCORE.showAlertDialog("Loading " + filePath + " failed.");
      console.error("Loading file " + filePath + " failed " + error);
    });
  }

  function contentChanged() {

    TSCORE.FileOpener.setFileChanged(true);
  }

  function setFileType(fileType) {

    console.log("setFileType not supported on this extension");
  }

  function viewerMode(isViewerMode) {
    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.viewerMode === "function") {
      contentWindow.viewerMode(isViewerMode);
    } else {
      window.setTimeout(function() {
        if (typeof contentWindow.viewerMode === "function") {
          contentWindow.viewerMode(isViewerMode);
        }
      } , 500);
    }
  }

  function setContent(jsonContent) {
    var UTF8_BOM = "\ufeff";

    if (jsonContent.indexOf(UTF8_BOM) === 0) {
      jsonContent = jsonContent.substring(1 , jsonContent.length);
    }

    try {
      JSON.parse(jsonContent);
    } catch (e) {
      console.log("Error parsing JSON document. " + e);
      TSCORE.FileOpener.closeFile(true);
      TSCORE.showAlertDialog("Error parsing JSON document");
      return false;
    }

    // console.log(jsonContent);
    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.setContent === "function") {
      console.log(jsonContent);
      contentWindow.setContent(jsonContent , currentFilePath);
    } else {
      window.setTimeout(function() {
        if (typeof contentWindow.setContent === "function") {
          contentWindow.setContent(jsonContent);
        }
      } , 500);
    }
  }

  function getContent() {
    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.getContent === "function") {
      return contentWindow.getContent();
    }
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  exports.setFileType = setFileType;
});
