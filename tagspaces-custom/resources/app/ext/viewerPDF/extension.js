/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "viewerPDF"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["pdf"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath, elementID) {
    console.log("Initalization Browser PDF Viewer...");
    $('#' + elementID).append($('<iframe>', {
      id: "iframeViewer",
      src: "libs/pdfjs/web/viewer.html?file=" + encodeURIComponent(filePath),
      "nwdisable": "",
      "nwfaketop": ""
    }));
  }

  function viewerMode() {

    console.log("viewerMode not supported on this extension");
  }

  function setContent() {

    console.log("setContent not supported on this extension");
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
