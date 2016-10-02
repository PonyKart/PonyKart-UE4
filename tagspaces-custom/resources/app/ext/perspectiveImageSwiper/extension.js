/* Copyright (c) 2015-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

define(function(require, exports, module) {
  "use strict";

  var extensionTitle = "ImageSwiper"; // should be equal to the name in the bower.json
  var extensionID = "perspectiveImageSwiper"; // ID should be equal to the directory name where the ext. is located   
  var extensionIcon = "fa fa-image"; // icon class from font awesome

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var marked = require('marked');
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
  var UI;
  var $viewContainer = $("#" + extensionID + "Container");
  var homeScreen;
  var template;
  var UI;
  var aboutContent;
  var extensionLoaded;

  function init() {
    console.log("Initializing perspective " + extensionID);

    $viewContainer = $("#" + extensionID + "Container").empty();

    extensionLoaded = new Promise(function(resolve, reject) {
      require([
        extensionDirectory + "/perspectiveUI.js",
        "text!" + extensionDirectory + "/extension.html",
        "css!" + extensionDirectory + "/libs/photoswipe/dist/photoswipe.css",
        "css!" + extensionDirectory + "/libs/photoswipe/dist/default-skin/default-skin.css",
        "css!" + extensionDirectory + "/extension.css",              
        ], function(perspectiveUI, tmpl) {
          UI = perspectiveUI;
          template = tmpl;
          UI.initUI(extensionDirectory);
          $('#' + extensionID + 'Container [data-i18n]').i18n();
          resolve(true);
        }
      );
    });
  }

  function load() {
    console.log("Loading perspective " + extensionID);
    extensionLoaded.then(function() {
      UI.load($viewContainer, template);
      TSCORE.hideLoadingAnimation();

      if (!aboutContent) {
        $.ajax({
          url: extensionDirectory + '/README.md',
          type: 'GET'
        })
        .done(function(mdData) {
          if (marked) {
            aboutContent = marked(mdData, { sanitize: true });
            var modalBody = $("#aboutExtensionModalImageSwiper .modal-body");
            modalBody.html(aboutContent);
            handleLinks(modalBody);
          }
        })
        .fail(function(data) {
          console.log("Loading about content failed " + data);
        });
      }
    });
  }
  
  function handleLinks($element) {
    $element.find("a[href]").each(function() {
      var currentSrc = $(this).attr("href");
      $(this).bind('click', function(e) {
        e.preventDefault();
        var msg = {command: "openLinkExternally", link : currentSrc};
        window.parent.postMessage(JSON.stringify(msg), "*");
      });
    });
  }  

  function clearSelectedFiles() {}
    
  function removeFileUI(filePath) {}
    
  function updateFileUI(oldFilePath, newFilePath) {}
  
  function getNextFile(filePath) {}

  function getPrevFile(filePath) {}

  function updateTreeData(fsTreeData) {

    console.log("Updating tree data not implemented");
  }

  // API Vars
  exports.Title = extensionTitle;
  exports.ID = extensionID;
  exports.Icon = extensionIcon;

  // API Methods
  exports.init = init;
  exports.load = load;
  exports.clearSelectedFiles = clearSelectedFiles;
  exports.getNextFile = getNextFile;
  exports.getPrevFile = getPrevFile;
  exports.removeFileUI = removeFileUI;
  exports.updateFileUI = updateFileUI;
  exports.updateTreeData = updateTreeData;

});