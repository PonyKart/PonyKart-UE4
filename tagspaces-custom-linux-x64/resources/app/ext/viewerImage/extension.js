/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require , exports , module) {
  "use strict";

  var extensionID = "viewerImage"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["jpeg" , "jpg" , "png" , "gif" , "bmp" , "ico" , "webp"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
  var UI;
  var currentFilePath;
  var $containerElement;

  function init(filePath , elementID) {
    console.log("Initalization Browser Image Viewer...");

    $containerElement = $('#' + elementID);
    currentFilePath = filePath;

    var lng = $.i18n.lng();

    $containerElement.append($('<iframe>' , {
      id: "iframeViewer" ,
      sandbox: "allow-same-origin allow-scripts allow-modals" ,
      scrolling: "no" ,
      type: "content" ,
      src: extensionDirectory + "/index.html?cp=" + encodeURIComponent(filePath) + "&setLng=" + lng ,
      "nwdisable": "" ,
      "nwfaketop": ""
    }));

    /* $("#iframeViewer")
     .hammer().on("swipeleft", function() {
     TSCORE.FileOpener.openFile(TSCORE.PerspectiveManager.getNextFile(internPath));
     })
     .hammer().on("swiperight", function() {
     TSCORE.FileOpener.openFile(TSCORE.PerspectiveManager.getPrevFile(internPath));
     }); */
  }

  function viewerMode(isViewerMode) {

    console.log("viewerMode not supported on this extension");
  }

  function setContent(content) {

    console.log("setContent not supported on this extension");
  }

  function getContent() {

    console.log("getContent not supported on this extension");
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;

});
