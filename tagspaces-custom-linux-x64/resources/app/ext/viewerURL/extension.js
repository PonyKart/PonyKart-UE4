/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require , exports , module) {
  "use strict";

  var extensionID = "viewerURL"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["url" , "website" , "desktop"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var containerElID , $containerElement , currentFilePath;
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath , containerElementID) {
    console.log("Initalization URL Viewer...");
    containerElID = containerElementID;
    $containerElement = $('#' + containerElID);

    currentFilePath = filePath;
    $containerElement.empty();
    $containerElement.css("background-color" , "white");
    $containerElement.append($('<iframe>' , {
      "sandbox": "allow-same-origin allow-scripts allow-modals" ,
      "id": "iframeViewer" ,
      "nwdisable": "" ,
      "src": extensionDirectory + "/index.html?&locale=" + TSCORE.currentLanguage ,
    }));

    TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
              exports.setContent(content);
            } ,
            function(error) {
              TSCORE.hideLoadingAnimation();
              TSCORE.showAlertDialog("Loading " + filePath + " failed.");
              console.error("Loading file " + filePath + " failed " + error);
            });
  }

  function setFileType() {
    // TODO
    console.log("setFileType not supported on this extension");
  }

  function viewerMode(isViewerMode) {
    // TODO
    console.log("viewerMode not supported on this extension");
  }

  function setContent(content) {
    var urlBegin = "URL=";
    var url = content.substring(content.indexOf(urlBegin) + urlBegin.length , content.length);

    // preventing the case the url is at the end of the file
    // url = url + "\n";
    // url = url.substring(0, url.indexOf("\n"));
    //console.log("URL substring : " + url );

    var fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(currentFilePath);
    if (isWeb) {
      fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(location.href) + "/" + fileDirectory;
    }

    var urlRegExp = /https?:\/\/([-\w\.]+)+(:\d+)?(\/([\w/_\.]*(\?\S+)?)?)?/;

    if (urlRegExp.test(url)) {
      var contentWindow = document.getElementById("iframeViewer").contentWindow;
      if (typeof contentWindow.setContent === "function") {
        contentWindow.setContent(url , fileDirectory);
      } else {
        // TODO optimize setTimeout
        window.setTimeout(function() {
          contentWindow.setContent(url , fileDirectory);
        } , 500);
      }
    } else {
      TSCORE.showAlertDialog("No URL found in this file.");
    }
  }

  function getContent() {
    // TODO
    console.log("getContent not supported on this extension");
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  exports.setFileType = setFileType;
});
