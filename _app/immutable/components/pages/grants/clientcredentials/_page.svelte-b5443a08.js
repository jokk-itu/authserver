import{S as x,i as b,s as z,x as u,a as y,y as m,c as q,z as _,b as p,f as d,t as g,A as h,h as l,k as w,q as C,l as v,m as P,r as D,E as k,C as O,n as S}from"../../../../chunks/index-86aa8d89.js";import{D as L}from"../../../../chunks/Diagram-a4d00bce.js";import{P as T}from"../../../../chunks/PageTitle-cc378c43.js";import{S as A}from"../../../../chunks/Section-4061cb73.js";function U(c){let n,s;return{c(){n=w("p"),s=C(`Allows confidential clients which do not use resource owners,
        to allow themselves authorization without interaction,
        by simply requesting the token endpoint for access tokens using client credentials.`)},l(e){n=v(e,"P",{});var a=P(n);s=D(a,`Allows confidential clients which do not use resource owners,
        to allow themselves authorization without interaction,
        by simply requesting the token endpoint for access tokens using client credentials.`),a.forEach(l)},m(e,a){p(e,n,a),k(n,s)},p:O,d(e){e&&l(n)}}}function j(c){let n,s,e,a;return{c(){n=w("ul"),s=w("li"),e=w("a"),a=C("OAuth 2.1"),this.h()},l(r){n=v(r,"UL",{class:!0});var f=P(n);s=v(f,"LI",{});var i=P(s);e=v(i,"A",{href:!0});var $=P(e);a=D($,"OAuth 2.1"),$.forEach(l),i.forEach(l),f.forEach(l),this.h()},h(){S(e,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08"),S(n,"class","list-disc")},m(r,f){p(r,n,f),k(n,s),k(s,e),k(e,a)},p:O,d(r){r&&l(n)}}}function B(c){let n=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`,s;return{c(){s=C(n)},l(e){s=D(e,n)},m(e,a){p(e,s,a)},p:O,d(e){e&&l(s)}}}function F(c){let n,s;return n=new L({props:{$$slots:{default:[B]},$$scope:{ctx:c}}}),{c(){u(n.$$.fragment)},l(e){m(n.$$.fragment,e)},m(e,a){_(n,e,a),s=!0},p(e,a){const r={};a&1&&(r.$$scope={dirty:a,ctx:e}),n.$set(r)},i(e){s||(d(n.$$.fragment,e),s=!0)},o(e){g(n.$$.fragment,e),s=!1},d(e){h(n,e)}}}function G(c){let n,s,e,a,r,f,i,$;return n=new T({props:{title:"Client Credentials"}}),e=new A({props:{title:"Introduction",$$slots:{default:[U]},$$scope:{ctx:c}}}),r=new A({props:{title:"Specifications",$$slots:{default:[j]},$$scope:{ctx:c}}}),i=new A({props:{title:"Client Credentials",$$slots:{default:[F]},$$scope:{ctx:c}}}),{c(){u(n.$$.fragment),s=y(),u(e.$$.fragment),a=y(),u(r.$$.fragment),f=y(),u(i.$$.fragment)},l(t){m(n.$$.fragment,t),s=q(t),m(e.$$.fragment,t),a=q(t),m(r.$$.fragment,t),f=q(t),m(i.$$.fragment,t)},m(t,o){_(n,t,o),p(t,s,o),_(e,t,o),p(t,a,o),_(r,t,o),p(t,f,o),_(i,t,o),$=!0},p(t,[o]){const R={};o&1&&(R.$$scope={dirty:o,ctx:t}),e.$set(R);const E={};o&1&&(E.$$scope={dirty:o,ctx:t}),r.$set(E);const I={};o&1&&(I.$$scope={dirty:o,ctx:t}),i.$set(I)},i(t){$||(d(n.$$.fragment,t),d(e.$$.fragment,t),d(r.$$.fragment,t),d(i.$$.fragment,t),$=!0)},o(t){g(n.$$.fragment,t),g(e.$$.fragment,t),g(r.$$.fragment,t),g(i.$$.fragment,t),$=!1},d(t){h(n,t),t&&l(s),h(e,t),t&&l(a),h(r,t),t&&l(f),h(i,t)}}}class N extends x{constructor(n){super(),b(this,n,null,G,z,{})}}export{N as default};
