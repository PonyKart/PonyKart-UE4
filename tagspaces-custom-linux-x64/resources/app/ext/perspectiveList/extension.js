/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* global define, Handlebars  */
define(function(require, exports, module) {
  "use strict";

  var extensionTitle = "List"; // should be equal to the name in the bower.json
  var extensionID = "perspectiveList"; // ID should be equal to the directory name where the ext. is located   
  var extensionIcon = "fa fa-list-ul"; // icon class from font awesome

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
  var UI;
  var extensionLoaded;

  function init() {
    console.log("Initializing perspective " + extensionID);



    extensionLoaded = new Promise(function(resolve, reject) {
      require([
        "css!" + extensionDirectory + '/extension.css',
        extensionDirectory + '/libs/datatables/media/js/jquery.dataTables.js',
      ], function() {
        require([
          extensionDirectory + '/perspectiveUI.js',
          "text!" + extensionDirectory + '/toolbar.html',
					"marked",
          extensionDirectory + '/libs/natural.js',                    
        ], function(extUI, toolbarTPL, marked) {

          var toolbarTemplate = Handlebars.compile(toolbarTPL);
          UI = new extUI.ExtUI(extensionID);
          UI.buildUI(toolbarTemplate);

          try {
            // TODO refactor translations
            var translation = $.i18n.t("ns.perspectiveList:fileExtension");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('File Ext.')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:fileTitle");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('Title')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:fileTags");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('Tags')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:fileSize");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('Size')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:fileLDTM");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('Last Modified')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:filePath");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('File Path')").text(translation);
            }
            translation = $.i18n.t("ns.perspectiveList:fileName");
            if (translation.length > 0) {
              $('#' + extensionID + 'Container').find("th:contains('File Name')").text(translation);
            }
            $('#' + extensionID + 'Container [data-i18n]').i18n();
            
            //console.log("#aboutExtensionModal: " + document.getElementById("aboutExtensionModal"));          
            $('#aboutExtensionModal').on('show.bs.modal', function() {
              $.ajax({
                url: extensionDirectory + '/README.md',
                type: 'GET'
              })
              .done(function(mdData) {
                //console.log("DATA: " + mdData);
                if (marked) {
                  var modalBody = $("#aboutExtensionModal .modal-body");
                  modalBody.html(marked(mdData, { sanitize: true }));
                  handleLinks(modalBody);
                } else {
                  console.log("markdown to html transformer not found");
                }
              })
              .fail(function(data) {
                console.warn("Loading file failed " + data);
              });
            }); 
            
          } catch (err) {
            console.log("Translating extension failed.");
          }

          if (isCordova) {
            TSCORE.reLayout();
          }
          platformTuning();
          resolve(true);
        });
      });
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

  function platformTuning() {
    if (isCordova) {
      $('#' + extensionID + 'Export2CSVButton').hide();
      //$('#' + extensionID + 'AddFileButton').hide(); // TODO tmp disabled due not working binary saving
    } else if (isChrome) {
      $('#' + extensionID + 'AddFileButton').hide();
      $('#' + extensionID + 'TagButton').hide();
      $('#' + extensionID + 'CopyMoveButton').hide();
      $('#' + extensionID + 'CreateDirectoryButton').hide();
    } else if (isFirefox) {
      $('#' + extensionID + 'AddFileButton').hide(); // Current impl has 0.5mb limit
    }
  }

  function load() {
    console.log("Loading perspective " + extensionID);
    extensionLoaded.then(function() {
      UI.reInit();
    }, function(err) {
      console.warn("Loading extension failed: " + err);
    });
  }

  function clearSelectedFiles() {
    if (UI) {
      UI.clearSelectedFiles();
      UI.handleElementActivation();
    }
  }

  function removeFileUI(filePath) {

    UI.removeFileUI(filePath);
  }

  function updateFileUI(oldFilePath, newFilePath) {

    UI.updateFileUI(oldFilePath, newFilePath);
  }

  function getNextFile(filePath) {

    return UI.getNextFile(filePath);
  }

  function getPrevFile(filePath) {

    return UI.getPrevFile(filePath);
  }

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
