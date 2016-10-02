/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "viewerMHTML"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["mht", "mhtml"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var MailParser = require("ext/viewerMHTML/mailparser/mailparser.min").MailParser;
  var currentFilePath;
  var $containerElement;
  var contentIFrame;
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
  var filePathURI;

  function init(filePath, containerElementID) {
    console.log("Initalization MHTML Viewer...");
    $containerElement = $('#' + containerElementID);
    currentFilePath = filePath;
    
    if (isCordova || isWeb) {
      filePathURI = filePath;
    } else {
      filePathURI = "file:///" + filePath;
    }

    $containerElement.empty();
    $containerElement.css("background-color", "white");

    var extUITmpl = Handlebars.compile(
      '<iframe id="{{id}}Viewer" sandbox="allow-same-origin allow-scripts allow-modals" style="background-color: white; border: 0px;" class="flexMaxHeight" nwdisable="" src="{{extDir}}/index.html?&locale={{lang}}"></iframe>'
    );

    var extUI = extUITmpl({
      id: extensionID,
      lang: TSCORE.currentLanguage,
      extDir: extensionDirectory
    });
    $containerElement.append(extUI);

    contentIFrame = document.getElementById(extensionID + "Viewer").contentWindow;
    contentIFrame.MailParser = MailParser;

    TSCORE.IO.loadTextFilePromise(filePath).then(function(content) {
      exports.setContent(content);
    }, 
    function(error) {
      TSCORE.hideLoadingAnimation();
      TSCORE.showAlertDialog("Loading " + filePath + " failed.");
      console.error("Loading file " + filePath + " failed " + error);
    });
  }

  function setFileType() {
    console.log("setFileType not supported on this extension");
  }

  function viewerMode(isViewerMode) {
    // set readonly
  }

  function setContent(content) {
    if (typeof contentIFrame.setContent === "function" && contentIFrame.MailParser) {
      contentIFrame.setContent(content, filePathURI);
    } else {
      window.setTimeout(function() {
        contentIFrame.setContent(content, filePathURI);
      }, 500);
    }
  }

  function getContent() {
    /*TSCORE.IO.getFileContentPromise(file).then(function(buf) {
      var mailparser = new MailParser();

      var text = TSCORE.Utils.arrayBufferToStr(buf);
      mailparser.on("end", function(parsedObject) {
        //console.log(parsedObject);
        var matched = parsedObject.html.match(/<body[^>]*>([\w|\W]*)<\/body>/im);
        console.log($(matched[1]).text());
      });

      mailparser.write(text);
      mailparser.end();

    }, function(err) {
      console.log(err);
    });*/
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
  exports.setFileType = setFileType;

});
