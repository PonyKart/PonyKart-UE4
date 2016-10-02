/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* global define, Handlebars, isWin  */
define(function(require, exports, module) {
  "use strict";

  console.log("Loading UI for perspectiveList");

  var TSCORE = require("tscore");
  var TSPOSTIO = require("tspostioapi");
  var saveAs = require("libs/filesaver.js/FileSaver.min");

  var TMB_SIZES = ["100px", "200px", "300px", "400px", "500px"];
  var supportedFileTypeThumnailing = ['jpg', 'jpeg', 'png', 'gif'];

  var extensionDirectory;
      
  var selectedIsFolderArr = []; 
  var showFoldersInList = false; 
  var hasFolderInList = false;
  var extSettings;
  loadExtSettings();

  if (extSettings && extSettings.showFoldersInList) {
    showFoldersInList = extSettings.showFoldersInList;
  }
  //save settings for perpectiveGrid
  function saveExtSettings() {
    var settings = {
      "showFoldersInList": showFoldersInList,      
    };
    localStorage.setItem('perpectiveListSettings', JSON.stringify(settings));
  }
  //load settings for perpectiveGrid
  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("perpectiveListSettings"));
  }   
   

  function ExtUI(extID) {
    this.extensionID = extID;
    this.viewContainer = $("#" + this.extensionID + "Container").empty();
    this.viewToolbar = $("#" + this.extensionID + "Toolbar").empty();

    this.thumbEnabled = false;
    this.showFileDetails = false;
    this.showTags = true;
    this.currentTmbSize = 0;

    extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + this.extensionID;
  }

  ExtUI.prototype.buildUI = function(toolbarTemplate) {
    console.log("Init UI module");

    var self = this;

    var context = {
      id: this.extensionID
    };

    // Init Toolbar
    this.viewContainer.append(toolbarTemplate(context));

    $("#" + this.extensionID + "ToogleSelectAll").on("click", function() {
      self.toggleSelectAll();
    });

    $("#" + this.extensionID + "IncludeSubDirsButton").on("click", function() {
      TSCORE.IOUtils.createDirectoryIndex(TSCORE.currentPath);
    });

    $("#" + this.extensionID + "DeleteSelectedFilesButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) {
        return false;
      } else {
        TSCORE.UI.showDeleteFilesDialog();
      }
    });

    $("#" + this.extensionID + "TagButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) { return false; }
      TSCORE.showAddTagsDialog();
    });

    $("#" + this.extensionID + "CopyMoveButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) { return false; }
      TSCORE.showMoveCopyFilesDialog();
    });

    $("#" + this.extensionID + "Export2CSVButton").on("click", function() {
      var blob = new Blob([TSCORE.exportFileListCSV(TSCORE.fileList)], {
        type: "text/csv;charset=utf-8"
      });
      saveAs(blob, "Export.csv");
    });

    $("#" + this.extensionID + "ReloadFolderButton").on("click", function() {
      TSCORE.navigateToDirectory(TSCORE.currentPath);
    });

    $("#" + this.extensionID + "CreateFileButton").on("click", function() {
      TSCORE.showFileCreateDialog();
    });

    $("#" + this.extensionID + "showFoldersInListCheckbox").attr('checked', showFoldersInList);
    $("#" + this.extensionID + "showFoldersInListCheckbox").on("click", function(evt) {
      showFoldersInList = evt.currentTarget.checked;
      saveExtSettings();
    });
    
    $("#modal_button_ok").on("click", function(evt) {
      TSCORE.navigateToDirectory(TSCORE.currentPath);
    });

    $("#" + this.extensionID + "CreateDirectoryButton").on("click", function() {
      TSCORE.showCreateDirectoryDialog(TSCORE.currentPath);
    });

    $("#" + this.extensionID + "CreateHTMLFileButton").on("click", function() {
      TSCORE.createHTMLFile();
    });

    $("#" + this.extensionID + "CreateMDFileButton").on("click", function() {
      TSCORE.createMDFile();
    });

    $("#" + this.extensionID + "CreateTXTFileButton").on("click", function() {
      TSCORE.createTXTFile();
    });

    $("#" + this.extensionID + "AddFileButton").on("click", function() {
      $("#addFileInput").click();
    });

    $("#" + this.extensionID + "MainDropUp").on('click', function() {
      TSCORE.hideAllDropDownMenus();
      //$(this).dropdown('toggle');
    });

    // Disabling all buttons by no data
    this.viewToolbar.find(".btn").prop('disabled', true);

    // Init Toolbar END

    // Init Container

    // Init File Context Menu
    this.viewContainer.on("contextmenu click", ".fileTitleButton", function(e) {
      e.preventDefault();
      TSCORE.hideAllDropDownMenus();
      self.selectFile(this, $(this).attr("filepath"));
      if (!$(this).attr("isDirectory")) {
        TSCORE.showContextMenu("#fileMenu", $(this));
      }
      return false;
    });
    
    this.viewContainer.on("contextmenu mousedown", ".ui-selected td", function(e) {
      if (e.which == 3) {
        //console.warn("right mousedown");
        e.preventDefault();
        TSCORE.hideAllDropDownMenus();
        //self.selectFile(this, $(this).attr("filepath"));
        if (!$(this).attr("isDirectory")) {
          TSCORE.showContextMenu("#fileMenu", $(this));
        }
      }  
      return false;
    });      

    //this.viewContainer.on("contextmenu mousedown", ".ui-selected td", function(e) {
    this.viewContainer.on("contextmenu", ".fileTitle", function(e) {
      var selEl = $(this).parent().find(".fileTitle button");
      //console.warn("right mousedown: " + selEl.attr("filepath"));
      e.preventDefault();
      TSCORE.hideAllDropDownMenus();
      TSCORE.PerspectiveManager.clearSelectedFiles();
      self.selectFile(selEl, selEl.attr("filepath"));
      TSCORE.showContextMenu("#fileMenu", $(this));
      return false;
    });  

    // Init Tag Context Menu
    this.viewContainer.on("contextmenu click", ".tagButton", function(e) {
      e.preventDefault();
      TSCORE.hideAllDropDownMenus();
      self.selectFile(this, $(this).attr("filepath"));
      TSCORE.openTagMenu(this, $(this).attr("tag"), $(this).attr("filepath"));
      TSCORE.showContextMenu("#tagMenu", $(this));
      return false;
    });

    this.viewContainer.append($("<table>", {
      style: "width: 100%",
      class: "table content disableTextSelection",
      id: this.extensionID + "FileTable"
    }));

    this.viewContainer.append($("<div>", {
      style: "width: 100%; text-align: center; display: none;",
    }).append($("<button>", {
      style: "text-align: center; margin-bottom: 10px;",
      class: "btn btn-primary",
      text: "Show all files",
      title: "If you are trying to open more then 1000 files, TagSpaces may experience performance issues.",
      id: this.extensionID + "ShowAllResults"
    }).on("click", function() {
      self.reInit(true);
    })));

    this.fileTable = $('#' + this.extensionID + "FileTable").dataTable({
      "bStateSave": true,
      "iCookieDuration": 60 * 60 * 24 * 365,
      "bJQueryUI": false,
      "bPaginate": false,
      "bLengthChange": false,
      "bFilter": true,
      "bSort": true,
      "bInfo": false,
      "bAutoWidth": false,
      "oLanguage": {
        "sEmptyTable": " " // No files found
      },
      "aoColumns": [
        {
          "sType": 'natural',
          "sTitle": "File Ext.",
          "sClass": "fileTitle noWrap",
          "mRender": function(data, type, row) {
            return self.buttonizeTitle((row.isDirectory ? row.name : row.title), row.path, row.extension, row.isDirectory);
          },
          "mData": "extension",
        }, {
          "sTitle": "Title",
          "sClass": "fileTitle forceWrap fileTitleWidth",
          "mData": "title",
        }, {
          "sTitle": "Tags",
          "sClass": "fileTitle forceWrap",
          "mRender": function(data, type, row) {
            return TSCORE.generateTagButtons(data, row.path);
          },
          "mData": "tags",
        }, {
          "sType": 'numeric',
          "sTitle": "Size",
          "sClass": "fileTitle",
          "mData": "size",
          "mRender": function(data, type, row) {
            return TSCORE.TagUtils.formatFileSize(data, true);
          }
        }, {
          "sTitle": "Last Modified",
          "sClass": "fileTitle",
          "mRender": function(data) {
            return TSCORE.TagUtils.formatDateTime(data, true); // moment(data).fromNow();
          },
          "mData": "lmdt",
        },
      ],
      "aaSorting": [
        [1, "asc"]
      ], // default sorting by filename
    });

    /*
    var dropTarget =  this.viewToolbar;
    //dropTarget.on("dragover", function () { dropTarget.css("border","2px red solid"); return false; });
    //dropTarget.on("dragend", function () { dropTarget.css("border","0px solid"); return false; });
    dropTarget.on("drop", function (e) {
      e.preventDefault();
      var droppedFilePath = e.originalEvent.dataTransfer.files[0].path;
      if( droppedFilePath != undefined){
          var targetFilePath = TSCORE.currentPath+TSCORE.dirSeparator+TSCORE.TagUtils.extractFileName(droppedFilePath);
          TSCORE.showConfirmDialog(
                "Confirm File Move",
                "Do you want to move '"+droppedFilePath+"' to '"+targetFilePath+"'?",
                function() { TSCORE.IO.renameFile(droppedFilePath,targetFilePath); }
            );
      }
      //for (var i = 0; i < e.originalEvent.dataTransfer.files.length; ++i) {
      //  console.log(e.originalEvent.dataTransfer.files[i].path);
      //}
      return false;
    }); */

    // Disable alerts in datatable
    this.fileTable.dataTableExt.sErrMode = 'throw';
  };

  ExtUI.prototype.reInit = function(showAllResult) {
    // Clearing old data
    this.fileTable.fnClearTable();

    if (showAllResult && this.partialResult && this.partialResult.length > 0) {
      this.searchResults = this.allResults;
      this.partialResult = [];
      $("#" + this.extensionID + "ShowAllResults").parent().hide();
    } else {
      this.allResults = TSCORE.Search.searchData(TSCORE.fileList, TSCORE.Search.nextQuery);
      //this.allResults = TSCORE.Search.searchData(arrFolderAndFiles, TSCORE.Search.nextQuery);
      if (this.allResults.length >= TSCORE.Config.getMaxSearchResultCount()) {
        this.partialResult = this.allResults.slice(0, TSCORE.Config.getMaxSearchResultCount());
        $("#" + this.extensionID + "ShowAllResults").parent().show();
        this.searchResults = this.partialResult;
      } else {
        this.searchResults = this.allResults;
        $("#" + this.extensionID + "ShowAllResults").parent().hide();
      }
    }


    function SortByName(a, b) {
      var aName = a.name.toLowerCase();
      var bName = b.name.toLowerCase();       
      return ((aName < bName) ? -1 : ((aName > bName) ? 1 : 0));       
    }    
    
    function SortByIsDirectory(a, b) {
      if (b.isDirectory && a.isDirectory) {  
        return 0;
      }
      return a.isDirectory && !b.isDirectory ? -1 : 1;
    }
 
    //sort by isDirectory in order to show folders on the top of the list
    this.searchResults = this.searchResults.sort(SortByIsDirectory);
    
    if (showFoldersInList && this.searchResults.length > 0 && this.searchResults[0].isDirectory) { //sort by isDirectory and next by names only if in list have folders
      hasFolderInList = true; 
      var arrFolders = [] , arrFiles = [];              
      for (var inx = 0; inx < this.searchResults.length; inx++) {
        if (this.searchResults[inx].isDirectory) {
          arrFolders.push(this.searchResults[inx]); 
        } else {
          arrFiles.push(this.searchResults[inx]);
        }      
      }
      arrFolders = arrFolders.sort(SortByName);
      arrFiles = arrFiles.sort(SortByName);
      this.searchResults = arrFolders.concat(arrFiles); 
    } else {      
      if (this.searchResults.length > 0 && this.searchResults[0].isDirectory) {
        var arrFiles = [];   
        for (var inx = 0; inx < this.searchResults.length; inx++) {
          if (!this.searchResults[inx].isDirectory) {
            arrFiles.push(this.searchResults[inx]);
          }      
        }
        arrFiles = arrFiles.sort(SortByName);
        this.searchResults = arrFiles;         
      } else {
        this.searchResults = this.searchResults.sort(SortByName);
      }      
    }

    this.fileTable.fnAddData(this.searchResults);

    var self = this;

    this.fileTable.$('tr')
      .droppable({
        accept: ".tagButton",
        hoverClass: "activeRow",
        drop: function(event, ui) {

          var tagName = TSCORE.selectedTag;
          var targetFilePath = $(this).find(".fileTitleButton").attr("filepath");

          // preventing self drag of tags
          var targetTags = TSCORE.TagUtils.extractTags(targetFilePath);
          for (var i = 0; i < targetTags.length; i++) {
            if (targetTags[i] === tagName) {
              return true;
            }
          }

          console.log("Tagging file: " + tagName + " to " + targetFilePath);
          $(this).toggleClass("ui-selected");
          TSCORE.PerspectiveManager.clearSelectedFiles();
          TSCORE.selectedFiles.push(targetFilePath);
          TSCORE.TagUtils.addTag(TSCORE.selectedFiles, [tagName]);
          self.handleElementActivation();

          $(ui.helper).remove();
        }
      })
      .click(function() {
        console.log("Selecting file...");
        var titleBut = $(this).find(".fileTitleButton");
        self.selectFile(titleBut, $(titleBut).attr("filepath"));
      });

    if (isCordova) {
      this.fileTable.$('tr').hammer().on("doubletap", function() {
          console.log("Doubletap & Opening file...");
          var titleBut = $(this).find(".fileTitleButton");
          TSCORE.FileOpener.openFile($(titleBut).attr("filepath"));
          self.selectFile(titleBut, $(titleBut).attr("filepath"));
        });
    } else {
      this.fileTable.$('tr')
        .on("dblclick", function() {
          console.log("Doubletap -> Opening file...");
          var titleBut = $(this).find(".fileTitleButton");
          if ($(titleBut).attr("isDirectory")) {
            TSCORE.navigateToDirectory($(titleBut).attr("filepath"));
          } else {
            TSCORE.FileOpener.openFile($(titleBut).attr("filepath"));
          }
          
          self.selectFile(titleBut, $(titleBut).attr("filepath"));
        });
    }

    this.fileTable.$('.fileTitleButton')
      //.on('click', function(e) { e.preventDefault(); return false; })
      .draggable({
        "zIndex": 10000,
        "cancel": false,
        "appendTo": "body",
        "helper": "clone",
        "revert": true,
        "start": function() {
          console.log("Start dragging -----");
          self.selectFile(this, $(this).attr("filepath"));
        }
      });

    this.fileTable.$('.fileSelection')
      .on('click', function(e) {
        e.preventDefault();
        var fpath = $(this).parent().find(".fileTitleButton").attr("filepath");
        var stateTag = $(this).find("i");
        if (stateTag.hasClass("fa-square-o")) {
          stateTag.removeClass("fa-square-o").addClass("fa fa-check-square-o");
          $(this).parent().parent().addClass("ui-selected");
          TSCORE.selectedFiles.push(fpath);
          selectedIsFolderArr[fpath] =  $(this).parent().find(".fileTitleButton").attr("isDirectory");
        } else {
          stateTag.removeClass("fa-check-square-o").addClass("fa-square-o");
          $(this).parent().parent().removeClass("ui-selected");
          TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(fpath), 1);
          selectedIsFolderArr[fpath] =  false;
        }
        self.handleElementActivation();
        return false;
      });

    this.fileTable.$('.tagButton')
      //.on('click', function(e) { e.preventDefault(); return false; })
      .draggable({
        "cancel": false,
        "appendTo": "body",
        "helper": "clone",
        "revert": true,
        "start": function() {
          TSCORE.selectedTag = $(this).attr("tag");
          self.selectFile(this, $(this).attr("filepath"));
        }
      });

    // Enable all buttons
    this.viewToolbar.find(".btn").prop('disabled', false);
    // Disable certain buttons again
    $("#" + this.extensionID + "IncreaseThumbsButton").prop('disabled', true);
    $("#" + this.extensionID + "TagButton").prop('disabled', true);
    $("#" + this.extensionID + "CopyMoveButton").prop('disabled', true);

    //Update statusbar, TODO make an core.ui api call for statusbar updates
    if (this.searchResults.length !== undefined) {
      if (TSCORE.Search.nextQuery.length > 0) {
        $("#statusBar").text(this.searchResults.length + " " + $.i18n.t("ns.perspectiveList:filesFoundFor") + " '" + TSCORE.Search.nextQuery + "'");
      } else {
        $("#statusBar").text(this.searchResults.length + " " + $.i18n.t("ns.perspectiveList:filesFound"));
      }
    }

    Mousetrap.unbind(TSCORE.Config.getSelectAllKeyBinding());
    Mousetrap.bindGlobal(TSCORE.Config.getSelectAllKeyBinding(), function() {
      self.toggleSelectAll();
    });

    this.refreshThumbnails();
    TSCORE.hideLoadingAnimation();
  };

  var buttonCompTmpl = Handlebars.compile('<button filepath="{{filepath}}" class="btn btn-link fileSelection"><i class="fa {{selected}} fa-fw fa-lg"></i></button><br>' +
    '<button filepath="{{filepath}}" isDirectory="{{isDirectory}}" title="{{filepath}}" class="btn btn-link fileTitleButton {{coloredExtClass}}" data-ext="{{fileext}}">' +
    '<span class="fileExt">{{fileext}}&nbsp;&nbsp;<span class="fa fa-ellipsis-v"></span></span></button>');

  // Helper function user by basic and search views
  ExtUI.prototype.buttonizeTitle = function(title, filePath, fileExt, isDirectory, isSelected) {
    if (title.length < 1) {
      title = filePath;
    }

    //TODO minimize platform specific calls
    var tmbPath;
    if (isCordova || isWeb) {
      tmbPath = filePath;
    } else {
      tmbPath = "file:///" + filePath;
    }

    var context = {
      filepath: filePath,
      isDirectory: isDirectory,
      coloredExtClass: TSCORE.Config.getColoredFileExtensionsEnabled()? "fileExtColor" : "",
      tmbpath: tmbPath,
      fileext: fileExt,
      folder: isDirectory ? "fa fa-folder-o" : "",
      selected: isSelected ? "fa-check-square-o" : "fa-square-o"
    };

    return buttonCompTmpl(context);
  };

  ExtUI.prototype.clearSelectedFiles = function() {
    TSCORE.selectedFiles = [];    
    $("#" + this.extensionID + "Container").find(".fileSelection").find("i")
      .removeClass("fa-check-square-o")
      .addClass("fa-square-o");
    $("#" + this.extensionID + "Container").find("tr").removeClass('ui-selected');
  };

  ExtUI.prototype.selectFile = function(uiElement, filePath) {
    TSCORE.PerspectiveManager.clearSelectedFiles(); 
    $(uiElement).parent().parent().toggleClass("ui-selected");
    $(uiElement).parent().parent().find(".fileSelection").find("i")
      .toggleClass("fa-check-square-o")
      .toggleClass("fa-square-o");

    TSCORE.selectedFiles.push(filePath);
    selectedIsFolderArr[filePath] = $(uiElement).parent().parent().attr("isDirectory"); 
    this.handleElementActivation();
  };

  ExtUI.prototype.switchThumbnailSize = function() {
    this.currentTmbSize = this.currentTmbSize + 1;

    if (this.currentTmbSize >= TMB_SIZES.length) {
      this.currentTmbSize = 0;
    }

    $('.thumbImg').css({
      "max-width": TMB_SIZES[this.currentTmbSize],
      "max-height": TMB_SIZES[this.currentTmbSize]
    });
  };

  ExtUI.prototype.enableThumbnails = function() {
    $("#" + this.extensionID + "IncreaseThumbsButton").prop('disabled', false);
    $.each(this.fileTable.$('.thumbImg'), function() {
      $(this).attr('style', "");
      $(this).attr('src', $(this).attr('filepath'));
    });
    $('.thumbImg').css({
      "max-width": TMB_SIZES[this.currentTmbSize],
      "max-height": TMB_SIZES[this.currentTmbSize]
    });
  };

  ExtUI.prototype.disableThumbnails = function() {
    $("#" + this.extensionID + "IncreaseThumbsButton").prop('disabled', true);
    $.each(this.fileTable.$('.thumbImg'), function() {
      $(this).attr('style', "width: 0px; height: 0px; border: 0px");
      $(this).attr('src', "");
    });
  };

  ExtUI.prototype.refreshThumbnails = function() {
    if (this.thumbEnabled) {
      this.enableThumbnails();
    } else {
      this.disableThumbnails();
    }
  };

  ExtUI.prototype.toggleThumbnails = function() {
    this.thumbEnabled = !this.thumbEnabled;
    this.refreshThumbnails();
  };

  ExtUI.prototype.handleElementActivation = function() {
    console.log("Entering element activation handler...");

    var tagButton = $("#" + this.extensionID + "TagButton");
    var copyMoveButton = $("#" + this.extensionID + "CopyMoveButton");
    var deleteSelectedFilesButton = $("#" + this.extensionID + "DeleteSelectedFilesButton");

    var isFolderInSelection = false;
      
    if (hasFolderInList) {
      for (var inx = 0; inx < TSCORE.selectedFiles.length; inx++) {        
        if (selectedIsFolderArr[TSCORE.selectedFiles[inx]]) {
          isFolderInSelection = true;
          break;
        }
      }
    }

    if (TSCORE.selectedFiles.length >= 1 && !isFolderInSelection) {
      tagButton.parent().removeClass("disabled");
      copyMoveButton.parent().removeClass("disabled");
      deleteSelectedFilesButton.parent().removeClass("disabled");
    } else {
      tagButton.parent().addClass("disabled");
      copyMoveButton.parent().addClass("disabled");
      deleteSelectedFilesButton.parent().addClass("disabled");
    }
  };

  ExtUI.prototype.removeFileUI = function(filePath) {
    console.log("Removing from UI" + filePath + " from UI");

    // Updating the file selection
    TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(filePath), 1);

    var row4remove;
    if (isWin && !isWeb) {
      filePath = filePath.replace("\\", "");
      $("#" + this.extensionID + "Container button[filepath]").each(function() {
        if ($(this).attr("filepath").replace("\\", "") === filePath) {
          row4remove = $(this).parent().parent();
        }
      });
    } else {
      row4remove = $("#" + this.extensionID + "Container button[filepath='" + filePath + "']").parent().parent();
    }
    if (row4remove !== undefined) {
      row4remove.remove();
    }
  };

  ExtUI.prototype.updateFileUI = function(oldFilePath, newFilePath) {
    console.log("Updating UI for oldfile " + oldFilePath + " newfile " + newFilePath);

    // Updating the file selection
    if (oldFilePath !== newFilePath) {
      TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(oldFilePath), 1);
      TSCORE.selectedFiles.push(newFilePath);
    }

    var title = TSCORE.TagUtils.extractTitle(newFilePath),
      fileExt = TSCORE.TagUtils.extractFileExtension(newFilePath),
      fileTags = TSCORE.TagUtils.extractTags(newFilePath);

    var $fileRow;

    if (isWin && !isWeb) {
      oldFilePath = oldFilePath.replace("\\", "");
      $("#" + this.extensionID + "Container button[filepath]").each(function() {
        if ($(this).attr("filepath").replace("\\", "") === oldFilePath) {
          $fileRow = $(this).parent().parent();
        }
      });
    } else {
      $fileRow = $("#" + this.extensionID + "Container button[filepath='" + oldFilePath + "']").parent().parent();
    }

    $($fileRow.find("td")[0]).empty().append(this.buttonizeTitle(title, newFilePath, fileExt, false, true));
    $($fileRow.find("td")[1]).text(title);
    $($fileRow.find("td")[2]).empty().append(TSCORE.generateTagButtons(fileTags, newFilePath));

    this.refreshThumbnails();

    var self = this;
    $fileRow.find('.fileTitleButton')
      .draggable({
        "cancel": false,
        "appendTo": "body",
        "helper": "clone",
        "revert": true,
        "start": function() {
          self.selectFile(this, $(this).attr("filepath"));
        }
      });

    $fileRow.find('.fileSelection')
      .click(function(e) {
        e.preventDefault();
        var fpath = $(this).parent().find(".fileTitleButton").attr("filepath");
        var stateTag = $(this).find("i");
        if (stateTag.hasClass("fa-square-o")) {
          stateTag.removeClass("fa-square-o").addClass("fa fa-check-square-o");
          $(this).parent().parent().addClass("ui-selected");
          TSCORE.selectedFiles.push(fpath);
          selectedIsFolderArr[fpath] =  $(this).parent().find(".fileTitleButton").attr("isDirectory");
        } else {
          stateTag.removeClass("fa-check-square-o").addClass("fa-square-o");
          $(this).parent().parent().removeClass("ui-selected");
          TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(fpath), 1);
          selectedIsFolderArr[fpath] = false; 
        }
        self.handleElementActivation();
        return false;
      });

    $fileRow.find('.tagButton')
      .draggable({
        "cancel": false,
        "appendTo": "body",
        "helper": "clone",
        "revert": true,
        "start": function() {
          TSCORE.selectedTag = $(this).attr("tag");
          self.selectFile(this, $(this).attr("filepath"));
        }
      });

  };

  ExtUI.prototype.getNextFile = function(filePath) {
    var nextFilePath;
    var self = this;
    this.searchResults.forEach(function(entry, index) {
      if (entry.path === filePath) {
        var nextIndex = index + 1;
        if (nextIndex < self.searchResults.length) {
          nextFilePath = self.searchResults[nextIndex].path;
        } else {
          nextFilePath = self.searchResults[0].path;
        }
      }
      console.log("Path: " + entry.path);
    });
    TSCORE.PerspectiveManager.clearSelectedFiles();
    console.log("Next file: " + nextFilePath);
    return nextFilePath;
  };

  ExtUI.prototype.getPrevFile = function(filePath) {
    var prevFilePath;
    var self = this;
    this.searchResults.forEach(function(entry, index) {
      if (entry.path === filePath) {
        var prevIndex = index - 1;
        if (prevIndex >= 0) {
          prevFilePath = self.searchResults[prevIndex].path;
        } else {
          prevFilePath = self.searchResults[self.searchResults.length - 1].path;
        }
      }
      console.log("Path: " + entry.path);
    });
    TSCORE.PerspectiveManager.clearSelectedFiles();
    console.log("Prev file: " + prevFilePath);
    return prevFilePath;
  };

  ExtUI.prototype.toggleSelectAll = function() {
    var $checkIcon = $("#" + this.extensionID + "ToogleSelectAll").find("i");
    if ($checkIcon.hasClass("fa-square-o")) {
      TSCORE.selectedFiles = [];
      $('#' + this.extensionID + 'FileTable tbody tr').each(function() {
        if (!$(this).attr("isDirectory")) {
          $(this).addClass('ui-selected');
          $(this).find(".fileSelection").find("i").addClass("fa-check-square-o").removeClass("fa-square-o");
          TSCORE.selectedFiles.push($(this).find(".fileTitleButton").attr("filepath"));
        } else {
          $(this).parent().removeClass("ui-selected");
          $(this).find(".fileSelection").find("i").removeClass("fa-check-square-o").addClass("fa-square-o");
        }
      });
      $checkIcon.removeClass("fa-square-o").addClass("fa-check-square-o");
    } else {
      TSCORE.PerspectiveManager.clearSelectedFiles();
      $checkIcon.removeClass("fa fa-check-square-o").addClass("fa fa-square-o");
    }
    this.handleElementActivation();
  };

  exports.ExtUI = ExtUI;
});
