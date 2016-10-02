/* Copyright (c) 2013-2016 The TagSpaces Authors. All rights reserved.
 * Use of this source code is governed by a AGPL3 license that
 * can be found in the LICENSE file. */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "editorODF"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["odt", "ods"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;

  function init(filePath, elementID) {
    console.log("Initalization ODF Viewer/Editor...");
    //filePath = "file:///"+filePath;
    //filePath = "http://www.webodf.org/demos/presentation/ohm2013.odp";
    var extPath = extensionDirectory + "/index.html";
    $('#' + elementID).append($('<iframe>', {
      id: "iframeViewer",
      sandbox: "allow-same-origin allow-scripts",
      style: "background-color: white",
      src: extPath + "?cp=" + filePath,
      "nwdisable": "",
      "nwfaketop": ""
    }));

    /*require([extensionDirectory+'/webodf/webodf.js'], function() {
        var odfelement = document.getElementById(elementID),
        odfcanvas = new odf.OdfCanvas(odfelement);
        odfcanvas.load(filePath);
    });*/
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

});
