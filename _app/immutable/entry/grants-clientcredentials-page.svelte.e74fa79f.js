import{S as b,i as x,s as z,y as u,a as y,z as m,c as D,A as _,b as p,g as d,d as g,B as h,h as l,k as w,q as A,l as v,m as P,r as O,D as k,H as R,n as E}from"../chunks/index.2ef5bca6.js";import{D as L}from"../chunks/Diagram.9ecf90b9.js";import{P as B}from"../chunks/PageTitle.4f3ee9c3.js";import{S as q}from"../chunks/Section.7d0b7be1.js";function H(c){let n,s;return{c(){n=w("p"),s=A(`Allows confidential clients which do not use resource owners,
        to allow themselves authorization without interaction,
        by simply requesting the token endpoint for access tokens using client credentials.`)},l(e){n=v(e,"P",{});var a=P(n);s=O(a,`Allows confidential clients which do not use resource owners,
        to allow themselves authorization without interaction,
        by simply requesting the token endpoint for access tokens using client credentials.`),a.forEach(l)},m(e,a){p(e,n,a),k(n,s)},p:R,d(e){e&&l(n)}}}function T(c){let n,s,e,a;return{c(){n=w("ul"),s=w("li"),e=w("a"),a=A("OAuth 2.1"),this.h()},l(r){n=v(r,"UL",{class:!0});var f=P(n);s=v(f,"LI",{});var i=P(s);e=v(i,"A",{href:!0});var $=P(e);a=O($,"OAuth 2.1"),$.forEach(l),i.forEach(l),f.forEach(l),this.h()},h(){E(e,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08"),E(n,"class","list-disc")},m(r,f){p(r,n,f),k(n,s),k(s,e),k(e,a)},p:R,d(r){r&&l(n)}}}function U(c){let n=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`,s;return{c(){s=A(n)},l(e){s=O(e,n)},m(e,a){p(e,s,a)},p:R,d(e){e&&l(s)}}}function j(c){let n,s;return n=new L({props:{$$slots:{default:[U]},$$scope:{ctx:c}}}),{c(){u(n.$$.fragment)},l(e){m(n.$$.fragment,e)},m(e,a){_(n,e,a),s=!0},p(e,a){const r={};a&1&&(r.$$scope={dirty:a,ctx:e}),n.$set(r)},i(e){s||(d(n.$$.fragment,e),s=!0)},o(e){g(n.$$.fragment,e),s=!1},d(e){h(n,e)}}}function F(c){let n,s,e,a,r,f,i,$;return n=new B({props:{title:"Client Credentials"}}),e=new q({props:{title:"Introduction",$$slots:{default:[H]},$$scope:{ctx:c}}}),r=new q({props:{title:"Specifications",$$slots:{default:[T]},$$scope:{ctx:c}}}),i=new q({props:{title:"Client Credentials",$$slots:{default:[j]},$$scope:{ctx:c}}}),{c(){u(n.$$.fragment),s=y(),u(e.$$.fragment),a=y(),u(r.$$.fragment),f=y(),u(i.$$.fragment)},l(t){m(n.$$.fragment,t),s=D(t),m(e.$$.fragment,t),a=D(t),m(r.$$.fragment,t),f=D(t),m(i.$$.fragment,t)},m(t,o){_(n,t,o),p(t,s,o),_(e,t,o),p(t,a,o),_(r,t,o),p(t,f,o),_(i,t,o),$=!0},p(t,[o]){const C={};o&1&&(C.$$scope={dirty:o,ctx:t}),e.$set(C);const I={};o&1&&(I.$$scope={dirty:o,ctx:t}),r.$set(I);const S={};o&1&&(S.$$scope={dirty:o,ctx:t}),i.$set(S)},i(t){$||(d(n.$$.fragment,t),d(e.$$.fragment,t),d(r.$$.fragment,t),d(i.$$.fragment,t),$=!0)},o(t){g(n.$$.fragment,t),g(e.$$.fragment,t),g(r.$$.fragment,t),g(i.$$.fragment,t),$=!1},d(t){h(n,t),t&&l(s),h(e,t),t&&l(a),h(r,t),t&&l(f),h(i,t)}}}class N extends b{constructor(n){super(),x(this,n,null,F,z,{})}}export{N as default};
