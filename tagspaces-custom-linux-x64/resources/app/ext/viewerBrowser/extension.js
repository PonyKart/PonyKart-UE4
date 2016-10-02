/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "viewerBrowser"; // ID should be equal to the directory name where the ext. is located
  var extensionType = "viewer";

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var containerElID,
    $containerElement,
    currentFilePath;
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath, containerElementID) {
    console.log("Initalization Text Viewer...");
    containerElID = containerElementID;
    $containerElement = $('#' + containerElID);

    currentFilePath = filePath;

    var filePathURI;
    if (isCordova || isWeb) {
      filePathURI = filePath;
    } else {
      filePathURI = "file:///" + filePath;
    }

    var fileExt = TSCORE.TagUtils.extractFileExtension(filePath);

    $containerElement.empty();
    $containerElement.css("background-color", "white");

    if ((fileExt.indexOf("htm") === 0 || fileExt.indexOf("xhtm") === 0 || fileExt.indexOf("txt") === 0) && !isFirefox) {
      $containerElement.append($('<iframe>', {
        sandbox: "allow-same-origin allow-scripts",
        id: "iframeViewer",
        "nwdisable": "",
        "nwfaketop": ""
      }));
      
      TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
        exports.setContent(content);
      }, 
      function(error) {
        TSCORE.hideLoadingAnimation();
        TSCORE.showAlertDialog("Loading " + filePath + " failed.");
        console.error("Loading file " + filePath + " failed " + error);
      });
    } else {
      $containerElement.append($('<iframe>', {
        // sandbox: "allow-same-origin allow-scripts", // comment out due not loading pdfs in chrome ext
        id: "iframeViewer",
        src: filePathURI,
        "nwdisable": "",
        "nwfaketop": ""
      }));
    }
  }

  function setFileType(fileType) {

    console.log("setFileType not supported on this extension");
  }

  function viewerMode(isViewerMode) {

    // set readonly      
  }

  function setContent(content) {
    // removing the script tags from the content 
    var cleanedContent = content.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, "");
    var viewerIframe = $("#iframeViewer").get(0);

    var fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(currentFilePath);

    if (viewerIframe !== undefined) {
      viewerIframe.contentWindow.document.write(cleanedContent);

      // making all links open in the user default browser
      $(viewerIframe.contentWindow.document).find("a").bind('click', function(e) {
        e.preventDefault();
        TSCORE.IO.openFile($(this).attr("href"));
      });

      // fixing embedding of local images
      $(viewerIframe.contentWindow.document).find("img[src]").each(function() {
        var currentSrc = $(this).attr("src");
        if (currentSrc.indexOf("http://") === 0 ||
            currentSrc.indexOf("https://") === 0 ||
            currentSrc.indexOf("file://") === 0 ||
            currentSrc.indexOf("data:") === 0) {
          // do nothing if src begins with http(s):// or data:
        } else {
          $(this).attr("src", "file://" + fileDirectory + "/" + currentSrc);
        }
      });

      $(viewerIframe.contentWindow.document).find("a[href]").each(function() {
        var currentSrc = $(this).attr("href");
        if (currentSrc.indexOf("http://") === 0 ||
            currentSrc.indexOf("https://") === 0 ||
            currentSrc.indexOf("file://") === 0 ||
            currentSrc.indexOf("data:") === 0) {
          // do nothing if src begins with http(s):// or data:
        } else {
          var path = "file://" + fileDirectory + "/" + currentSrc;
          $(this).attr("href", path);
        }
      });

    }
  }

  function getContent() {

    console.log("Not implemented");
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  exports.setFileType = setFileType;

});
