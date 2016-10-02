/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  // GFM https://help.github.com/articles/github-flavored-markdown

  var extensionID = "viewerMD"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["md", "markdown", "mdown"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var md2htmlConverter;
  var containerElID;
  var currentFilePath;
  var $containerElement;
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath, containerElementID) {
    console.log("Initalization MD Viewer...");
    containerElID = containerElementID;
    $containerElement = $('#' + containerElID);

    currentFilePath = filePath;
    $containerElement.empty();
    $containerElement.css("background-color", "white");
    $containerElement.append($('<iframe>', {
      sandbox: "allow-same-origin allow-scripts allow-modals",
      id: "iframeViewer",
      "nwdisable": "",
      //"nwfaketop": "",
      "src": extensionDirectory + "/index.html?&locale=" + TSCORE.currentLanguage,
    }));

    require([
      extensionDirectory + '/libs/marked/lib/marked.js',
    ], function(marked) {
      md2htmlConverter = marked;
      md2htmlConverter.setOptions({
        renderer: new marked.Renderer(),
        //highlight: function (code) {
        //    //return require([extensionDirectory+'/highlightjs/highlight.js']).highlightAuto(code).value;
        //},
        gfm: true,
        tables: true,
        breaks: false,
        pedantic: false,
        smartLists: true,
        smartypants: false
      });
      
      TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
        exports.setContent(content);
      }, 
      function(error) {
        TSCORE.hideLoadingAnimation();
        TSCORE.showAlertDialog("Loading " + filePath + " failed.");
        console.error("Loading file " + filePath + " failed " + error);
      });
    });
  }

  function setFileType() {

    console.log("setFileType not supported on this extension");
  }

  function viewerMode(isViewerMode) {

    // set readonly
  }

  function setContent(content) {
    var UTF8_BOM = "\ufeff";

    // removing the UTF8 bom because it brakes thing like #header1 in the beginning of the document
    if (content.indexOf(UTF8_BOM) === 0) {
      content = content.substring(1, content.length);
    }

    var fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(currentFilePath);

    if (isWeb) {
      fileDirectory = TSCORE.TagUtils.extractContainingDirectoryPath(location.href) + "/" + fileDirectory;
    }

    var cleanedContent = content.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, "");
    var mdContent = md2htmlConverter(cleanedContent);

    var contentWindow = document.getElementById("iframeViewer").contentWindow;
    if (typeof contentWindow.setContent === "function") {
      contentWindow.setContent(mdContent, fileDirectory);
    } else {
      // TODO optimize setTimeout
      window.setTimeout(function() {
        contentWindow.setContent(mdContent, fileDirectory);
      }, 500);
    }
  }
  function getContent() {
    //$('#'+containerElID).html();
    console.log("Not implemented");
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  exports.setFileType = setFileType;
  
});
