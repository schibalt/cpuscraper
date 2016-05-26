(function(global){var key,optimost=global.optimost||{},dmh=global.dmh||(global.dmh={}),runtime=dmh.runtime||(dmh.runtime=optimost),config=optimost.config||{},library={opCreativeSetCookieA:function(n,v,d,e){var de=new Date;de.setTime(de.getTime()+e*1000);document.cookie=n+"="+escape(v)+((e==null)?"":("; expires="+de.toGMTString()))+"; path=/"+((d==null)?"":(";domain="+d));},opCreativeGetDocumentSLD:(runtime.SLD?function(){return runtime.SLD.apply(runtime,arguments);}:function(sldIn){var sld=sldIn||document.domain,dp=sld.split("."),l=dp.length,suffix,numLevels=2,i=0,threeLevelDomains=config.threeLevelDomains||["co.uk","gov.uk","com.au","com.cn"];if(l<2){return null;}
if(!isNaN(dp[l-1])&&!isNaN(dp[l-2])){return null;}
suffix=(dp[l-2]+"."+dp[l-1]).toLowerCase();for(i=0;i<threeLevelDomains.length;++i){if(suffix===threeLevelDomains[i]){numLevels=3;break;}}
if(l<numLevels){return null;}
sld="";for(i=0;i<numLevels;++i){sld+="."+dp[(l-numLevels)+i];}
return sld;})};for(key in library){global[key]=library[key];}
return global;})(this);
opCreativeSetCookieA("op1799openboxgum", "a00500d0092c5ok04316n107c", opCreativeGetDocumentSLD(), 604800);
if( "2c5ok04316n1" ){ opCreativeSetCookieA("op1799openboxliid", "a00500d0092c5ok04316n107c", opCreativeGetDocumentSLD(), 86400);}


