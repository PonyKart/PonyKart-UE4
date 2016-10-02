/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/*jshint loopfunc: true */

define(function(require , exports , module) {
  "use strict";

  var extensionID = "viewerZIP"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["zip"];
  var maxPreviewSize = (1024 * 3) || {}; //3kb limit for preview
  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var JSZip = require("jszip");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  var zip2htmlConverter;
  var containerElID;
  var currentFilePath;
  var $containerElement;

  function init(filePath , containerElementID) {
    console.log("Initalization Browser ZIP Viewer...");
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
    TSCORE.IO.getFileContentPromise(filePath).then(function(content) {
      exports.setContent(content);
    } , function(error) {
      TSCORE.hideLoadingAnimation();
      TSCORE.showAlertDialog("Loading " + filePath + " failed.");
      console.error("Loading file " + filePath + " failed " + error);
    });
  }

  function viewerMode() {

    console.log("viewerMode not supported on this extension");
  }

  function setContent(content) {
    var zipFile = new JSZip(content);
    console.log(zipFile);

    var fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(currentFilePath);
    if (isWeb) {
      fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(location.href) + "/" + fileDirectory;
    }

    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.setContent === "function") {
      contentWindow.setContent(zipFile , fileDirectory);
    } else {
      //// TODO optimize setTimeout
      window.setTimeout(function() {
        contentWindow.setContent(zipFile , fileDirectory);
      } , 500);
    }
  }

  function getContent() {
    console.log("getContent not supported on this extension");
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  //exports.setFileType = setFileType;

});