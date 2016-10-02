/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* globals marked */
'use strict';

var isCordova;
var isWin;
var isWeb;

$(document).ready(function() {
  function getParameterByName(name) {
    name = name.replace(/[\[]/ , "\\\[").replace(/[\]]/ , "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)") ,
            results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g , " "));
  }

  var locale = getParameterByName("locale");

  var extSettings;
  loadExtSettings();

  isCordova = parent.isCordova;
  isWin = parent.isWin;
  isWeb = parent.isWeb;
  //
  $(document).on('drop dragend dragenter dragover' , function(event) {
    event.preventDefault();
  });

  var $htmlContent = $("#htmlContent");

  var styles = ['', 'solarized-dark', 'github', 'metro-vibes', 'clearness', 'clearness-dark'];
  var currentStyleIndex = 0;
  if (extSettings && extSettings.styleIndex) {
    currentStyleIndex = extSettings.styleIndex;
  }

  var zoomSteps = ['zoomSmallest', 'zoomSmaller', 'zoomSmall', 'zoomDefault', 'zoomLarge', 'zoomLarger', 'zoomLargest'];
  var currentZoomState = 3;
  if (extSettings && extSettings.zoomState) {
    currentZoomState = extSettings.zoomState;
  }

  $htmlContent.removeClass();
  $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);

  $("#changeStyleButton").bind('click', function() {
    currentStyleIndex = currentStyleIndex + 1;
    if (currentStyleIndex >= styles.length) {
      currentStyleIndex = 0;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#zoomInButton").bind('click', function() {
    currentZoomState++;
    if (currentZoomState >= zoomSteps.length) {
      currentZoomState = 6;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#zoomOutButton").bind('click', function() {
    currentZoomState--;
    if (currentZoomState < 0) {
      currentZoomState = 0;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#zoomResetButton").bind('click', function() {
    currentZoomState = 3;
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#printButton").on("click", function() {
    $(".dropdown-menu").dropdown('toggle');
    window.print();
  });

  if (isCordova) {
    $("#printButton").hide();
  }

  // Init internationalization
  $.i18n.init({
    ns: {namespaces: ['ns.viewerAudioVideo']} ,
    debug: true ,
    lng: locale ,
    fallbackLng: 'en_US'
  } , function() {
    $('[data-i18n]').i18n();
  });

  function saveExtSettings() {
    var settings = {};
    localStorage.setItem('viewerAudioVideoSettings', JSON.stringify(settings));
  }

  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("viewerAudioVideoSettings"));
  }

  document.querySelector('.js-plyr').addEventListener('ended', function(event) {
    handleVideoEnded();
  });

  var filePath;
  function handleVideoEnded() {
    var msg = {command: "playbackEnded" , filepath: filePath};
    window.parent.postMessage(JSON.stringify(msg) , "*");
  }
});

function setContent(content , fileDirectory) {
  var $htmlContent = $('#main');
  $htmlContent.append(content);
  console.log('SHOW MD CONTENT : ' + content);

  $("base").attr("href" , fileDirectory + "//");

  if (fileDirectory.indexOf("file://") === 0) {
    fileDirectory = fileDirectory.substring(("file://").length , fileDirectory.length);
  }

  var hasURLProtocol = function(url) {
    return (
      url.indexOf("http://") === 0 ||
      url.indexOf("https://") === 0 ||
      url.indexOf("file://") === 0 ||
      url.indexOf("data:") === 0
    );
  };

  // fixing embedding of local images
  $htmlContent.find("img[src]").each(function() {
    var currentSrc = $(this).attr("src");
    if (!hasURLProtocol(currentSrc)) {
      var path = (isWeb ? "" : "file://") + fileDirectory + "/" + currentSrc;
      $(this).attr("src" , path);
    }
  });

  $htmlContent.find("a[href]").each(function() {
    var currentSrc = $(this).attr("href");
    var path;

    if (!hasURLProtocol(currentSrc)) {
      path = (isWeb ? "" : "file://") + fileDirectory + "/" + currentSrc;
      $(this).attr("href" , path);
    }

    $(this).bind('click' , function(e) {
      e.preventDefault();
      if (path) {
        currentSrc = encodeURIComponent(path);
      }
      var msg = {command: "openLinkExternally" , link: currentSrc};
      window.parent.postMessage(JSON.stringify(msg) , "*");
    });
  });


}
