/*
 * @overview EPUB viewer for tagspaces
 * @copyright Copyright (c) 2015 Borislav Sapundzhiev <BSapundzhiev@gmail.com>
 * @license   Licensed under MIT license
 *            See https://opensource.org/licenses/MIT
 */

define(function(require, exports, module) {
  "use strict";

  var extensionID = "viewerEPUB"; // ID should be equal to the directory name where the ext. is located
  var extensionSupportedFileTypes = ["epub"];

  console.log("Loading " + extensionID);

  var TSCORE = require("tscore");
  var extensionDirectory = TSCORE.Config.getExtensionPath() + "/" + extensionID;
  var reader = require("ext/viewerEPUB/epubreader");
  require([
    'css!' + extensionDirectory + '/extension.css',
  ], function() {});

  function initViewerUI(elementID, renderID) {
    var $prev = $("<div class='viewerEPUBNaviButton'>‹</div>").click(reader.prevPage);

    var $next = $("<div class='viewerEPUBNaviButton'>›</div>").click(reader.nextPage);

    var $area = $("<div>")
      .attr('id', renderID)
      .addClass("flexMaxWidth")
      .addClass("flexLayoutVertical")
      .css({"margin": "5% auto"});

    var $main = $("<div>")
      .attr('id', 'viewerEPUBMain')
      .addClass("flexLayout")
      .css({"width": "100%"})
      .append($prev)
      .append($area)
      .append($next);

    $('#' + elementID).append($main);
  }

  function init(filePath, elementID) {
    console.log("Initalization EPUB Viewer...");

    var renderID = getRandomID("epub");
    initViewerUI(elementID, renderID);
    if (!isCordova) {
      filePath = "file://" + filePath;
    }
    reader.loadBook(filePath, renderID);
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

  function getRandomID(prefix, length) {
    var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz";
    var string_length = length || 8;
    var randomstring = '';
    for (var i = 0; i < string_length; i++) {
      var rnum = Math.floor(Math.random() * chars.length);
      randomstring += chars.substring(rnum, rnum + 1);
    }
    return prefix ? prefix + "-" + randomstring : randomstring;
  }

  exports.init = init;
  exports.getContent = getContent;
  exports.setContent = setContent;
  exports.viewerMode = viewerMode;
});
