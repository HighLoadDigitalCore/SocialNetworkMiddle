/*! HTML5 Shiv v3 | @jon_neal @afarkas @rem | MIT/GPL2 Licensed
 *  Uncompressed source: https://github.com/aFarkas/html5shiv
*/
(function(a,b){function f(a){var c,d,e,f;b.documentMode>7?(c=b.createElement("font"),c.setAttribute("data-html5shiv",a.nodeName.toLowerCase())):c=b.createElement("shiv:"+a.nodeName);while(a.firstChild)c.appendChild(a.childNodes[0]);for(d=a.attributes,e=d.length,f=0;f<e;++f)d[f].specified&&c.setAttribute(d[f].nodeName,d[f].nodeValue);c.style.cssText=a.style.cssText,a.parentNode.replaceChild(c,a),c.originalElement=a}function g(a){var b=a.originalElement;while(a.childNodes.length)b.appendChild(a.childNodes[0]);a.parentNode.replaceChild(b,a)}function h(a,b){b=b||"all";var c=-1,d=[],e=a.length,f,g;while(++c<e){f=a[c],g=f.media||b;if(f.disabled||!/print|all/.test(g))continue;d.push(h(f.imports,g),f.cssText)}return d.join("")}function i(c){var d=new RegExp("(^|[\\s,{}])("+a.html5.elements.join("|")+")","gi"),e=c.split("{"),f=e.length,g=-1;while(++g<f)e[g]=e[g].split("}"),b.documentMode>7?e[g][e[g].length-1]=e[g][e[g].length-1].replace(d,'$1font[data-html5shiv="$2"]'):e[g][e[g].length-1]=e[g][e[g].length-1].replace(d,"$1shiv\\:$2"),e[g]=e[g].join("}");return e.join("{")}var c=function(a){return a.innerHTML="<x-element></x-element>",a.childNodes.length===1}(b.createElement("a")),d=function(a,b,c){return b.appendChild(a),(c=(c?c(a):a.currentStyle).display)&&b.removeChild(a)&&c==="block"}(b.createElement("nav"),b.documentElement,a.getComputedStyle),e={elements:"abbr article aside audio bdi canvas data datalist details figcaption figure footer header hgroup mark meter nav output progress section summary time video".split(" "),shivDocument:function(a){a=a||b;if(a.documentShived)return;a.documentShived=!0;var f=a.createElement,g=a.createDocumentFragment,h=a.getElementsByTagName("head")[0],i=function(a){f(a)};c||(e.elements.join(" ").replace(/\w+/g,i),a.createElement=function(a){var b=f(a);return b.canHaveChildren&&e.shivDocument(b.document),b},a.createDocumentFragment=function(){return e.shivDocument(g())});if(!d&&h){var j=f("div");j.innerHTML=["x<style>","article,aside,details,figcaption,figure,footer,header,hgroup,nav,section{display:block}","audio{display:none}","canvas,video{display:inline-block;*display:inline;*zoom:1}","[hidden]{display:none}audio[controls]{display:inline-block;*display:inline;*zoom:1}","mark{background:#FF0;color:#000}","</style>"].join(""),h.insertBefore(j.lastChild,h.firstChild)}return a}};e.shivDocument(b),a.html5=e;if(c||!a.attachEvent)return;a.attachEvent("onbeforeprint",function(){if(a.html5.supportsXElement||!b.namespaces)return;b.namespaces.shiv||b.namespaces.add("shiv");var c=-1,d=new RegExp("^("+a.html5.elements.join("|")+")$","i"),e=b.getElementsByTagName("*"),g=e.length,j,k=i(h(function(a,b){var c=[],d=a.length;while(d)c.unshift(a[--d]);d=b.length;while(d)c.unshift(b[--d]);c.sort(function(a,b){return a.sourceIndex-b.sourceIndex}),d=c.length;while(d)c[--d]=c[d].styleSheet;return c}(b.getElementsByTagName("style"),b.getElementsByTagName("link"))));while(++c<g)j=e[c],d.test(j.nodeName)&&f(j);b.appendChild(b._shivedStyleSheet=b.createElement("style")).styleSheet.cssText=k}),a.attachEvent("onafterprint",function(){if(a.html5.supportsXElement||!b.namespaces)return;var c=-1,d=b.getElementsByTagName("*"),e=d.length,f;while(++c<e)f=d[c],f.originalElement&&g(f);b._shivedStyleSheet&&b._shivedStyleSheet.parentNode.removeChild(b._shivedStyleSheet)})})(this,document);
/*! matchMedia() polyfill - Test a CSS media type/query in JS. Authors & copyright (c) 2012: Scott Jehl, Paul Irish, Nicholas Zakas. Dual MIT/BSD license */
/*! NOTE: If you're already including a window.matchMedia polyfill via Modernizr or otherwise, you don't need this part */
window.matchMedia=window.matchMedia||function(a){"use strict";var c,d=a.documentElement,e=d.firstElementChild||d.firstChild,f=a.createElement("body"),g=a.createElement("div");return g.id="mq-test-1",g.style.cssText="position:absolute;top:-100em",f.style.background="none",f.appendChild(g),function(a){return g.innerHTML='&shy;<style media="'+a+'"> #mq-test-1 { width: 42px; }</style>',d.insertBefore(f,e),c=42===g.offsetWidth,d.removeChild(f),{matches:c,media:a}}}(document);
/*! Respond.js v1.1.0: min/max-width media query polyfill. (c) Scott Jehl. MIT/GPLv2 Lic. j.mp/respondjs  */
(function(a){"use strict";function x(){u(!0)}var b={};if(a.respond=b,b.update=function(){},b.mediaQueriesSupported=a.matchMedia&&a.matchMedia("only all").matches,!b.mediaQueriesSupported){var q,r,t,c=a.document,d=c.documentElement,e=[],f=[],g=[],h={},i=30,j=c.getElementsByTagName("head")[0]||d,k=c.getElementsByTagName("base")[0],l=j.getElementsByTagName("link"),m=[],n=function(){for(var b=0;l.length>b;b++){var c=l[b],d=c.href,e=c.media,f=c.rel&&"stylesheet"===c.rel.toLowerCase();d&&f&&!h[d]&&(c.styleSheet&&c.styleSheet.rawCssText?(p(c.styleSheet.rawCssText,d,e),h[d]=!0):(!/^([a-zA-Z:]*\/\/)/.test(d)&&!k||d.replace(RegExp.$1,"").split("/")[0]===a.location.host)&&m.push({href:d,media:e}))}o()},o=function(){if(m.length){var b=m.shift();v(b.href,function(c){p(c,b.href,b.media),h[b.href]=!0,a.setTimeout(function(){o()},0)})}},p=function(a,b,c){var d=a.match(/@media[^\{]+\{([^\{\}]*\{[^\}\{]*\})+/gi),g=d&&d.length||0;b=b.substring(0,b.lastIndexOf("/"));var h=function(a){return a.replace(/(url\()['"]?([^\/\)'"][^:\)'"]+)['"]?(\))/g,"$1"+b+"$2$3")},i=!g&&c;b.length&&(b+="/"),i&&(g=1);for(var j=0;g>j;j++){var k,l,m,n;i?(k=c,f.push(h(a))):(k=d[j].match(/@media *([^\{]+)\{([\S\s]+?)$/)&&RegExp.$1,f.push(RegExp.$2&&h(RegExp.$2))),m=k.split(","),n=m.length;for(var o=0;n>o;o++)l=m[o],e.push({media:l.split("(")[0].match(/(only\s+)?([a-zA-Z]+)\s?/)&&RegExp.$2||"all",rules:f.length-1,hasquery:l.indexOf("(")>-1,minw:l.match(/\(\s*min\-width\s*:\s*(\s*[0-9\.]+)(px|em)\s*\)/)&&parseFloat(RegExp.$1)+(RegExp.$2||""),maxw:l.match(/\(\s*max\-width\s*:\s*(\s*[0-9\.]+)(px|em)\s*\)/)&&parseFloat(RegExp.$1)+(RegExp.$2||"")})}u()},s=function(){var a,b=c.createElement("div"),e=c.body,f=!1;return b.style.cssText="position:absolute;font-size:1em;width:1em",e||(e=f=c.createElement("body"),e.style.background="none"),e.appendChild(b),d.insertBefore(e,d.firstChild),a=b.offsetWidth,f?d.removeChild(e):e.removeChild(b),a=t=parseFloat(a)},u=function(b){var h="clientWidth",k=d[h],m="CSS1Compat"===c.compatMode&&k||c.body[h]||k,n={},o=l[l.length-1],p=(new Date).getTime();if(b&&q&&i>p-q)return a.clearTimeout(r),r=a.setTimeout(u,i),void 0;q=p;for(var v in e)if(e.hasOwnProperty(v)){var w=e[v],x=w.minw,y=w.maxw,z=null===x,A=null===y,B="em";x&&(x=parseFloat(x)*(x.indexOf(B)>-1?t||s():1)),y&&(y=parseFloat(y)*(y.indexOf(B)>-1?t||s():1)),w.hasquery&&(z&&A||!(z||m>=x)||!(A||y>=m))||(n[w.media]||(n[w.media]=[]),n[w.media].push(f[w.rules]))}for(var C in g)g.hasOwnProperty(C)&&g[C]&&g[C].parentNode===j&&j.removeChild(g[C]);for(var D in n)if(n.hasOwnProperty(D)){var E=c.createElement("style"),F=n[D].join("\n");E.type="text/css",E.media=D,j.insertBefore(E,o.nextSibling),E.styleSheet?E.styleSheet.cssText=F:E.appendChild(c.createTextNode(F)),g.push(E)}},v=function(a,b){var c=w();c&&(c.open("GET",a,!0),c.onreadystatechange=function(){4!==c.readyState||200!==c.status&&304!==c.status||b(c.responseText)},4!==c.readyState&&c.send(null))},w=function(){var b=!1;try{b=new a.XMLHttpRequest}catch(c){b=new a.ActiveXObject("Microsoft.XMLHTTP")}return function(){return b}}();n(),b.update=n,a.addEventListener?a.addEventListener("resize",x,!1):a.attachEvent&&a.attachEvent("onresize",x)}})(this);
/*
 * selectivizr v1.0.3b - (c) Keith Clark, freely distributable under the terms of the MIT license.
 * selectivizr.com
*/
(function(win){var ieUserAgent=navigator.userAgent.match(/MSIE (\d+)/);if(!ieUserAgent){return false}var doc=document;var root=doc.documentElement;var xhr=getXHRObject();var ieVersion=ieUserAgent[1];if(doc.compatMode!="CSS1Compat"||ieVersion<6||ieVersion>8||!xhr){return}var selectorEngines={NW:"*.Dom.select",MooTools:"$$",DOMAssistant:"*.$",Prototype:"$$",YAHOO:"*.util.Selector.query",Sizzle:"*",jQuery:"*",dojo:"*.query"};var selectorMethod;var enabledWatchers=[];var domPatches=[];var ie6PatchID=0;var patchIE6MultipleClasses=true;var namespace="slvzr";var RE_COMMENT=/(\/\*[^*]*\*+([^\/][^*]*\*+)*\/)\s*?/g;var RE_IMPORT=/@import\s*(?:(?:(?:url\(\s*(['"]?)(.*)\1)\s*\))|(?:(['"])(.*)\3))\s*([^;]*);/g;var RE_ASSET_URL=/(behavior\s*?:\s*)?\burl\(\s*(["']?)(?!data:)([^"')]+)\2\s*\)/g;var RE_PSEUDO_STRUCTURAL=/^:(empty|(first|last|only|nth(-last)?)-(child|of-type))$/;var RE_PSEUDO_ELEMENTS=/:(:first-(?:line|letter))/g;var RE_SELECTOR_GROUP=/((?:^|(?:\s*})+)(?:\s*@media[^{]+{)?)\s*([^\{]*?[\[:][^{]+)/g;var RE_SELECTOR_PARSE=/([ +~>])|(:[a-z-]+(?:\(.*?\)+)?)|(\[.*?\])/g;var RE_LIBRARY_INCOMPATIBLE_PSEUDOS=/(:not\()?:(hover|enabled|disabled|focus|checked|target|active|visited|first-line|first-letter)\)?/g;var RE_PATCH_CLASS_NAME_REPLACE=/[^\w-]/g;var RE_INPUT_ELEMENTS=/^(INPUT|SELECT|TEXTAREA|BUTTON)$/;var RE_INPUT_CHECKABLE_TYPES=/^(checkbox|radio)$/;var BROKEN_ATTR_IMPLEMENTATIONS=ieVersion>6?/[\$\^*]=(['"])\1/:null;var RE_TIDY_TRAILING_WHITESPACE=/([(\[+~])\s+/g;var RE_TIDY_LEADING_WHITESPACE=/\s+([)\]+~])/g;var RE_TIDY_CONSECUTIVE_WHITESPACE=/\s+/g;var RE_TIDY_TRIM_WHITESPACE=/^\s*((?:[\S\s]*\S)?)\s*$/;var EMPTY_STRING="";var SPACE_STRING=" ";var PLACEHOLDER_STRING="$1";function patchStyleSheet(cssText){return cssText.replace(RE_PSEUDO_ELEMENTS,PLACEHOLDER_STRING).replace(RE_SELECTOR_GROUP,function(m,prefix,selectorText){var selectorGroups=selectorText.split(",");for(var c=0,cs=selectorGroups.length;c<cs;c++){var selector=normalizeSelectorWhitespace(selectorGroups[c])+SPACE_STRING;var patches=[];selectorGroups[c]=selector.replace(RE_SELECTOR_PARSE,function(match,combinator,pseudo,attribute,index){if(combinator){if(patches.length>0){domPatches.push({selector:selector.substring(0,index),patches:patches});patches=[]}return combinator}else{var patch=(pseudo)?patchPseudoClass(pseudo):patchAttribute(attribute);if(patch){patches.push(patch);return"."+patch.className}return match}})}return prefix+selectorGroups.join(",")})}function patchAttribute(attr){return(!BROKEN_ATTR_IMPLEMENTATIONS||BROKEN_ATTR_IMPLEMENTATIONS.test(attr))?{className:createClassName(attr),applyClass:true}:null}function patchPseudoClass(pseudo){var applyClass=true;var className=createClassName(pseudo.slice(1));var isNegated=pseudo.substring(0,5)==":not(";var activateEventName;var deactivateEventName;if(isNegated){pseudo=pseudo.slice(5,-1)}var bracketIndex=pseudo.indexOf("(");if(bracketIndex>-1){pseudo=pseudo.substring(0,bracketIndex)}if(pseudo.charAt(0)==":"){switch(pseudo.slice(1)){case"root":applyClass=function(e){return isNegated?e!=root:e==root};break;case"target":if(ieVersion==8){applyClass=function(e){var handler=function(){var hash=location.hash;var hashID=hash.slice(1);return isNegated?(hash==EMPTY_STRING||e.id!=hashID):(hash!=EMPTY_STRING&&e.id==hashID)};addEvent(win,"hashchange",function(){toggleElementClass(e,className,handler())});return handler()};break}return false;case"checked":applyClass=function(e){if(RE_INPUT_CHECKABLE_TYPES.test(e.type)){addEvent(e,"propertychange",function(){if(event.propertyName=="checked"){toggleElementClass(e,className,e.checked!==isNegated)}})}return e.checked!==isNegated};break;case"disabled":isNegated=!isNegated;case"enabled":applyClass=function(e){if(RE_INPUT_ELEMENTS.test(e.tagName)){addEvent(e,"propertychange",function(){if(event.propertyName=="$disabled"){toggleElementClass(e,className,e.$disabled===isNegated)}});enabledWatchers.push(e);e.$disabled=e.disabled;return e.disabled===isNegated}return pseudo==":enabled"?isNegated:!isNegated};break;case"focus":activateEventName="focus";deactivateEventName="blur";case"hover":if(!activateEventName){activateEventName="mouseenter";deactivateEventName="mouseleave"}applyClass=function(e){addEvent(e,isNegated?deactivateEventName:activateEventName,function(){toggleElementClass(e,className,true)});addEvent(e,isNegated?activateEventName:deactivateEventName,function(){toggleElementClass(e,className,false)});return isNegated};break;default:if(!RE_PSEUDO_STRUCTURAL.test(pseudo)){return false}break}}return{className:className,applyClass:applyClass}}function applyPatches(){var elms,selectorText,patches,domSelectorText;for(var c=0;c<domPatches.length;c++){selectorText=domPatches[c].selector;patches=domPatches[c].patches;domSelectorText=selectorText.replace(RE_LIBRARY_INCOMPATIBLE_PSEUDOS,EMPTY_STRING);if(domSelectorText==EMPTY_STRING||domSelectorText.charAt(domSelectorText.length-1)==SPACE_STRING){domSelectorText+="*"}try{elms=selectorMethod(domSelectorText)}catch(ex){log("Selector '"+selectorText+"' threw exception '"+ex+"'")}if(elms){for(var d=0,dl=elms.length;d<dl;d++){var elm=elms[d];var cssClasses=elm.className;for(var f=0,fl=patches.length;f<fl;f++){var patch=patches[f];if(!hasPatch(elm,patch)){if(patch.applyClass&&(patch.applyClass===true||patch.applyClass(elm)===true)){cssClasses=toggleClass(cssClasses,patch.className,true)}}}elm.className=cssClasses}}}}function hasPatch(elm,patch){return new RegExp("(^|\\s)"+patch.className+"(\\s|$)").test(elm.className)}function createClassName(className){return namespace+"-"+((ieVersion==6&&patchIE6MultipleClasses)?ie6PatchID++:className.replace(RE_PATCH_CLASS_NAME_REPLACE,function(a){return a.charCodeAt(0)}))}function log(message){if(win.console){win.console.log(message)}}function trim(text){return text.replace(RE_TIDY_TRIM_WHITESPACE,PLACEHOLDER_STRING)}function normalizeWhitespace(text){return trim(text).replace(RE_TIDY_CONSECUTIVE_WHITESPACE,SPACE_STRING)}function normalizeSelectorWhitespace(selectorText){return normalizeWhitespace(selectorText.replace(RE_TIDY_TRAILING_WHITESPACE,PLACEHOLDER_STRING).replace(RE_TIDY_LEADING_WHITESPACE,PLACEHOLDER_STRING))}function toggleElementClass(elm,className,on){var oldClassName=elm.className;var newClassName=toggleClass(oldClassName,className,on);if(newClassName!=oldClassName){elm.className=newClassName;elm.parentNode.className+=EMPTY_STRING}}function toggleClass(classList,className,on){var re=RegExp("(^|\\s)"+className+"(\\s|$)");var classExists=re.test(classList);if(on){return classExists?classList:classList+SPACE_STRING+className}else{return classExists?trim(classList.replace(re,PLACEHOLDER_STRING)):classList}}function addEvent(elm,eventName,eventHandler){elm.attachEvent("on"+eventName,eventHandler)}function getXHRObject(){if(win.XMLHttpRequest){return new XMLHttpRequest}try{return new ActiveXObject("Microsoft.XMLHTTP")}catch(e){return null}}function loadStyleSheet(url){xhr.open("GET",url,false);xhr.send();return(xhr.status==200)?xhr.responseText:EMPTY_STRING}function resolveUrl(url,contextUrl,ignoreSameOriginPolicy){function getProtocol(url){return url.substring(0,url.indexOf("//"))}function getProtocolAndHost(url){return url.substring(0,url.indexOf("/",8))}if(!contextUrl){contextUrl=baseUrl}if(url.substring(0,2)=="//"){url=getProtocol(contextUrl)+url}if(/^https?:\/\//i.test(url)){return !ignoreSameOriginPolicy&&getProtocolAndHost(contextUrl)!=getProtocolAndHost(url)?null:url}if(url.charAt(0)=="/"){return getProtocolAndHost(contextUrl)+url}var contextUrlPath=contextUrl.split(/[?#]/)[0];if(url.charAt(0)!="?"&&contextUrlPath.charAt(contextUrlPath.length-1)!="/"){contextUrlPath=contextUrlPath.substring(0,contextUrlPath.lastIndexOf("/")+1)}return contextUrlPath+url}function parseStyleSheet(url){if(url){return loadStyleSheet(url).replace(RE_COMMENT,EMPTY_STRING).replace(RE_IMPORT,function(match,quoteChar,importUrl,quoteChar2,importUrl2,media){var cssText=parseStyleSheet(resolveUrl(importUrl||importUrl2,url));return(media)?"@media "+media+" {"+cssText+"}":cssText}).replace(RE_ASSET_URL,function(match,isBehavior,quoteChar,assetUrl){quoteChar=quoteChar||EMPTY_STRING;return isBehavior?match:" url("+quoteChar+resolveUrl(assetUrl,url,true)+quoteChar+") "})}return EMPTY_STRING}function getStyleSheets(){var url,stylesheet;for(var c=0;c<doc.styleSheets.length;c++){stylesheet=doc.styleSheets[c];if(stylesheet.href!=EMPTY_STRING){url=resolveUrl(stylesheet.href);if(url){stylesheet.cssText=stylesheet.rawCssText=patchStyleSheet(parseStyleSheet(url))}}}}function init(){applyPatches();if(enabledWatchers.length>0){setInterval(function(){for(var c=0,cl=enabledWatchers.length;c<cl;c++){var e=enabledWatchers[c];if(e.disabled!==e.$disabled){if(e.disabled){e.disabled=false;e.$disabled=true;e.disabled=true}else{e.$disabled=e.disabled}}}},250)}}var baseTags=doc.getElementsByTagName("BASE");var baseUrl=(baseTags.length>0)?baseTags[0].href:doc.location.href;getStyleSheets();ContentLoaded(win,function(){for(var engine in selectorEngines){var members,member,context=win;if(win[engine]){members=selectorEngines[engine].replace("*",engine).split(".");while((member=members.shift())&&(context=context[member])){}if(typeof context=="function"){selectorMethod=context;init();return}}}});
function ContentLoaded(win,fn){var done=false,top=true,init=function(e){if(e.type=="readystatechange"&&doc.readyState!="complete"){return}(e.type=="load"?win:doc).detachEvent("on"+e.type,init,false);if(!done&&(done=true)){fn.call(win,e.type||e)}},poll=function(){try{root.doScroll("left")}catch(e){setTimeout(poll,50);return}init("poll")};if(doc.readyState=="complete"){fn.call(win,EMPTY_STRING)}else{if(doc.createEventObject&&root.doScroll){try{top=!win.frameElement}catch(e){}if(top){poll()}}addEvent(doc,"readystatechange",init);addEvent(win,"load",init)}}})(this);