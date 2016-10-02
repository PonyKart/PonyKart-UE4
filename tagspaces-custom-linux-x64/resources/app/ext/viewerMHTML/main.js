/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* global MailParser, DOMPurify */
 /* globals marked */

"use strict";

function setContent(content, filePathURI) {
  //console.log("MHTML Content: "+content);
  var mhtparser = new MailParser();
  mhtparser.on("end", function(mail_object) {
    //console.log("mail_object:", mail_object);

    var m = /^content-location:(.*$)/im.exec(content); 
    mail_object.contentLocation = (m.length > 0) ?  m[1] : "not found";
    var cleanedHTML = DOMPurify.sanitize(mail_object.html);
    
    $("#mhtmlViewer").html(cleanedHTML);

    // making all links open in the user default browser
    $("#mhtmlViewer").find("a").bind('click', function(e) {
      e.preventDefault();
      var msg = {command: "openLinkExternally", link : $(this).attr("href")};
      window.parent.postMessage(JSON.stringify(msg), "*");
    }).css("cursor", "default");

    $("#fileMeta").append("saved on " + mail_object.headers.date);

    init(filePathURI, mail_object);

  });

  mhtparser.write(content);
  mhtparser.end();
}

function init(filePathURI, objectlocation) {
  var isCordova;
  var isWin;
  var isWeb;
  
  var $htmlContent;  
  
  function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
    results = regex.exec(window.location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  }

  var locale = getParameterByName("locale");

  var extSettings;
  loadExtSettings();

  isCordova = parent.isCordova;
  isWin = parent.isWin;
  isWeb = parent.isWeb;

  $htmlContent = $("#mhtmlViewer");
  
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

  $("#changeStyleButton").on('click', function() {
    currentStyleIndex = currentStyleIndex + 1;
    if (currentStyleIndex >= styles.length) {
      currentStyleIndex = 0;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#resetStyleButton").on('click', function() {
    currentStyleIndex = 0;
    //currentZoomState = 5;
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  //hide zoom operation menu items because they don't influence on the style
  $("#zoomInButton").hide();
  $("#zoomOutButton").hide();
  $("#zoomResetButton").hide();

  $("#zoomInButton").on('click', function() {
    //console.log("#zoomInButton click");
    currentZoomState++;
    if (currentZoomState >= zoomSteps.length) {
      currentZoomState = 6;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#zoomOutButton").on('click', function() {
    //console.log("#zoomOutButton  click");
    currentZoomState--;
    if (currentZoomState < 0) {
      currentZoomState = 0;
    }
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });

  $("#zoomResetButton").on('click', function() {
    currentZoomState = 3;
    $htmlContent.removeClass();
    $htmlContent.addClass('markdown ' + styles[currentStyleIndex] + " " + zoomSteps[currentZoomState]);
    saveExtSettings();
  });


  $("#openInNewWindowButton").click(function() {
    window.parent.open(filePathURI, '_blank');
  });

  $("#openURLButton").click(function() {
    var msg = {command: "openLinkExternally", link : objectlocation.contentLocation.trim()};
    window.parent.postMessage(JSON.stringify(msg), "*");    
  });
  
  // Init internationalization
  $.i18n.init({
    ns: {namespaces: ['ns.viewerMHTML']},
    debug: true,
    lng: locale,
    fallbackLng: 'en_US'
  }, function() {
    $('[data-i18n]').i18n();
  });

  function saveExtSettings() {
    var settings = {
      "styleIndex": currentStyleIndex,
      "zoomState":  currentZoomState
    };
    localStorage.setItem('viewerMHTMLSettings', JSON.stringify(settings));
  }

  function loadExtSettings() {
    extSettings = JSON.parse(localStorage.getItem("viewerMHTMLSettings"));
  }
}