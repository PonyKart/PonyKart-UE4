/* Copyright (c) 2013-2016 The TagSpaces Authors. All rights reserved.
 * Use of this source code is governed by a AGPL3 license that 
 * can be found in the LICENSE file. */

define(function(require, exports, module) {
"use strict";
	
	var extensionTitle = "FolderViz";
	var extensionID = "perspectiveGraph";  // ID should be equal to the directory name where the ext. is located
	var extensionIcon = "fa fa-sitemap";

  console.log("Loading " + extensionID);

	var TSCORE = require("tscore");
	var $viewContainer;
	var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
	var graphMode = "mindmap";
	var treeData;
	
	function init() {
		console.log("Initializing View "+extensionID);
		
    $viewContainer = $("#"+extensionID+"Container").empty();

    require([
      "text!" + extensionDirectory + '/toolbar.html',
			"marked",
      "css!" + extensionDirectory + '/extension.css',
    ], function(toolbarTPL, marked) {
      var toolbarTemplate = Handlebars.compile(toolbarTPL);
      $viewContainer.append(toolbarTemplate({ id: extensionID }));

      initUI();

      try {
        $('#' + extensionID + 'Container [data-i18n]').i18n();
        $('#aboutExtensionModalGraph').on('show.bs.modal', function() {
          $.ajax({
            url: extensionDirectory + '/README.md',
            type: 'GET'
          })
          .done(function(mdData) {
            //console.log("DATA: " + mdData);
            if (marked) {
              var modalBody = $("#aboutExtensionModalGraph .modal-body");
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
        console.log("Failed translating extension");
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
  
	function load() {
		console.log("Loading View "+extensionID);
    TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
	}
	
	function reDraw() {
	  var $viewContainers = $("#viewContainers");
	  var height = $viewContainers.height() - 5;
	  var width = $viewContainers.width() - 5;
    switch (graphMode) {
      case "treeMap":
        require([
            extensionDirectory+'/treeViz.js',
            'css!'+extensionDirectory+'/styles.css',
            ], function(viz) {
                d3.select("svg").remove();
                var svg = d3.select("#"+extensionID+"Container")
                    .append("svg")
                    .attr("width", width)
                    .attr("height", height);
                viz.drawTreeMap(svg, treeData);
                TSCORE.hideLoadingAnimation();
        });
        break;
      case "treeMap2":
        require([
            extensionDirectory+'/treeMap.js',
            'css!'+extensionDirectory+'/styles.css',
            ], function(viz) {
                d3.select("svg").remove();
                var svg = d3.select("#"+extensionID+"Container")
                    .append("svg")
                    .attr("width", width)
                    .attr("height", height);
                viz.drawZoomableTreeMap(svg, treeData);
                TSCORE.hideLoadingAnimation();
        });
        break;
      case "tree":
        require([
            extensionDirectory+'/treeViz.js',
            'css!'+extensionDirectory+'/styles.css',
            ], function(viz) {
                d3.select("svg").remove();
                var svg = d3.select("#"+extensionID+"Container")
                    .append("svg")
                    .attr("width", width)
                    .attr("height", height);
                viz.drawTree(svg, treeData);
                TSCORE.hideLoadingAnimation();
        });
        break;
      case "mindmap":
        require([
          extensionDirectory+'/mindmap.js',
          'css!'+extensionDirectory+'/mindmap.css',
          ], function(viz) {
            d3.select("svg").remove();
            var svg = d3.select("#"+extensionID+"Container")
              .append("svg")
              .attr("id", "tagspacesMindmap")
              .attr("width", width)
              .attr("height", height);
            viz.drawMindMap(svg, treeData);
            TSCORE.hideLoadingAnimation();
        });
        break;
      case "bilevelPartition":
        require([
          extensionDirectory+'/bilevelPartition.js',
          'css!'+extensionDirectory+'/styles.css',
          ], function(viz) {
            d3.select("svg").remove();
            var svg = d3.select("#"+extensionID+"Container")
                .append("svg")
                .attr("width", width)
                .attr("height", height);
            viz.drawPartition(svg, treeData);
            TSCORE.hideLoadingAnimation();
        });
        break;
      default:
        break;
    }
	}
	
	function updateTreeData(fsTreeData) {
		console.log("Updating tree data, Rendering graph...");
		
		treeData = fsTreeData;
		
		reDraw();
   
		TSCORE.hideLoadingAnimation(); 
	}
	
	function clearSelectedFiles() {

		console.log("clearSelectedFiles not implemented in "+extensionID);
	}
	
  function removeFileUI(filePath) {

    console.log("removeFileUI not implemented in "+extensionID);
  }

  function updateFileUI(oldFilePath, newFilePath) {

    console.log("updateFileUI not implemented in "+extensionID);
  }
    
	function getNextFile(filePath) {

		console.log("getNextFile not implemented in "+extensionID);
	}

	function getPrevFile(filePath) {

		console.log("getPrevFile not implemented in "+extensionID);
	}
	
	function initUI() {

    $("#" + extensionID + "ActivateMindmapButton").click(function() {
      graphMode = "mindmap";
      TSCORE.showLoadingAnimation();
      TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
    });

    $("#" + extensionID + "ActivateTreeMapButton").click(function() {
      graphMode = "treeMap";
      TSCORE.showLoadingAnimation();
      TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
    });

    $("#" + extensionID + "ActivateTreeMapNaviButton").click(function() {
      graphMode = "treeMap2";
      TSCORE.showLoadingAnimation();
      TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
    });

    $("#" + extensionID + "ActivateTreeButton").click(function() {
      graphMode = "tree";
      TSCORE.showLoadingAnimation();
      TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
    });

    $("#" + extensionID + "ActivateBilevelPartitionButton").click(function() {
      graphMode = "bilevelPartition";
      TSCORE.showLoadingAnimation();
      TSCORE.IO.createDirectoryTree(TSCORE.currentPath);
    });
	}
	
  // API Vars
  exports.Title = extensionTitle;
  exports.ID = extensionID;
  exports.Icon = extensionIcon;

  // API Methods
  exports.init = init;
  exports.load = load;
	exports.clearSelectedFiles = clearSelectedFiles;
	exports.getNextFile	= getNextFile;
	exports.getPrevFile = getPrevFile;
  exports.removeFileUI = removeFileUI;
  exports.updateFileUI = updateFileUI;
  exports.updateTreeData = updateTreeData;

});