/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* global define, Handlebars, isWin, _  */
define(function(require, exports, module) {
  "use strict";

  console.log("Loading UI for perspectiveDefault");

  var TSCORE = require("tscore");

  var MONTH = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

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
    localStorage.setItem('perpectiveGridSettings', JSON.stringify(settings));
  }
  //load settings for perpectiveGrid
  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("perpectiveGridSettings"));
  }

  function ExtUI(extID) {
    this.extensionID = extID;
    this.viewContainer = $("#" + this.extensionID + "Container").empty();

    this.currentGrouping = ""; // tagchain, day, month, year
    this.thumbEnabled = false;
    this.currentTmbSize = 0;
    this.searchResults = undefined;
    this.supportedGroupings = [];

    this.supportedGroupings.push({
      "title": "Day",
      "key": "day"
    });
    this.supportedGroupings.push({
      "title": "Month",
      "key": "month"
    });
    this.supportedGroupings.push({
      "title": "Year",
      "key": "year"
    });

    for (var i = 0; i < TSCORE.Config.Settings.tagGroups.length; i++) {
      // Exclude smart tags and calculated tags 
      if (TSCORE.Config.Settings.tagGroups[i].key !== "SMR" &&
        TSCORE.Config.Settings.tagGroups[i].key !== "CTG") {
        this.supportedGroupings.push({
          "title": TSCORE.Config.Settings.tagGroups[i].title,
          "key": TSCORE.Config.Settings.tagGroups[i].key
        });
      }
    }
  }

  var fileTileTmpl = Handlebars.compile(
    '<div title="{{filepath}}" filepath="{{filepath}}" class="fileTile" style="background-image: url(\'{{thumbPath}}\')">' +
      '<button class="btn btn-link fileTileSelector {{coloredExtClass}}" data-ext="{{fileext}}" filepath="{{filepath}}">' +
        '<i class="fa {{selected}} fa-lg"></i><span class="fileExtTile">{{fileext}}</span></button>' +
      '<div class="tagsInFileTile">' +
      '{{#each tags}}' +
        '<button class="btn btn-sm tagButton fileTagsTile" tag="{{tag}}" filepath="{{filepath}}" style="{{style}}">{{tag}}' +
          '<!-- <span class="fa fa-ellipsis-v"></span--></button>' +
      '{{/each}}' +
      '</div>' +
      '<div class="titleInFileTile">{{title}}</div>' +
    '</div>'
    );

  var folderTileTmpl = Handlebars.compile(
    '<div title="{{folderpath}}" folderpath="{{folderpath}}" class="fileTile">' +
      '<button class="btn btn-link fileTileSelector {{coloredExtClass}}" data-ext="folder" folderpath="{{folderpath}}">' +
        '<i class="fa fa-folder-o fa-lg"></i><!--span class="fileExtTile">{{title}}</span--></button>' +
      '<div class="tagsInFileTile">' +
      '{{#each tags}}' +
        '<button class="btn btn-sm tagButton fileTagsTile" tag="{{tag}}" folderpath="{{folderpath}}" style="{{style}}">{{tag}}</button>' +
      '{{/each}}' +
      '</div>' +      
      '<div class="titleInFileTile">{{title}}</div>' +
    '</div>'
    );
    
  var mainLayoutTemplate = Handlebars.compile(
    '<div class="extMainContent accordion">' +
      '{{#each groups}}' +
      '<div class="accordion-group disableTextSelection" style="width: 100%; border: 0px #aaa solid;">' +
        '{{#if ../moreThanOneGroup}}' +
        '<div class="accordion-heading btn-group" style="width:100%; margin: 0px; border-bottom: solid 1px #eee; background-color: #f0f0f0;">' +
          '<button class="btn btn-link groupTitle" data-toggle="collapse" data-target="#{{../../id}}SortingButtons{{@index}}">' +
          '<i class="fa fa-minus-square">&nbsp;</i>' +
        '</button>' +
        '<span class="btn btn-link groupTitle" id="{{../../id}}HeaderTitle{{@index}}" style="margin-left: 0px; padding-left: 0px;"></span>' +
        '</div>' +
        '{{/if}}' +
        '<div class="accordion-body collapse in" id="{{../id}}SortingButtons{{@index}}" style="margin: 0px 0px 0px 3px; border: 0px;">' +
          '<div class="accordion-inner tileContainer" id="{{../id}}GroupContent{{@index}}"></div>' +
        '</div>' +
      '</div>' +
      '{{else}}' +
      '<p style="margin: 5px; font-size: 13px; text-align: center;">Directory does not contain any files or is currently being analysed.</p>' +
      '{{/each}}' +
      '<div id="gridShowAllFilesContainer">' +
        '<button class="btn btn-primary" id="gridShowAllFilesButton">Show all files</button>' +
      '</div>' +
    '</div>'
  );

  ExtUI.prototype.buildUI = function(toolbarTemplate) {
    console.log("Init UI module");

    var self = this;
    this.viewContainer.append(toolbarTemplate({id: this.extensionID}));

    $("#" + this.extensionID + "ToogleSelectAll").on("click", function() {
      self.toggleSelectAll();
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

    $("#" + this.extensionID + "IncludeSubDirsButton").on("click", function() {
      TSCORE.IOUtils.createDirectoryIndex(TSCORE.currentPath);
    });

    $("#" + this.extensionID + "TagButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) { return false; }
      TSCORE.showAddTagsDialog();
    });

    $("#" + this.extensionID + "CopyMoveButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) { return false; }
      TSCORE.showMoveCopyFilesDialog();
    });

    $("#" + this.extensionID + "AddFileButton").on("click", function() {
      $("#addFileInput").click();
    });

    $("#" + this.extensionID + "DeleteSelectedFilesButton").on("click", function() {
      if ($(this).parent().hasClass("disabled")) {
        return false;
      } else {
        TSCORE.UI.showDeleteFilesDialog();
      }
    });

    $("#" + this.extensionID + "MainDropUp").on('click', function() {
      TSCORE.hideAllDropDownMenus();
    });

    // Init Tag Context Menus
    /*$(".tagButton").contextmenu(function() {
      TSCORE.hideAllDropDownMenus();
      self.selectFile($(this).attr("filepath"));
      TSCORE.openTagMenu(this, $(this).attr("tag"), $(this).attr("filepath"));
      TSCORE.showContextMenu("#tagMenu", $(this));
      return false;
    });*/

    this.initFileGroupingMenu();

    $('#viewContainers')
      .on('scroll', _.debounce(function() { // Triggering thumbnails generation
        $('#viewContainers').find(".fileTile").each(function() {
          self.setThumbnail(this);
        });
      }, 500));
      //.contextmenu(function(e) {
      //  $('.mainDropUpMenu').show();
      //});

  };

  ExtUI.prototype.reInit = function(showAllResult) {
    var self = this;
    var shouldShowAllFilesContainer;

    this.viewContainer.find('.extMainContent').remove();

    var $extMainContent = this.viewContainer.find(".extMainContent");
    if ($extMainContent) {
      $extMainContent.remove();
    }

    if (showAllResult && this.partialResult && this.partialResult.length > 0) {
      this.searchResults = this.allResults;
      this.partialResult = [];
      shouldShowAllFilesContainer = false;
    } else {      
      this.allResults = TSCORE.Search.searchData(TSCORE.fileList, TSCORE.Search.nextQuery);
      if (this.allResults.length >= TSCORE.Config.getMaxSearchResultCount()) {
        this.partialResult = this.allResults.slice(0, TSCORE.Config.getMaxSearchResultCount());        
        this.searchResults = this.partialResult;        
        shouldShowAllFilesContainer = true;
      } else {        
        this.searchResults = this.allResults;        
        shouldShowAllFilesContainer = false;
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
      this.searchResults = this.searchResults.sort(SortByName);
    }
    
    
    var fileGroups = self.calculateGrouping(this.searchResults);

    var moreThanOneGroup = (fileGroups.length > 1) ? true : false;

    this.viewContainer.append(mainLayoutTemplate({
      id: self.extensionID,
      groups: fileGroups,
      moreThanOneGroup: moreThanOneGroup
    }));

    shouldShowAllFilesContainer ? $("#gridShowAllFilesContainer").show() : $("#gridShowAllFilesContainer").hide();

    $('#gridShowAllFilesButton').on("click", function() {
      self.reInit(true);
    });

    this.viewContainer.on("contextmenu", ".fileTile", function(e) {
      var selEl = $(this).parent().find(".fileTitle button");
      e.preventDefault();
      TSCORE.hideAllDropDownMenus();
      TSCORE.PerspectiveManager.clearSelectedFiles();
      self.selectFile($(this).attr("filepath"));
      TSCORE.showContextMenu("#fileMenu", $(this));
      return false;
    }); 

    $extMainContent = this.viewContainer.find(".extMainContent");

    var $groupeContent;
    var $groupeTitle;

    _.each(fileGroups, function(value, index) {
      $groupeContent = $("#" + self.extensionID + "GroupContent" + index);
      $groupeTitle = $("#" + self.extensionID + "HeaderTitle" + index);

      var groupingTitle = self.calculateGroupTitle(value[0]);
      $groupeTitle.text(groupingTitle);

      // Sort the files in group by name
      /*
      value = _.sortBy(value, function(entry) {
        return entry.name;
      });
      */
         
      // Iterating over the files in group
      for (var j = 0; j < value.length; j++) {
        //console.warn("value: " +value[j].isDirectory + " -- " + value[j].name);        
        if (value[j].isDirectory) {
          if (showFoldersInList) {
            hasFolderInList = true;
            $groupeContent.append(self.createFolderTile(
              value[j].name,
              value[j].path,
              false            
            ));          
          }
        } else {
          $groupeContent.append(self.createFileTile(
            value[j].title,
            value[j].path,
            value[j].extension,
            value[j].tags,
            false,
            value[j].meta
          ));
        }
      }
    });

    //console.warn("--showFoldersInList--" + showFoldersInList);
    

    // Adding event listeners
    $extMainContent.find(".fileTile").each(function() {
      self.assingFileTileHandlers($(this));
    });

    $extMainContent.find(".groupTitle").click(function() {
      $(this).find('i').toggleClass("fa-minus-square").toggleClass("fa-plus-square");
    });

    // Enable all buttons
    $(this.extensionID + "IncludeSubDirsButton").prop('disabled', false);

    this.viewContainer.find(".extMainMenu .btn").prop('disabled', false);
    // Disable certain buttons again
    $("#" + this.extensionID + "IncreaseThumbsButton").prop('disabled', true);
    $("#" + this.extensionID + "TagButton").prop('disabled', true);

    if (this.searchResults.length !== undefined) {
      if (TSCORE.Search.nextQuery.length > 0) {
        $("#statusBar").text(this.searchResults.length + " files found for '" + TSCORE.Search.nextQuery + "'");
      } else {
        $("#statusBar").text(this.searchResults.length + " files found");
      }
    }

    TSCORE.hideLoadingAnimation();
    $('#viewContainers').trigger('scroll');
  };

  ExtUI.prototype.createFileTile = function(title, filePath, fileExt, fileTags, isSelected, metaObj) {
    var fileParentDir = TSCORE.TagUtils.extractParentDirectoryPath(filePath);
    var fileName = TSCORE.TagUtils.extractFileName(filePath);

    var tmbPath = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
    var metaObj = metaObj || {thumbnailPath : ""};

    if (metaObj.thumbnailPath && metaObj.thumbnailPath.length > 2) {
      tmbPath = encodeURI(metaObj.thumbnailPath);
    }

    var context = {
      filepath: filePath,
      fileext: fileExt,
      title: title,
      coloredExtClass: TSCORE.Config.getColoredFileExtensionsEnabled()? "fileExtColor" : "",
      tags: [],
      selected: isSelected ? "fa-check-square" : "fa-square-o",
      thumbPath: tmbPath
    };
    
    if (fileTags.length > 0) {
      var tagString = "" + fileTags;
      var tags = tagString.split(",");

      for (var i = 0; i < tags.length; i++) {
        context.tags.push({
          tag: tags[i],
          filepath: filePath,
          style: TSCORE.generateTagStyle(TSCORE.Config.findTag(tags[i]))
        });
      }
    }

    if (metaObj.metaData && metaObj.metaData.tags) {
      metaObj.metaData.tags.forEach(function(elem) {
        context.tags.push({
          tag: elem.title,
          filepath: filePath,
          style: elem.style
        });
      });
    }

    return fileTileTmpl(context);
  };

  ExtUI.prototype.createFolderTile = function(name, filePath, isSelected) {
    
    var context = {
      folderpath: filePath,
      title: name,
      tags: [],
      selected: isSelected ? "fa-check-square" : "fa-square-o"
    };    
    
    return folderTileTmpl(context);
  };


  ExtUI.prototype.initFileGroupingMenu = function() {
    var self = this;

    var suggMenu = $("#" + self.extensionID + "GroupingMenu");

    suggMenu.append($('<li>').append($('<a>', {
        title: "Ungroup all elementes",
        text: " Ungroup"
      })
      .prepend("<i class='fa fa-times-circle'></i>")
      .click(function() {
        $("#" + self.extensionID + "GroupingButton")
          .text(" Group ")
          .prepend("<i class='fa fa-group' />")
          .append("<span class='caret'></span>");
        self.switchGrouping("");
      })
    ));
    suggMenu.append('<li class="divider"></li>');

    // Adding context menu entries according to the taggroups
    for (var i = 0; i < self.supportedGroupings.length; i++) {
      suggMenu.append($('<li>').append($('<a>', {
          text: " Group by " + self.supportedGroupings[i].title,
          key: self.supportedGroupings[i].key,
          group: self.supportedGroupings[i].title
        })
        .prepend("<i class='fa fa-group fa-fw' />")
        .click(function() {
          $("#" + self.extensionID + "GroupingButton")
            .attr("title", " Grouped by " + $(this).attr("group") + " ")
            .text(" " + $(this).attr("group") + " ")
            .prepend("<i class='fa fa-group fa-fw' />")
            .append("<span class='caret'></span>");
          self.switchGrouping($(this).attr("key"));
        }) // jshint ignore:line
      ));
    }
  };

  ExtUI.prototype.switchGrouping = function(grouping) {
    this.currentGrouping = grouping;
    //TSCORE.startTime = new Date().getTime(); 
    this.reInit();
  };

  ExtUI.prototype.calculateGroupTitle = function(rawSource) {
    var groupingTitle = "No Grouping";
    var self = this;
    var tmpDate;
    switch (this.currentGrouping) {
      case "day":
        tmpDate = new Date(rawSource.lmdt);
        tmpDate.setHours(0, 0, 0, 0);
        groupingTitle = TSCORE.TagUtils.formatDateTime(tmpDate, false);
        break;
      case "month":
        tmpDate = new Date(rawSource.lmdt);
        tmpDate.setHours(0, 0, 0, 0);
        tmpDate.setDate(1);
        groupingTitle = MONTH[tmpDate.getMonth()] + ", " + tmpDate.getFullYear();
        break;
      case "year":
        tmpDate = new Date(rawSource.lmdt);
        tmpDate.setHours(0, 0, 0, 0);
        tmpDate.setDate(1);
        tmpDate.setMonth(1);
        groupingTitle = tmpDate.getFullYear();
        break;
      default:
        for (var i = 0; i < TSCORE.Config.Settings.tagGroups.length; i++) {
          if (TSCORE.Config.Settings.tagGroups[i].key === self.currentGrouping) {
            var tagsInGroup = _.pluck(TSCORE.Config.Settings.tagGroups[i].children, "title");
            var matchedTags = _.intersection(
              rawSource.tags,
              tagsInGroup
            );
            groupingTitle = "not grouped";
            if (matchedTags.length > 0) {
              groupingTitle = TSCORE.Config.Settings.tagGroups[i].title + " - " + matchedTags[0];
            }
            break;
          }
        }
    }
    return groupingTitle;
  };

  // Helper function for organizing the files in data buckets
  ExtUI.prototype.calculateGrouping = function(data) {
    var self = this;
    switch (this.currentGrouping) {
      case "day":
        data = _.groupBy(data, function(value) {
          var tmpDate = new Date(value.lmdt);
          tmpDate.setHours(0, 0, 0, 0);
          return tmpDate.getTime();
        });
        break;
      case "month":
        data = _.groupBy(data, function(value) {
          var tmpDate = new Date(value.lmdt);
          tmpDate.setHours(0, 0, 0, 0);
          tmpDate.setDate(1);
          return tmpDate.getTime();
        });
        break;
      case "year":
        data = _.groupBy(data, function(value) {
          var tmpDate = new Date(value.lmdt);
          tmpDate.setHours(0, 0, 0, 0);
          tmpDate.setDate(1);
          tmpDate.setMonth(1);
          return tmpDate.getTime();
        });
        break;
      default:
        var grouped = false;
        this.supportedGroupings.forEach(function(grouping) {
          if (grouping.key === self.currentGrouping) {
            data = _.groupBy(data, function(value) {
              var tagGroup = TSCORE.Config.getTagGroupData(grouping.key);
              for (var i = 0; i < tagGroup.children.length; i++) {
                for (var j = 0; j < value.tags.length; j++) {
                  if (tagGroup.children[i].title === value.tags[j]) {
                    return tagGroup.children[i].title;
                  }
                }
              }
            });
            grouped = true;
          }
        });
        if (!grouped) {
          data = _.groupBy(data, function() {
            return true;
          });
        }
        break;
    }

    // Sort groups by date
    data = _.sortBy(data, function(value) {
      var tmpDate = new Date(value[0].lmdt);
      return -tmpDate.getTime();
    });

    return data;
  };

  ExtUI.prototype.setThumbnail = function(uiElement) {
    if (TSCORE.Utils.isVisibleOnScreen(uiElement) && (uiElement.style.backgroundImage.indexOf("image/gif") > 0)) {
      var filePath = uiElement.getAttribute('filepath');
      TSCORE.Meta.loadThumbnailPromise(filePath).then(function(url) {
        uiElement.style.backgroundImage = "url('" + url + "')";
      });
    }
  };

  ExtUI.prototype.assingFileTileHandlers = function($fileTile) {
    var filePath = $fileTile.attr("filepath");
    var folderPath = $fileTile.attr("folderpath");
    var self = this;

    $fileTile
      .hammer().on("doubletap", function() { //.dblclick(function() {
        return false;
        //TSCORE.FileOpener.openFile(filePath);
        //self.selectFile(filePath);
      })
      .click(function() {
        if (folderPath) {
          TSCORE.navigateToDirectory(folderPath);
        } else {
          TSCORE.FileOpener.openFile(filePath);
          self.selectFile(filePath);
        }
      })
      .droppable({
        accept: ".tagButton",
        hoverClass: "activeRow",
        drop: function(event, ui) {
          var tagName = TSCORE.selectedTag; //ui.draggable.attr("tag");                                   
          var targetFilePath = (folderPath ? folderPath : filePath); // $(this).attr("filepath");;

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
      });

    $fileTile.find(".fileTileSelector")
      .click(function(e) {
        e.preventDefault();
        var folderPath = $(this).attr("folderpath");
        if(folderPath) {
          TSCORE.navigateToDirectory(folderPath);
          return false;
        } else {
          var $stateTag = $(this).find("i");
          if ($stateTag.hasClass("fa-square-o")) {
            $stateTag.removeClass("fa-square-o").addClass("fa fa-check-square");
            $(this).parent().addClass("ui-selected");
            TSCORE.selectedFiles.push(filePath);
            selectedIsFolderArr[filePath] =  (typeof($(this).attr("folderpath")) != "undefined");
          } else {
            $stateTag.removeClass("fa-check-square").addClass("fa-square-o");
            $(this).parent().removeClass("ui-selected");
            TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(filePath), 1);
            selectedIsFolderArr[filePath] =  false;
          }
          self.handleElementActivation();
          return false;
        }
      })
      .draggable({
        "cancel": false,
        "zIndex": 10000,
        "appendTo": "body",
        "helper": "clone",
        "opacity": "0.5",
        "revert": true,
        "start": function() {
          if($(this).attr("folderpath")) {
            return false;
          } else {
            self.selectFile(filePath);
          }
        }
      });


    $fileTile.find(".fileTagsTile")
      /*.click(function(e) {
        //e.preventDefault();
        self.selectFile(filePath);
        TSCORE.openTagMenu(this, $(this).attr("tag"), filePath);
      })*/
      .draggable({
        "cancel": false,
        "appendTo": "body",
        "helper": "clone",
        "revert": true,
        "start": function() {
          TSCORE.selectedTag = $(this).attr("tag");
          self.selectFile(filePath);
        }
      });

    Mousetrap.unbind(TSCORE.Config.getSelectAllKeyBinding());
    Mousetrap.bindGlobal(TSCORE.Config.getSelectAllKeyBinding(), function() {
      self.toggleSelectAll();
    });
  };

  ExtUI.prototype.clearSelectedFiles = function() {
    TSCORE.selectedFiles = [];
    $("#" + this.extensionID + "Container").find(".ui-selected")
      .removeClass("ui-selected");
    $("#" + this.extensionID + "Container").find(".fileTileSelector").find("i")
      .removeClass("fa-check-square")
      .addClass("fa-square-o");
  };

  ExtUI.prototype.selectFile = function(filePath) {
    selectedIsFolderArr = [];
    TSCORE.PerspectiveManager.clearSelectedFiles();
    $(this.viewContainer).find('.fileTileSelector').each(function() {
      if ($(this).attr("filepath") === filePath) {
        $(this).parent().toggleClass("ui-selected");
        $(this).find("i").toggleClass("fa-check-square").toggleClass("fa-square-o");
        TSCORE.selectedFiles.push($(this).attr("filepath"));
        selectedIsFolderArr[$(this).attr("filepath")] =  (typeof($(this).attr("folderpath")) != "undefined");         
      }
    });

    TSCORE.selectedFiles.push(filePath);
    this.handleElementActivation();
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
    console.log("Removing " + filePath + " from UI");

    // Updating the file selection
    TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(filePath), 1);

    if (isWin && !isWeb) {
      filePath = filePath.replace("\\", "");
      $("#" + this.extensionID + "Container div[filepath]").each(function() {
        if ($(this).attr("filepath").replace("\\", "") === filePath) {
          $(this).remove();
        }
      });
    } else {
      $("#" + this.extensionID + "Container div[filepath='" + filePath + "']").remove();
    }
  };

  ExtUI.prototype.updateFileUI = function(oldFilePath, newFilePath) {
    console.log("Updating file in UI");

    // Updating the file selection
    if (oldFilePath !== newFilePath) {
      TSCORE.selectedFiles.splice(TSCORE.selectedFiles.indexOf(oldFilePath), 1);
      TSCORE.selectedFiles.push(newFilePath);
    }
    
    var title = TSCORE.TagUtils.extractTitle(newFilePath);
    var fileExt = TSCORE.TagUtils.extractFileExtension(newFilePath);
    var fileTags = TSCORE.TagUtils.extractTags(newFilePath);
    var parentFolderNewFile = TSCORE.TagUtils.extractParentDirectoryPath(newFilePath);
    var newFileName = TSCORE.TagUtils.extractFileName(newFilePath);

    var $fileTile;
    var attrFilePath;

    if (isWin && !isWeb) {
      oldFilePath = oldFilePath.replace("\\", "");
      $("#" + this.extensionID + "Container div[filepath]").each(function() {
        attrFilePath = $(this).attr("filepath");
        if (attrFilePath.replace("\\", "") === oldFilePath) {
          $fileTile = $(this);
        }
      });
    } else {
      $fileTile = $("#" + this.extensionID + "Container div[filepath='" + oldFilePath + "']");
    }

    var metaObj = TSCORE.Meta.findMetaObjectFromFileList(oldFilePath);
    if (!metaObj) {
      metaObj = {};
      metaObj.thumbnailPath = parentFolderNewFile + TSCORE.dirSeparator + TSCORE.metaFolder + TSCORE.dirSeparator + newFileName + TSCORE.thumbFileExt;
    }
    $fileTile.replaceWith(this.createFileTile(title, newFilePath, fileExt, fileTags, true, metaObj)); 

    if (isWin && !isWeb) {
      newFilePath = newFilePath.replace("\\", "");
      $("#" + this.extensionID + "Container div[filepath]").each(function() {
        attrFilePath = $(this).attr("filepath");
        if (attrFilePath.replace("\\", "") === newFilePath) {
          $fileTile = $(this);
        }
      });
    } else {
      $fileTile = $("#" + this.extensionID + "Container div[filepath='" + newFilePath + "']");
    }

    TSCORE.Meta.loadThumbnailPromise(newFilePath).then(function(url) {
      $fileTile.css("background-image", "url('" + url + "')");
    });

    this.assingFileTileHandlers($fileTile);
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
      //console.log("Path: "+entry.path);
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
      //console.log("Path: "+entry.path);
    });
    TSCORE.PerspectiveManager.clearSelectedFiles();
    console.log("Prev file: " + prevFilePath);
    return prevFilePath;
  };

  ExtUI.prototype.toggleSelectAll = function() {
    var checkIcon = $("#" + this.extensionID + "ToogleSelectAll").find("i");
    if (checkIcon.hasClass("fa-square-o")) {
      TSCORE.selectedFiles = [];            
      $(this.viewContainer).find('.fileTileSelector').each(function() {
        if (typeof($(this).attr("folderpath")) == "undefined") {
          $(this).parent().addClass("ui-selected");
          $(this).find("i").addClass("fa-check-square").removeClass("fa-square-o");
          TSCORE.selectedFiles.push($(this).attr("filepath"));
        } else {
          $(this).parent().removeClass("ui-selected");
          $(this).find("i").removeClass("fa-check-square").addClass("fa-square-o");
        }
      });
    } else {
      TSCORE.PerspectiveManager.clearSelectedFiles();
    }
    this.handleElementActivation();
    checkIcon.toggleClass("fa-check-square");
    checkIcon.toggleClass("fa-square-o");
  };

  exports.ExtUI = ExtUI;
});