window.optimost = window.optimost || {};
(function (objName, args) {
args = args || {};

window.optg = window.optg || {};
optg.functions = optg.functions || {};
optg.variables = optg.variables || {};
optg.variables[objName] = {};

var ver = "1.1.0",
date = "2016/03/22",
author = "Alex Wilson";

if (typeof args.liid === 'string' && typeof args.clientId === 'undefined') {
args.clientId = args.liid.match(/^op\d*/);
if (args.clientId.length === 1) {
args.clientId = args.clientId[0].replace(/op/, '');
args.clientId = args.clientId.substr(0, 4);
}
}

if (typeof String.prototype.trim !== 'function') {
String.prototype.trim = function () {
return this.replace(/^\s+|\s+$/g, '');
}
}
if (!window.location.origin) {
window.location.origin = window.location.protocol + "//" + window.location.host;
}

optg.variables[objName] = {
debug: true,
creative: 'cr #9 - wv #' + objName + ' - "All Visitors" persona - ',
QApersona: ("All Visitors".toLowerCase().indexOf("qa") != -1),
secure: true,
consoleType: (typeof args.consoleType === 'string' ? args.consoleType : 'new'),
clientId: (typeof args.clientId === 'string' ? args.clientId : ''),
liid: (typeof args.liid === 'string' ? args.liid : ''),
sectionToHide: (typeof args.sectionToHide === 'string' ? args.sectionToHide : ".opDefaultCSSSectionHolder"),
opImgCounterArray: [],
rollCall: {}
};

if (optg.variables[objName].consoleType === 'new') {
window.jQuery = window.jQuery || optimost.jQuery;
}

optg.functions[objName] = {
/**
* Registers an element to have the click event fire a counter to track clicks for an element that opens in a new window..
* @param {HTMLElement} targetElement - The element to be registered.
* @param {Number} counterNumber - The counter to be called.
* @param {Object|Array|String} [objAttributes] - An object of name-value pairs to be passed to the counter.
*/
addClickTrackingToElement: function (targetElement, counterNumber, objAttributes) {
if (typeof targetElement !== 'object') {
optg.functions[objName].log('Tracking cannot be set without a targeted HTML element.', 'err');
return false;
}
if (typeof counterNumber !== 'string' && typeof counterNumber !== 'number') {
optg.functions[objName].log('Tracking cannot be set without a counter number.', 'err');
return false;
}

if (targetElement.attributes["data-opt" + objName]) {
optg.functions[objName].log('Tracking already set for the "' + targetElement.tagName + '" element with id "' + targetElement.id + '".', 'warn');
return false;
}

objAttributes = optg.functions[objName].convertLegacyParameters(objAttributes);

optg.functions[objName].addEvent(targetElement, "click", function (counterNumber, objAttributes) {
return function () {
optg.functions[objName].executeCounterCall(counterNumber, objAttributes);
};
}(counterNumber, objAttributes));

targetElement.setAttribute("data-opt" + objName, "true:" + counterNumber + ":" + optg.functions[objName].generateAttributesArray(objAttributes, '|').join(''));
optg.functions[objName].log('New HTML Element Click Tracking set for the "' + targetElement.tagName + '" element with id "' + targetElement.id + '".', 'default');
return true;
},
/**
* Adds a level 2 event to the DOM for a given element.
* @param {HTMLElement} element - The element to give this new event to.
* @param {String} eventName - The event to define.
* Example: click
* @param {Function} functionToFire - The function to execute when the event fires.
* @param {Boolean} bubble - Determines if the event should bubble up to the next element. [default: false]
* @returns {boolean} - True if successful, otherwise false.
*/
addEvent: function (element, eventName, functionToFire, bubble) {
if (!element || typeof(functionToFire) != "function") {
return false;
}
bubble = bubble || false;
var evtType = (window.attachEvent) ? "attach" : (window.addEventListener) ? "add" : "none";
if (evtType == "attach") {
element.attachEvent("on" + eventName, functionToFire);
}
else if (evtType == "add") {
element.addEventListener(eventName, functionToFire, bubble);
}
},
/**
* Makes a call the Optimost server to record a counter event by creating an image with the given URL.
* @param {String} counterUrl - The counter URL.
*/
addImageCounter: function (counterUrl) {
optg.variables[objName].opImgCounterArray[optg.variables[objName].opImgCounterArray.length] = new Image();
optg.variables[objName].opImgCounterArray[optg.variables[objName].opImgCounterArray.length - 1].src = counterUrl;
},
/**
* Determines the the class of the object.
* Examples: function, array
* @param obj - The object being analyzed.
* @returns {string} - The class of the object.
*/
checkClass: function (obj) {
return Object.prototype.toString.call(obj).slice(8, -1).toLowerCase();
},
/**
* Converts the legacy attributes string or array into an object to be used in the new code.
* @params {Object|String|Array} objAttributes - The attribute parameter.
*/
convertLegacyParameters: function (objAttributes) {
var arrNames, i, tempObjAttributes;
objAttributes = objAttributes || {};

if (optg.functions[objName].checkClass(objAttributes) === 'string') {
objAttributes = {
'opType': objAttributes
};
} else if (optg.functions[objName].checkClass(objAttributes) === 'array') {
tempObjAttributes = {};
arrNames = ['opType', 'click'];
for (i = 0; i < arrNames.length && i < objAttributes.length; i++) {
tempObjAttributes[arrNames[i]] = objAttributes[i];
}
objAttributes = tempObjAttributes;
} else if (optg.functions[objName].checkClass(objAttributes) !== 'object') {
optg.functions[objName].log('[Setup] convertLegacyParameters - Counter attributes (objAttributes) is not a string, array, or object. Parameter ignored.', 'warn');
objAttributes = {};
}

return objAttributes;
},
/**
* Creates and adds a style element to a given DOM tree.
* @param {string} styleText - The new style to be added.
* @param {document} [domElement=document] - The style will be added to this DOM tree.
*/
createStyle: function (styleText, domElement) {
if (typeof styleText !== 'string' || styleText === '') {
optg.functions[objName].log('[Version: ' + ver + '] createStyle - style text not defined, or was a blank string. No style tag added to DOM.', 'warn');
return false;
}
domElement = domElement || document;
var head = domElement.getElementsByTagName('head')[0],
style = document.createElement('style'),
rules = document.createTextNode(styleText);
style.type = 'text/css';
if (style.styleSheet) {
style.styleSheet.cssText = rules.nodeValue;
}
else {
style.appendChild(rules);
}
head.appendChild(style);
return true;
},
/**
* Determines which protocol to use for the counter URLs.
* @param {String} [forceOverwriteValue=undefined] - The string to be used instead of expected protocol string.
* Note: Used only in special occasions, otherwise leave undefined.
* @returns {String} - The beginning portion of the array needed
*/
counterProtocol: function (forceOverwriteValue) {
var protocol;
if (typeof forceOverwriteValue !== 'undefined') {
protocol = forceOverwriteValue;
} else {
if (optg.variables[objName].consoleType == 'new') {
if (optg.variables[objName].secure) {
protocol = "https://secure.marketinghub.hp.com/by/counter";
} else {
protocol = "http://by.marketinghub.hp.com/counter";
}
} else {
if (optg.variables[objName].secure) {
protocol = "https://by.essl.optimost.com/by/counter";
} else {
protocol = "http://by.optimost.com/counter";
}
}
}

return protocol;
},
/**
* Checks to see if all items in rollCall are true. If so, will display the main content section.
* This is to prevent flicker while we manipulate the DOM.
* @returns {boolean} True if all items in rollCall are set to true, otherwise false.
*/
displayContent: function () {
var item,
success = true;

for (item in optg.variables[objName].rollCall) {
if (!optg.variables[objName].rollCall[item]) {
success = false;
break;
}
}
if (success) {
optg.functions[objName].createStyle((optg.variables[objName].sectionToHide) + " {display: block!important;}");
optg.functions[objName].log('Content section being displayed.', 'normal');
}
return success;
},
/**
* Makes a call to the counter.
* @param {String|Number} counterNumber - The counter number to call.
* @param {Object|Array|String} [objAttributes] - An object of name-value pairs to be passed to the counter.
*/
executeCounterCall: function (counterNumber, objAttributes) {
var rand, url, arrQueryString;
if (typeof counterNumber != 'number') {
return false;
}

objAttributes = optg.functions[objName].convertLegacyParameters(objAttributes);

arrQueryString = optg.functions[objName].generateAttributesArray(objAttributes);

rand = Math.floor(Math.random() * 10000);
url = optg.functions[objName].counterProtocol() + "/" + optg.variables[objName].clientId + "/-/" + counterNumber + "/event.gif?" + arrQueryString.join('') + optg.variables[objName].liid + "=" + optg.functions[objName].collectLiid() + "&session=" + rand;
optg.functions[objName].addImageCounter(url);
return true;
},
collectLiid: function () {
var cookieArray, j;
cookieArray = document.cookie.split(';');
for (j = 0; j < cookieArray.length; ++j) {
var pair = cookieArray[j].split('=');
if (pair.length > 1 && pair[0].indexOf(optg.variables[objName].liid) !== -1) {
return pair[1];
}
}
return '';
},
/**
* Executes a function when a specific element is found on the page using jQuery and executed through the default
* or given jQuery function. The function can be the default add counter function to the element,
* or a passed in function.
* @example
* // Simple counter call, no attributes
* elementDependentExecution('.ctaButton', 281);
*
* // Basic counter call with all parameters provided
* elementDependentExecution('.footElement', 167, 'each', 320);
*
* // Complex call, custom function executed on the jQuery click function
* elementDependentExecution('input[type=text]', function() {
*     executeCounterCall(132, this.value);
* }, 'click');
* @param {String} jquerySelector - The jQuery selector used to find the required element.
* @param {Number|Function} counterOrFunction - The counter number used for the default click tracking function,
* or the function to execute when the element is found with the jQuery selector.
* @param {String} [jQueryEvent='each'] - The jQuery function used on the elements returned from the jQuery selector.
* This must be an existing jQuery event.
* Examples: each, click, blur, keyup, hover, scroll, submit, etc...
* @param {Number} [timeoutMax='160'] - The maximum number of checks for the required element before timing out. There
* is a 50ms timeout between each check. Defaulted to 8 seconds.
* @param {Number} [timeout=0] - The timeout counter
*/
elementDependentExecution: function (jquerySelector, counterOrFunction, jQueryEvent, timeoutMax, timeout) {
var definedFunction;
if (typeof jquerySelector === 'undefined' || jquerySelector === '') {
optg.functions[objName].log('[Version: ' + ver + '] elementDependentExecution - jQuery Selector was undefined or blank.', 'warn');
return false;
}
if (typeof counterOrFunction !== 'number' && typeof counterOrFunction !== 'function') {
optg.functions[objName].log('[Version: ' + ver + '] elementDependentExecution - Counter number is not a number nor a function to execute on jQuery selector.', 'error');
return false;
}
if (typeof jQuery !== 'function') {
optg.functions[objName].log('[Version: ' + ver + '] elementDependentExecution - jQuery could not be found.', 'error');
return false;
}

if (typeof jQueryEvent === 'undefined') {
jQueryEvent = 'each';
}
if (typeof timeoutMax !== 'number' || timeoutMax < 1) {
timeoutMax = 160;
}
if (typeof counterOrFunction === 'function') {
definedFunction = counterOrFunction;
} else {
definedFunction = function () {
optg.functions[objName].addClickTrackingToElement(this, counterOrFunction);
};
}
if (typeof timeout != 'number') {
timeout = 0;
}

if (jQuery(jquerySelector).length === 0) {
timeout += 1;
if (timeout > timeoutMax) {
optg.functions[objName].log('[Version: ' + ver + '] elementDependentExecution - jQuery Selector (' + jquerySelector + ') was not found. Timed out on attempt: ' + (timeout - 1), 'warn');
return false;
}
setTimeout(function () {
optg.functions[objName].elementDependentExecution(jquerySelector, counterOrFunction, jQueryEvent, timeoutMax, timeout);
}, 50);
return;
}

try {
jQuery(jquerySelector)[jQueryEvent](definedFunction);
} catch (e) {
optg.functions[objName].log('[Version: ' + ver + '] elementDependentExecution - Failed to execute jQuery code:\njquerySelector: ' + jquerySelector + '\njQueryEvent: ' + jQueryEvent + '\ndefinedFunction: ' + definedFunction + '\nError Message: ' + e.toString(), 'error');
return false;
}
return true;
},
/**
* Generates an array containing the name-value pair strings for the query string.
* @param {Object} objAttributes - An object of name-value pairs.
* @param {String} [joinString=&] - The string value use to separate the name-value pairs from each other.
* @returns {Array|Object} of name-value pairs in string form or given item.
*/
generateAttributesArray: function (objAttributes, joinString) {
var arrQueryString, name;
if (optg.functions[objName].checkClass(objAttributes) !== 'object') {
optg.functions[objName].log('[Version: ' + ver + '] generateAttributesArray - objAttributes was not an object. No array created. objAttributes returned unmodified.', 'err');
return objAttributes;
}
if (typeof joinString !== 'string') {
joinString = '&';
}

arrQueryString = [];
for (name in objAttributes) {
arrQueryString[arrQueryString.length] = encodeURIComponent(name) + '=' + encodeURIComponent(objAttributes[name]) + joinString;
}
return arrQueryString;
},
/**
* Logs a message to the console, if the console is defined. This function does nothing in browsers that do
* not support the console, such as IE 6 or if debug (found in variables section above) is off.
* @param {String} message The message to send to the console.
* @param {String} [type=normal] The type of message. (error, warning, info, or normal)
* @returns {Boolean} True if logging was successful, otherwise false.
*/
log: function (message, type) {
if (!optg.variables[objName].debug) {
return false;
}
if (typeof console == 'undefined' || !console
|| !console.log || typeof(console.log) != "function"
|| !console.error || typeof(console.error) != "function"
|| !console.warn || typeof(console.warn) != "function"
|| !console.info || typeof(console.info) != "function") {
return false;
}

if (typeof type == 'undefined') {
type = 'normal';
}

switch (type.toLowerCase()) {
case 'err':
case 'error':
console.error(message);
break;
case 'warn':
case 'warning':
console.warn(message);
break;
case 'info':
console.info(message);
break;
case 'write':
case 'normal':
default:
console.log(message);
break;
}
return true;
},
/**
* Logs the status of a variable to prevent the premature displaying of content.
* Used in conjunction with the function displayContent.
* @param {String} fileName The name of the variable with the given status.
* @param {Boolean} status The current status. True if ready, false if not ready.
*/
logStatus: function (fileName, status) {
optg.variables[objName].rollCall[fileName] = status;
if (status) {
optg.functions[objName].log("Variable '" + fileName + "' has successfully fired.", "normal");
}
else {
optg.functions[objName].log("Variable '" + fileName + "' has failed to fired.", "warn");
}
},
/**
* Prints a report with the basic information on the test, such as client id, client liid, and section to
* hide (as CSS). The function will displays an alert, if the content for client liid or id is not updated.
* NOTE: This helps prevent forgetting to set it when creating a new template and having to catch it during QA.
* This function will only work properly if debug mode is true (in variables) and in the QA persona.
*/
reviewSetup: function () {
var arrLogMessages;
arrLogMessages = [];

arrLogMessages[arrLogMessages.length] = optg.variables[objName].creative;
arrLogMessages[arrLogMessages.length] = '--------------------------------------------------';
if (optg.variables[objName].liid.length == 0 || optg.variables[objName].clientId.length == 0) {
window.alert("[Setup] The liid and/or client ID has not been set in the setup block.");
} else {
arrLogMessages[arrLogMessages.length] = 'The liid is set: ' + optg.variables[objName].liid;
arrLogMessages[arrLogMessages.length] = 'The client ID is set: ' + optg.variables[objName].clientId;
}
arrLogMessages[arrLogMessages.length] = 'The section to hide then display (CSS representation): ' + optg.variables[objName].sectionToHide;
optg.functions[objName].log(arrLogMessages.join('\n'), 'info');
}
};
optg.functions[objName].createStyle((optg.variables[objName].sectionToHide) + ' {display: none!important;}');

if (optg.variables[objName].QApersona) {
optg.functions[objName].reviewSetup();
}

})("opt13", {
consoleType: 'new',
clientId: '1799',
liid: 'op1799openboxliid',
sectionToHide: ''
});
(function () {
var currentWave = "opt13";
var styleX = '';

if(optg.variables[currentWave].QApersona) {
document.title = optg.variables[currentWave].creative + document.title;
}

window.loadSearchResults = function(e) {
$("#toTop").hide();
var a = readCookie("storeSelected"),
t = e.replace("/search/search_results.aspx", "/endeca/AjaxResults.aspx");
t.toLowerCase().indexOf(a) <= 0 && (t = t + "&storeid=" + a),
$.ajax({
type: "GET",
url: t,
success: function(a) {
if (a.length >= 6) {
window.setTimeout(function() {
$("#Aria-Announcements").html('<div aria-live="polite"  role="alert">Main search content has been updated.</div>')
}, 5e3),
window.setTimeout(function() {
$("#Aria-Announcements").html("")
}, 15e3),
$("#content").html(a);
try {
if ($(window).scrollTop() + 25 > $("#resultsBottom").offset().top) {
var t = $(window).scrollTop() - $("#resultsBottom").offset().top + 50;
$("#toTop").css({
top: t + "px"
}),
$("#toTop").fadeIn("slow")
}
} catch (n) {}
bindAjaxClick(e);
try {
if (history.pushState) {
if (e.toLowerCase().indexOf("mystore=") <= 0) {
var o = readCookie("myStore");
e = e.indexOf("?") > 0 ? e + "&myStore=" + o : e + "?myStore=" + o
}
history.pushState(null, null, e)
}
} catch (n) {}
optg.functions[currentWave].fireTest(e);
}
}
})
};

optg.functions[currentWave].fireTest = function (url) {
if (url.indexOf('prt=clearance') === -1) {
return false;
}
styleX += [
'.opStrike { text-decoration: line-through; font-size: 16px; } .opNewValue { color: grey; font-size: 20px; padding-top: 5px!important; } #productGrid.col1 .price span .mini { width: 170px; display: inline-block; } #productGrid div.price .price-label { margin-top: 0px; } .opNewValue .upper { font-size: 14px!important; }',
''].join('');
optg.functions[currentWave].areaA_1 = function(timeout) {

if (typeof timeout != 'number') {
timeout = 0;
}

timeout += 1;
if (typeof jQuery == 'undefined' || jQuery('.details .price').length === 0) {
if (timeout > 20) {
return;
}
optg.functions[currentWave].logStatus('areaA_1', false);
setTimeout(function() {
optg.functions[currentWave].areaA_1(timeout);
}, 200);
return;
}

optg.functions[currentWave].elementDependentExecution('.details .price', function() {
var costNew;
costNew = jQuery('.compareTo', this).html() || '';
costNew = costNew.replace(/Compare to new: |<[^>]*>|\$|,/g, '');
costNew = parseFloat(costNew);
jQuery('.mini:contains(from)', this).html('Open Box from ');
jQuery('.compareTo', this).remove();
if (!isNaN(costNew)) {
jQuery(this).prepend('<div class="opNewValue"><span class="mini">New </span><span class="upper">$</span><span class="opStrike">' + costNew.toString().replace(/(\d{1,3})(\d{3})/, '$1,$2') + '</span></div>');
}
});
optg.functions[currentWave].elementDependentExecution('.price_wrapper .clearance', function() {
jQuery(this).remove();
});

optg.functions[currentWave].logStatus('areaA_1', true);
if (timeout > 0) {
optg.functions[currentWave].displayContent();
}
};
optg.functions[currentWave].areaA_1(0);
styleX += [
'.nextButton { background: #ab5225 none repeat scroll 0 0!important; border: 1px solid #ab5225!important; }',
''].join('');
styleX += [
'',
''].join('');
optg.functions[currentWave].areaC_1 = function(timeout) {

if (typeof timeout != 'number') {
timeout = 0;
}

timeout += 1;
if (typeof jQuery == 'undefined' || jQuery('.nextButton a').length === 0) {
if (timeout > 20) {
return;
}
optg.functions[currentWave].logStatus('areaC_1', false);
setTimeout(function() {
optg.functions[currentWave].areaC_1(timeout);
}, 200);
return;
}

jQuery('.nextButton a').html('Buy Open Box');

optg.functions[currentWave].logStatus('areaC_1', true);
if (timeout > 0) {
optg.functions[currentWave].displayContent();
}
};
optg.functions[currentWave].areaC_1(0);
optimost.appHelper.reportPageView( optimost.lookupTrial( 4 ) );

optg.functions[currentWave].area_Counters(0);
optg.functions[currentWave].createStyle(styleX);
return true;
};

optg.functions[currentWave].area_Counters = function(timeout) {
if (typeof timeout != 'number') {
timeout = 0;
}

timeout += 1;
if (typeof jQuery == 'undefined') {
if (timeout > 20) {
return;
}
setTimeout(function() {
optg.functions[currentWave].area_Counters(timeout);
}, 200);
return;
}

optg.functions[currentWave].elementDependentExecution('.nextButton a', 4);
};

optg.functions[currentWave].fireTest(document.location.href);
optg.functions[currentWave].displayContent();
})();
if(typeof _mvtEnabled !== 'undefined'){(function(){
 var impr = { subjectId:"unknown", placementId:"4", segmentId:"5", waveId:"13", creativeId:"9", visitorId:"2c5ok04316n1", impressionId:"2c5ok04316n1",value0:"NULL",value1:"NULL",attributevalue0:"NULL",attributevalue1:"NULL" },
    dmh=window.dmh||{}, runtime=dmh.runtime||{}, info=runtime.info||{}, push=info.pushImpression||function(){};
_add_mvtParam( { segmentId:impr.segmentId, waveId:impr.waveId, creativeId:impr.creativeId, visitorId:impr.visitorId, impressionId2c5ok04316n1:{ segmentId:impr.segmentId, waveId:impr.waveId, creativeId:impr.creativeId, visitorId:impr.visitorId, impressionId:impr.impressionId, value0:impr.value0, value1:impr.value1, attributevalue0:impr.attributevalue0, attributevalue1:impr.attributevalue1 } } );
push(impr);})();}

